"""Delegate a focused task to Grok 4.5 from Codex or a terminal.

The command intentionally returns text/diffs instead of writing files. The
orchestrating Codex agent can review the result and apply only the approved
change to the workspace.
"""

from __future__ import annotations

import argparse
import json
import os
import sys
from pathlib import Path
from typing import Any, Dict, List

from grok_client import GrokApiError, GrokClient, GrokConfig, message_text


DEFAULT_SYSTEM = """あなたはCodexから委譲された、慎重な実装サブエージェントです。
与えられた作業範囲とファイル内容だけを根拠に調査してください。
不明なことは推測で埋めず、前提として明示してください。
実装を求められた場合は、変更理由と、適用可能な unified diff を返してください。
ファイルを直接変更したとは主張しないでください。セキュリティ上の秘密やAPIキーを出力しないでください。
"""

IGNORED_DIRS = {"Library", "Temp", "Logs", "Build", "Builds", "obj", ".git"}
SECRET_NAMES = {".env", ".env.local", "credentials.json", "secrets.json"}
MAX_FILE_BYTES = 200_000


def main() -> int:
    parser = argparse.ArgumentParser(description="Ask Grok 4.5 to act as a focused Codex sub-agent.")
    parser.add_argument("-p", "--prompt", help="Task prompt. If omitted, read stdin.")
    parser.add_argument("--file", action="append", default=[], help="Workspace file to include; repeatable.")
    parser.add_argument("--system", default=DEFAULT_SYSTEM, help="Override the system instruction.")
    parser.add_argument("--mode", choices=("analyze", "implement", "review"), default="analyze")
    parser.add_argument("--conversation-id", help="Stable ID used by xAI prompt-cache routing.")
    parser.add_argument("--reasoning-effort", choices=("low", "medium", "high"))
    parser.add_argument("--json", action="store_true", help="Print the complete API response as JSON.")
    args = parser.parse_args()

    prompt = args.prompt if args.prompt is not None else sys.stdin.read().strip()
    if not prompt:
        parser.error("--prompt or stdin input is required")

    root = Path.cwd().resolve()
    context = build_context(root, args.file)
    mode_instruction = {
        "analyze": "分析結果、リスク、確認すべき点を簡潔に返してください。",
        "implement": "最小変更の unified diff と検証コマンドを返してください。",
        "review": "問題点を優先度付きで列挙し、修正案を返してください。",
    }[args.mode]
    user_content = f"作業モード: {args.mode}\n{mode_instruction}\n\n依頼:\n{prompt}"
    if context:
        user_content += f"\n\n--- workspace context ---\n{context}\n--- end workspace context ---"

    try:
        client = GrokClient(GrokConfig.from_environment())
        payload = client.chat(
            [
                {"role": "system", "content": args.system},
                {"role": "user", "content": user_content},
            ],
            conversation_id=args.conversation_id,
            reasoning_effort=args.reasoning_effort,
        )
        if args.json:
            print(json.dumps(payload, ensure_ascii=False, indent=2))
        else:
            print(message_text(payload))
        return 0
    except GrokApiError as error:
        print(f"grok-agent: {error}", file=sys.stderr)
        return 1


def build_context(root: Path, paths: List[str]) -> str:
    chunks: List[str] = []
    total = 0
    for raw_path in paths:
        path = (root / raw_path).resolve()
        if not is_safe_context_path(root, path):
            raise GrokApiError(f"Refusing to include unsafe or ignored path: {raw_path}")
        if not path.is_file():
            raise GrokApiError(f"Context file does not exist: {raw_path}")
        if path.stat().st_size > MAX_FILE_BYTES:
            raise GrokApiError(f"Context file is larger than {MAX_FILE_BYTES} bytes: {raw_path}")
        try:
            text = path.read_text(encoding="utf-8")
        except UnicodeDecodeError as error:
            raise GrokApiError(f"Context file is not UTF-8 text: {raw_path}") from error
        addition = f"\n\n### {path.relative_to(root)}\n{text}"
        total += len(addition.encode("utf-8"))
        if total > 800_000:
            raise GrokApiError("Combined context is larger than 800000 bytes")
        chunks.append(addition)
    return "".join(chunks)


def is_safe_context_path(root: Path, path: Path) -> bool:
    try:
        relative = path.relative_to(root)
    except ValueError:
        return False
    if any(part in IGNORED_DIRS for part in relative.parts):
        return False
    if path.name in SECRET_NAMES or path.suffix.lower() in {".key", ".pem", ".p12"}:
        return False
    return True


if __name__ == "__main__":
    try:
        raise SystemExit(main())
    except GrokApiError as error:
        print(f"grok-agent: {error}", file=sys.stderr)
        raise SystemExit(1)
