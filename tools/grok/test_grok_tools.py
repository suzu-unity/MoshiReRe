import json
import os
import sys
import tempfile
import unittest
from pathlib import Path
from unittest.mock import patch

sys.path.insert(0, str(Path(__file__).parent))

from grok_client import GrokApiError, GrokConfig, message_text
from grok_agent import build_context, is_safe_context_path
from web_server import ConversationStore, content_with_files, public_conversation, validate_attachments


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

    def test_attachment_validation_and_context_format(self):
        files = validate_attachments([{"name": "Example.cs", "content": "class Example {}"}])
        self.assertIn("class Example", content_with_files("Review this", files))
        with self.assertRaises(GrokApiError):
            validate_attachments([{"name": ".env", "content": "XAI_API_KEY=secret"}])

    def test_conversation_store_persists_and_hides_attachment_contents(self):
        with tempfile.TemporaryDirectory() as directory:
            store = ConversationStore(Path(directory))
            conversation = store.create()
            conversation["messages"].append({
                "role": "user",
                "content": "Review this",
                "files": [{"name": "Example.cs", "content": "secret source"}],
            })
            store.save(conversation)
            loaded = store.get(conversation["id"])
            public = public_conversation(loaded)
            self.assertEqual(public["messages"][0]["files"], ["Example.cs"])
            self.assertNotIn("secret source", json.dumps(public, ensure_ascii=False))


if __name__ == "__main__":
    unittest.main()
