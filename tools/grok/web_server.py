"""Local-only, persistent WebUI server for Grok chat and Imagine agent tools."""

from __future__ import annotations

import base64
import argparse
import json
import os
import re
import threading
import uuid
from datetime import datetime, timezone
from http import HTTPStatus
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from typing import Any, Dict, List, Optional, Tuple
from urllib.parse import unquote, urlparse

from grok_client import GrokApiError, GrokClient, GrokConfig, message_text
from grok_media import XaiMediaClient


def _positive_env(name: str, fallback: int) -> int:
    raw = os.environ.get(name)
    if not raw:
        return fallback
    try:
        value = int(raw)
    except ValueError as error:
        raise RuntimeError(f"{name} must be a positive integer") from error
    if value <= 0:
        raise RuntimeError(f"{name} must be a positive integer")
    return value


ROOT = Path(__file__).resolve().parent
WEB_ROOT = ROOT / "web"
DATA_ROOT = ROOT / "data" / "conversations"
MAX_BODY_BYTES = _positive_env("GROK_MAX_REQUEST_BYTES", 64 * 1024 * 1024)
MAX_FILE_BYTES = _positive_env("GROK_MAX_FILE_BYTES", 25 * 1024 * 1024)
MAX_TOTAL_FILE_BYTES = _positive_env("GROK_MAX_TOTAL_FILE_BYTES", 50 * 1024 * 1024)
MAX_MESSAGE_BYTES = _positive_env("GROK_MAX_MESSAGE_BYTES", 2 * 1024 * 1024)
ALLOWED_ROLES = {"system", "user", "assistant"}
CONVERSATION_ID_RE = re.compile(r"^[0-9a-f-]{36}$", re.IGNORECASE)
SECRET_NAMES = {".env", ".env.local", "credentials.json", "secrets.json"}
IMAGE_EXTENSIONS = {".png", ".jpg", ".jpeg", ".webp", ".gif"}
MAX_AGENT_STEPS = 4

SYSTEM_PROMPT = """You are the local MoshiReRe development assistant.
Use only the conversation and attached files as evidence. Do not claim that you edited local files.
For code edits, return a minimal unified diff and verification steps. Never reveal API keys or secrets.
When agent tools are available, use them for image or video creation requests instead of merely describing how to do it.
The configured chat model is grok-4.5. Image and video generation use xAI Imagine models.
"""

MODE_INSTRUCTIONS = {
    "consult": "相談モード。内容を説明し、質問に答えてください。",
    "review": "レビュー mode。問題点を優先度付きで示し、改善案を提示してください。",
    "implement": "編集案モード。最小変更の unified diff と検証コマンドを提示してください。直接ファイルを書き換えないでください。",
    "agent": "Agentモード。必要に応じて利用可能な画像・動画生成ツールを実行し、結果を説明してください。",
}

AGENT_TOOLS: List[Dict[str, Any]] = [
    {
        "type": "function",
        "function": {
            "name": "generate_image",
            "description": "Generate an image with xAI Imagine when the user asks to create or edit a visual.",
            "parameters": {
                "type": "object",
                "properties": {
                    "prompt": {"type": "string", "description": "Detailed image generation prompt."},
                    "resolution": {"type": "string", "enum": ["1k", "2k"]},
                    "n": {"type": "integer", "minimum": 1, "maximum": 4},
                },
                "required": ["prompt"],
            },
        },
    },
    {
        "type": "function",
        "function": {
            "name": "generate_video",
            "description": "Generate a video with xAI Imagine when the user asks to create or animate a video.",
            "parameters": {
                "type": "object",
                "properties": {
                    "prompt": {"type": "string", "description": "Detailed video generation prompt."},
                    "duration": {"type": "integer", "minimum": 2, "maximum": 15},
                    "aspect_ratio": {"type": "string", "enum": ["16:9", "9:16", "1:1"]},
                    "resolution": {"type": "string", "enum": ["480p", "720p"]},
                    "use_first_attached_image": {"type": "boolean", "description": "Use the first attached image as the video source."},
                },
                "required": ["prompt"],
            },
        },
    },
]


def now_iso() -> str:
    return datetime.now(timezone.utc).isoformat()


class ConversationStore:
    """Small JSON-backed store. Each conversation is one local file."""

    def __init__(self, root: Path = DATA_ROOT):
        self.root = root
        self.root.mkdir(parents=True, exist_ok=True)
        self._lock = threading.RLock()

    def create(self, title: str = "新しい会話") -> Dict[str, Any]:
        conversation = {
            "id": str(uuid.uuid4()),
            "title": clean_title(title),
            "created_at": now_iso(),
            "updated_at": now_iso(),
            "messages": [],
        }
        with self._lock:
            self._write(conversation)
        return conversation

    def list(self) -> List[Dict[str, Any]]:
        with self._lock:
            conversations = []
            for path in self.root.glob("*.json"):
                try:
                    conversations.append(self._read_path(path))
                except (OSError, json.JSONDecodeError, KeyError, ValueError):
                    continue
        conversations.sort(key=lambda item: item.get("updated_at", ""), reverse=True)
        return conversations

    def get(self, conversation_id: str) -> Dict[str, Any]:
        path = self._path(conversation_id)
        with self._lock:
            if not path.exists():
                raise KeyError(conversation_id)
            return self._read_path(path)

    def save(self, conversation: Dict[str, Any]) -> Dict[str, Any]:
        conversation["updated_at"] = now_iso()
        with self._lock:
            self._write(conversation)
        return conversation

    def rename(self, conversation_id: str, title: str) -> Dict[str, Any]:
        conversation = self.get(conversation_id)
        conversation["title"] = clean_title(title)
        return self.save(conversation)

    def delete(self, conversation_id: str) -> None:
        path = self._path(conversation_id)
        with self._lock:
            if not path.exists():
                raise KeyError(conversation_id)
            path.unlink()

    def _path(self, conversation_id: str) -> Path:
        if not CONVERSATION_ID_RE.fullmatch(conversation_id):
            raise KeyError(conversation_id)
        return self.root / f"{conversation_id}.json"

    def _read_path(self, path: Path) -> Dict[str, Any]:
        conversation = json.loads(path.read_text(encoding="utf-8"))
        if not isinstance(conversation, dict) or not isinstance(conversation.get("messages"), list):
            raise ValueError("invalid conversation")
        return conversation

    def _write(self, conversation: Dict[str, Any]) -> None:
        path = self._path(conversation["id"])
        temp_path = path.with_suffix(".json.tmp")
        temp_path.write_text(json.dumps(conversation, ensure_ascii=False, indent=2), encoding="utf-8")
        temp_path.replace(path)


def clean_title(value: Any) -> str:
    title = str(value or "新しい会話").strip().replace("\r", " ").replace("\n", " ")
    return title[:80] or "新しい会話"


def attachment_kind(item: Dict[str, Any]) -> str:
    if item.get("kind") in {"text", "image"}:
        return item["kind"]
    mime = str(item.get("mime", ""))
    name = str(item.get("name", ""))
    return "image" if mime.startswith("image/") or Path(name).suffix.lower() in IMAGE_EXTENSIONS else "text"


def public_file(item: Dict[str, Any]) -> Dict[str, str]:
    return {
        "name": str(item.get("name", "")),
        "kind": attachment_kind(item),
        "mime": str(item.get("mime", "")),
    }


def public_conversation(conversation: Dict[str, Any]) -> Dict[str, Any]:
    messages = []
    for message in conversation.get("messages", []):
        messages.append(
            {
                "role": message.get("role"),
                "content": message.get("content", ""),
                "mode": message.get("mode"),
                "created_at": message.get("created_at"),
                "files": [public_file(item) for item in message.get("files", [])],
                "tools": message.get("tools", []),
                "artifacts": message.get("artifacts", []),
            }
        )
    return {
        "id": conversation["id"],
        "title": conversation.get("title", "新しい会話"),
        "created_at": conversation.get("created_at"),
        "updated_at": conversation.get("updated_at"),
        "messages": messages,
    }


def public_metadata(conversation: Dict[str, Any]) -> Dict[str, Any]:
    return {
        "id": conversation["id"],
        "title": conversation.get("title", "新しい会話"),
        "created_at": conversation.get("created_at"),
        "updated_at": conversation.get("updated_at"),
        "message_count": len(conversation.get("messages", [])),
    }


def validate_attachments(raw: Any) -> List[Dict[str, str]]:
    if raw is None:
        return []
    if not isinstance(raw, list) or len(raw) > 10:
        raise GrokApiError("添付ファイルは最大10個までです")

    attachments: List[Dict[str, str]] = []
    total = 0
    for item in raw:
        if not isinstance(item, dict):
            raise GrokApiError("添付ファイルの形式が不正です")
        name = str(item.get("name", "")).strip()
        mime = str(item.get("mime", "")).strip().lower()
        kind = attachment_kind(item)
        content = item.get("content")
        if not name or len(name) > 200 or "/" in name or "\\" in name:
            raise GrokApiError("添付ファイル名が不正です")
        if name.lower() in SECRET_NAMES:
            raise GrokApiError("秘密ファイルは添付できません")
        if not isinstance(content, str) or not content:
            raise GrokApiError(f"添付ファイルの内容が空か不正です: {name}")

        if kind == "image":
            if not content.startswith("data:image/") or "," not in content:
                raise GrokApiError(f"画像はdata URL形式で送信してください: {name}")
            try:
                base64.b64decode(content.split(",", 1)[1], validate=True)
            except (ValueError, base64.binascii.Error) as error:
                raise GrokApiError(f"画像データが不正です: {name}") from error
        size = len(content.encode("utf-8"))
        if size > MAX_FILE_BYTES:
            raise GrokApiError(f"添付ファイルが大きすぎます（現在の上限: {MAX_FILE_BYTES // (1024 * 1024)}MB）: {name}")
        total += size
        if total > MAX_TOTAL_FILE_BYTES:
            raise GrokApiError(f"1回の添付ファイル合計が大きすぎます（現在の上限: {MAX_TOTAL_FILE_BYTES // (1024 * 1024)}MB）")
        attachments.append({"name": name, "mime": mime, "kind": kind, "content": content})
    return attachments


def content_with_files(content: str, files: List[Dict[str, str]]) -> Any:
    text_parts = [content]
    image_parts: List[Dict[str, Any]] = []
    for file in files:
        if attachment_kind(file) == "image":
            image_parts.append({"type": "image_url", "image_url": {"url": file["content"]}})
        else:
            text_parts.extend(["", f"--- 添付テキスト: {file['name']} ---", file["content"]])
    text = "\n".join(text_parts)
    if not image_parts:
        return text
    return [{"type": "text", "text": text}, *image_parts]


def api_messages(conversation: Dict[str, Any], current_content: str, mode: str, files: List[Dict[str, str]]) -> List[Dict[str, Any]]:
    messages: List[Dict[str, Any]] = [{"role": "system", "content": SYSTEM_PROMPT}]
    for message in conversation.get("messages", []):
        role = message.get("role")
        if role not in {"user", "assistant"}:
            continue
        message_files = message.get("files", []) if role == "user" else []
        messages.append({"role": role, "content": content_with_files(message.get("content", ""), message_files)})
    current = f"作業モード: {mode}\n{MODE_INSTRUCTIONS[mode]}\n\n{content_with_files(current_content, files)}"
    messages.append({"role": "user", "content": current})
    return messages


def run_agent(
    client: GrokClient,
    messages: List[Dict[str, Any]],
    *,
    conversation_id: str,
    reasoning_effort: Optional[str],
    files: List[Dict[str, str]],
) -> Tuple[Dict[str, Any], List[str], List[Dict[str, Any]]]:
    working = list(messages)
    tools_used: List[str] = []
    artifacts: List[Dict[str, Any]] = []
    media = XaiMediaClient(client.config)
    for _ in range(MAX_AGENT_STEPS):
        payload = client.chat(
            working,
            conversation_id=conversation_id,
            reasoning_effort=reasoning_effort,
            tools=AGENT_TOOLS,
            tool_choice="auto",
        )
        assistant_message = payload.get("choices", [{}])[0].get("message", {})
        tool_calls = assistant_message.get("tool_calls", []) if isinstance(assistant_message, dict) else []
        if not tool_calls:
            return payload, tools_used, artifacts
        working.append(assistant_message)
        for tool_call in tool_calls:
            result, tool_artifacts = execute_agent_tool(tool_call, media, files)
            tool_name = tool_call.get("function", {}).get("name", "unknown")
            tools_used.append(tool_name)
            artifacts.extend(tool_artifacts)
            working.append(
                {
                    "role": "tool",
                    "tool_call_id": tool_call.get("id", ""),
                    "content": json.dumps(result, ensure_ascii=False),
                }
            )
    raise GrokApiError("Agent tool loop exceeded the safety limit")


def execute_agent_tool(
    tool_call: Dict[str, Any], media: XaiMediaClient, files: List[Dict[str, str]]
) -> Tuple[Dict[str, Any], List[Dict[str, Any]]]:
    function = tool_call.get("function", {})
    name = function.get("name")
    try:
        arguments = json.loads(function.get("arguments", "{}"))
    except json.JSONDecodeError as error:
        raise GrokApiError(f"Agent tool arguments were invalid: {error}") from error
    if not isinstance(arguments, dict):
        raise GrokApiError("Agent tool arguments must be an object")

    if name == "generate_image":
        result = media.generate_image(
            str(arguments.get("prompt", "")),
            resolution=str(arguments.get("resolution", "1k")),
            n=int(arguments.get("n", 1)),
        )
        artifacts = image_artifacts(result)
        return {"ok": True, "artifacts": artifacts}, artifacts
    if name == "generate_video":
        image_data_url = None
        if arguments.get("use_first_attached_image"):
            image_data_url = next((item["content"] for item in files if attachment_kind(item) == "image"), None)
        result = media.generate_video(
            str(arguments.get("prompt", "")),
            duration=int(arguments.get("duration", 5)),
            aspect_ratio=str(arguments.get("aspect_ratio", "16:9")),
            resolution=str(arguments.get("resolution", "720p")),
            image_data_url=image_data_url,
        )
        artifacts = video_artifacts(result)
        return {"ok": True, "artifacts": artifacts}, artifacts
    raise GrokApiError(f"Unknown agent tool: {name}")


def image_artifacts(payload: Dict[str, Any]) -> List[Dict[str, Any]]:
    artifacts = []
    for item in payload.get("data", []):
        if item.get("url"):
            artifacts.append({"type": "image", "url": item["url"]})
        elif item.get("b64_json"):
            artifacts.append({"type": "image", "url": f"data:image/png;base64,{item['b64_json']}"})
    return artifacts


def video_artifacts(payload: Dict[str, Any]) -> List[Dict[str, Any]]:
    video = payload.get("video", {})
    return [{"type": "video", "url": video["url"]}] if video.get("url") else []


class WebHandler(BaseHTTPRequestHandler):
    server_version = "MoshiReReGrokWeb/3.0"

    @property
    def store(self) -> ConversationStore:
        return self.server.store  # type: ignore[attr-defined]

    def do_GET(self) -> None:
        path = urlparse(self.path).path
        if path in {"/", "/index.html"}:
            self._send_file(WEB_ROOT / "index.html", "text/html; charset=utf-8")
            return
        if path == "/api/conversations":
            self._send_json({"conversations": [public_metadata(item) for item in self.store.list()]})
            return
        conversation_id = conversation_id_from_path(path, "/api/conversations/")
        if conversation_id:
            try:
                self._send_json(public_conversation(self.store.get(conversation_id)))
            except KeyError:
                self._send_json({"error": "会話が見つかりません"}, HTTPStatus.NOT_FOUND)
            return
        self.send_error(HTTPStatus.NOT_FOUND)

    def do_POST(self) -> None:
        path = urlparse(self.path).path
        try:
            request = self._read_json()
            if path == "/api/conversations":
                conversation = self.store.create(request.get("title", "新しい会話"))
                self._send_json(public_conversation(conversation), HTTPStatus.CREATED)
                return
            if path == "/api/chat":
                self._chat(request)
                return
            self.send_error(HTTPStatus.NOT_FOUND)
        except GrokApiError as error:
            self._send_json({"ok": False, "error": str(error)}, HTTPStatus.BAD_REQUEST)
        except KeyError:
            self._send_json({"ok": False, "error": "会話が見つかりません"}, HTTPStatus.NOT_FOUND)
        except Exception as error:
            self.log_error("request failed: %s", error)
            self._send_json({"ok": False, "error": "Unexpected server error"}, HTTPStatus.INTERNAL_SERVER_ERROR)

    def do_PATCH(self) -> None:
        conversation_id = conversation_id_from_path(urlparse(self.path).path, "/api/conversations/")
        if not conversation_id:
            self.send_error(HTTPStatus.NOT_FOUND)
            return
        try:
            request = self._read_json()
            conversation = self.store.rename(conversation_id, request.get("title", ""))
            self._send_json(public_conversation(conversation))
        except KeyError:
            self._send_json({"error": "会話が見つかりません"}, HTTPStatus.NOT_FOUND)
        except GrokApiError as error:
            self._send_json({"error": str(error)}, HTTPStatus.BAD_REQUEST)

    def do_DELETE(self) -> None:
        conversation_id = conversation_id_from_path(urlparse(self.path).path, "/api/conversations/")
        if not conversation_id:
            self.send_error(HTTPStatus.NOT_FOUND)
            return
        try:
            self.store.delete(conversation_id)
            self._send_json({"ok": True})
        except KeyError:
            self._send_json({"error": "会話が見つかりません"}, HTTPStatus.NOT_FOUND)

    def _chat(self, request: Dict[str, Any]) -> None:
        conversation_id = request.get("conversation_id")
        content = request.get("content")
        mode = request.get("mode", "consult")
        if not isinstance(conversation_id, str) or not CONVERSATION_ID_RE.fullmatch(conversation_id):
            raise GrokApiError("conversation_idが不正です")
        if not isinstance(content, str) or not content.strip():
            raise GrokApiError("メッセージを入力してください")
        if len(content.encode("utf-8")) > MAX_MESSAGE_BYTES:
            raise GrokApiError("メッセージが大きすぎます")
        if mode not in MODE_INSTRUCTIONS:
            raise GrokApiError("modeが不正です")
        files = validate_attachments(request.get("files"))
        conversation = self.store.get(conversation_id)
        messages = api_messages(conversation, content, mode, files)
        client = GrokClient(GrokConfig.from_environment())
        if mode == "agent":
            payload, tools_used, artifacts = run_agent(
                client,
                messages,
                conversation_id=conversation_id,
                reasoning_effort=request.get("reasoning_effort"),
                files=files,
            )
        else:
            payload = client.chat(
                messages,
                conversation_id=conversation_id,
                reasoning_effort=request.get("reasoning_effort"),
            )
            tools_used, artifacts = [], []
        assistant_text = message_text(payload)
        conversation["messages"].append(
            {
                "role": "user",
                "content": content,
                "mode": mode,
                "files": files,
                "created_at": now_iso(),
            }
        )
        conversation["messages"].append(
            {
                "role": "assistant",
                "content": assistant_text,
                "files": [],
                "tools": tools_used,
                "artifacts": artifacts,
                "created_at": now_iso(),
            }
        )
        if conversation.get("title") == "新しい会話":
            conversation["title"] = clean_title(content)
        saved = self.store.save(conversation)
        self._send_json(
            {
                "ok": True,
                "text": assistant_text,
                "model": payload.get("model"),
                "tools": tools_used,
                "artifacts": artifacts,
                "conversation": public_metadata(saved),
            }
        )

    def _read_json(self) -> Dict[str, Any]:
        try:
            length = int(self.headers.get("Content-Length", "0"))
        except ValueError as error:
            raise GrokApiError("Content-Lengthが不正です") from error
        if length <= 0 or length > MAX_BODY_BYTES:
            raise GrokApiError(f"リクエストが大きすぎます（現在の上限: {MAX_BODY_BYTES // (1024 * 1024)}MB）")
        try:
            payload = json.loads(self.rfile.read(length).decode("utf-8"))
        except (UnicodeDecodeError, json.JSONDecodeError) as error:
            raise GrokApiError("Request body must be valid UTF-8 JSON") from error
        if not isinstance(payload, dict):
            raise GrokApiError("Request body must be a JSON object")
        return payload

    def _send_file(self, path: Path, content_type: str) -> None:
        try:
            data = path.read_bytes()
        except FileNotFoundError:
            self.send_error(HTTPStatus.NOT_FOUND)
            return
        self.send_response(HTTPStatus.OK)
        self.send_header("Content-Type", content_type)
        self.send_header("Content-Length", str(len(data)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(data)

    def _send_json(self, payload: Dict[str, Any], status: HTTPStatus = HTTPStatus.OK) -> None:
        data = json.dumps(payload, ensure_ascii=False).encode("utf-8")
        self.send_response(status)
        self.send_header("Content-Type", "application/json; charset=utf-8")
        self.send_header("Content-Length", str(len(data)))
        self.send_header("Cache-Control", "no-store")
        self.end_headers()
        self.wfile.write(data)


def conversation_id_from_path(path: str, prefix: str) -> Optional[str]:
    if not path.startswith(prefix):
        return None
    value = unquote(path[len(prefix):]).strip("/")
    if "/" in value or not CONVERSATION_ID_RE.fullmatch(value):
        return None
    return value


def main() -> int:
    parser = argparse.ArgumentParser(description="Run the local persistent Grok 4.5 WebUI.")
    parser.add_argument("--host", default="127.0.0.1", help="Bind address; localhost is the safe default.")
    parser.add_argument("--port", type=int, default=8787)
    args = parser.parse_args()
    if not (1 <= args.port <= 65535):
        parser.error("--port must be between 1 and 65535")
    server = ThreadingHTTPServer((args.host, args.port), WebHandler)
    server.store = ConversationStore()  # type: ignore[attr-defined]
    print(f"Grok WebUI: http://{args.host}:{args.port}")
    print(f"Conversation data: {DATA_ROOT}")
    print(f"Upload limits: {MAX_FILE_BYTES // (1024 * 1024)}MB/file, {MAX_TOTAL_FILE_BYTES // (1024 * 1024)}MB/request")
    print("API key is read server-side from XAI_API_KEY; press Ctrl+C to stop.")
    try:
        server.serve_forever()
    except KeyboardInterrupt:
        print("\nStopping.")
    finally:
        server.server_close()
    return 0


if __name__ == "__main__":
    raise SystemExit(main())
