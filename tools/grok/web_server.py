"""Local-only WebUI server for chatting with Grok 4.5."""

from __future__ import annotations

import argparse
import json
from http import HTTPStatus
from http.server import BaseHTTPRequestHandler, ThreadingHTTPServer
from pathlib import Path
from typing import Any, Dict, List

from grok_client import GrokApiError, GrokClient, GrokConfig, message_text


ROOT = Path(__file__).resolve().parent
WEB_ROOT = ROOT / "web"
MAX_BODY_BYTES = 1_000_000
ALLOWED_ROLES = {"system", "user", "assistant"}


class WebHandler(BaseHTTPRequestHandler):
    server_version = "MoshiReReGrokWeb/1.0"

    def do_GET(self) -> None:
        if self.path in {"/", "/index.html"}:
            self._send_file(WEB_ROOT / "index.html", "text/html; charset=utf-8")
            return
        self.send_error(HTTPStatus.NOT_FOUND)

    def do_POST(self) -> None:
        if self.path != "/api/chat":
            self.send_error(HTTPStatus.NOT_FOUND)
            return
        try:
            request = self._read_json()
            messages = validate_messages(request.get("messages"))
            conversation_id = request.get("conversation_id")
            if conversation_id is not None and not isinstance(conversation_id, str):
                raise GrokApiError("conversation_id must be a string")
            effort = request.get("reasoning_effort")
            if effort is not None and effort not in {"low", "medium", "high"}:
                raise GrokApiError("reasoning_effort must be low, medium, or high")

            payload = GrokClient(GrokConfig.from_environment()).chat(
                messages,
                conversation_id=conversation_id,
                reasoning_effort=effort,
            )
            self._send_json({"ok": True, "text": message_text(payload), "model": payload.get("model")})
        except GrokApiError as error:
            self._send_json({"ok": False, "error": str(error)}, HTTPStatus.BAD_REQUEST)
        except Exception as error:  # Keep internal details out of the browser response.
            self.log_error("request failed: %s", error)
            self._send_json({"ok": False, "error": "Unexpected server error"}, HTTPStatus.INTERNAL_SERVER_ERROR)

    def log_message(self, format: str, *args: Any) -> None:
        # Useful access log, but never log request bodies or authorization data.
        super().log_message(format, *args)

    def _read_json(self) -> Dict[str, Any]:
        length = int(self.headers.get("Content-Length", "0"))
        if length <= 0 or length > MAX_BODY_BYTES:
            raise GrokApiError("Request body must be between 1 byte and 1 MB")
        raw = self.rfile.read(length)
        try:
            payload = json.loads(raw.decode("utf-8"))
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


def validate_messages(raw: Any) -> List[Dict[str, str]]:
    if not isinstance(raw, list) or not raw:
        raise GrokApiError("messages must be a non-empty array")
    if len(raw) > 100:
        raise GrokApiError("A maximum of 100 messages is allowed")
    messages: List[Dict[str, str]] = []
    for item in raw:
        if not isinstance(item, dict) or item.get("role") not in ALLOWED_ROLES:
            raise GrokApiError("Each message must have role system, user, or assistant")
        content = item.get("content")
        if not isinstance(content, str) or not content.strip():
            raise GrokApiError("Each message must have non-empty string content")
        if len(content) > 200_000:
            raise GrokApiError("A message is larger than 200000 characters")
        messages.append({"role": item["role"], "content": content})
    return messages


def main() -> int:
    parser = argparse.ArgumentParser(description="Run the local Grok 4.5 WebUI.")
    parser.add_argument("--host", default="127.0.0.1", help="Bind address; localhost is the safe default.")
    parser.add_argument("--port", type=int, default=8787)
    args = parser.parse_args()
    if not (1 <= args.port <= 65535):
        parser.error("--port must be between 1 and 65535")
    server = ThreadingHTTPServer((args.host, args.port), WebHandler)
    print(f"Grok WebUI: http://{args.host}:{args.port}")
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
