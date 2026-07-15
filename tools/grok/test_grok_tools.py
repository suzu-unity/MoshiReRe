import json
import os
import sys
import unittest
from pathlib import Path
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).parent))

from grok_client import GrokApiError, GrokConfig, message_text
from grok_agent import build_context, is_safe_context_path
from web_server import validate_messages


class GrokToolsTests(unittest.TestCase):
    def test_config_requires_key(self):
        with patch.dict(os.environ, {}, clear=True):
            with self.assertRaises(GrokApiError):
                GrokConfig.from_environment()

    def test_message_text_supports_content_parts(self):
        payload = {"choices": [{"message": {"content": [{"text": "A"}, {"text": "B"}]}}]}
        self.assertEqual(message_text(payload), "AB")

    def test_context_rejects_ignored_and_secret_paths(self):
        root = Path.cwd().resolve()
        self.assertFalse(is_safe_context_path(root, root / "Library" / "x.txt"))
        self.assertFalse(is_safe_context_path(root, root / ".env"))

    def test_context_reads_explicit_text_file(self):
        root = Path.cwd().resolve()
        test_file = root / "tools" / "grok" / "_test_context.txt"
        test_file.write_text("hello", encoding="utf-8")
        try:
            self.assertIn("hello", build_context(root, [str(test_file.relative_to(root))]))
        finally:
            test_file.unlink(missing_ok=True)

    def test_web_messages_are_normalized(self):
        result = validate_messages([{"role": "user", "content": " hello "}])
        self.assertEqual(result, [{"role": "user", "content": " hello "}])
        with self.assertRaises(GrokApiError):
            validate_messages([{"role": "tool", "content": "not allowed"}])


if __name__ == "__main__":
    unittest.main()
