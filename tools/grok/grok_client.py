"""Small dependency-free xAI client used by the CLI and local WebUI."""

from __future__ import annotations

import json
import os
import urllib.error
import urllib.request
from dataclasses import dataclass
from typing import Any, Dict, List, Optional


class GrokApiError(RuntimeError):
    """Raised when the xAI API returns an error or an unexpected payload."""


@dataclass(frozen=True)
class GrokConfig:
    api_key: str
    base_url: str = "https://api.x.ai/v1"
    model: str = "grok-4.5"
    timeout_seconds: int = 120
    reasoning_effort: Optional[str] = None
    max_output_tokens: Optional[int] = None

    @classmethod
    def from_environment(cls) -> "GrokConfig":
        api_key = os.environ.get("XAI_API_KEY", "").strip()
        if not api_key:
            raise GrokApiError(
                "XAI_API_KEY is not set. Set it in the process environment; "
                "do not put the key in source files or the browser."
            )

        timeout = _positive_int(os.environ.get("XAI_TIMEOUT_SECONDS"), 120)
        max_output = _optional_positive_int(os.environ.get("XAI_MAX_OUTPUT_TOKENS"))
        reasoning = os.environ.get("XAI_REASONING_EFFORT", "").strip() or None
        if reasoning and reasoning not in {"low", "medium", "high"}:
            raise GrokApiError("XAI_REASONING_EFFORT must be low, medium, or high")

        return cls(
            api_key=api_key,
            base_url=os.environ.get("XAI_BASE_URL", cls.base_url).rstrip("/"),
            model=os.environ.get("XAI_MODEL", cls.model).strip() or cls.model,
            timeout_seconds=timeout,
            reasoning_effort=reasoning,
            max_output_tokens=max_output,
        )


class GrokClient:
    def __init__(self, config: GrokConfig):
        self.config = config

    def chat(
        self,
        messages: List[Dict[str, Any]],
        *,
        conversation_id: Optional[str] = None,
        reasoning_effort: Optional[str] = None,
        tools: Optional[List[Dict[str, Any]]] = None,
        tool_choice: Optional[Any] = None,
    ) -> Dict[str, Any]:
        if not messages:
            raise GrokApiError("At least one message is required")

        body: Dict[str, Any] = {
            "model": self.config.model,
            "messages": messages,
        }
        effort = reasoning_effort or self.config.reasoning_effort
        if effort:
            if effort not in {"low", "medium", "high"}:
                raise GrokApiError("reasoning_effort must be low, medium, or high")
            body["reasoning_effort"] = effort
        if self.config.max_output_tokens is not None:
            body["max_tokens"] = self.config.max_output_tokens
        if tools:
            body["tools"] = tools
        if tool_choice is not None:
            body["tool_choice"] = tool_choice

        headers = {
            "Authorization": f"Bearer {self.config.api_key}",
            "Content-Type": "application/json",
            "Accept": "application/json",
            "User-Agent": "MoshiReRe-GrokTools/1.0",
        }
        if conversation_id:
            # xAI documents this header for reliable prompt-cache routing.
            headers["x-grok-conv-id"] = conversation_id

        request = urllib.request.Request(
            f"{self.config.base_url}/chat/completions",
            data=json.dumps(body, ensure_ascii=False).encode("utf-8"),
            headers=headers,
            method="POST",
        )

        try:
            with urllib.request.urlopen(request, timeout=self.config.timeout_seconds) as response:
                raw = response.read().decode("utf-8")
        except urllib.error.HTTPError as error:
            detail = error.read().decode("utf-8", errors="replace")
            raise GrokApiError(f"xAI API returned HTTP {error.code}: {detail[:1000]}") from error
        except urllib.error.URLError as error:
            raise GrokApiError(f"Could not reach xAI API: {error.reason}") from error

        try:
            payload = json.loads(raw)
        except json.JSONDecodeError as error:
            raise GrokApiError("xAI API returned invalid JSON") from error

        if not isinstance(payload, dict):
            raise GrokApiError("xAI API returned an unexpected response")
        if payload.get("error"):
            raise GrokApiError(f"xAI API error: {payload['error']}")
        return payload


def message_text(payload: Dict[str, Any]) -> str:
    """Extract assistant text while tolerating string or content-part responses."""
    try:
        content = payload["choices"][0]["message"]["content"]
    except (KeyError, IndexError, TypeError) as error:
        raise GrokApiError("xAI API response did not contain assistant content") from error

    if isinstance(content, str):
        return content
    if isinstance(content, list):
        parts = []
        for item in content:
            if isinstance(item, str):
                parts.append(item)
            elif isinstance(item, dict) and isinstance(item.get("text"), str):
                parts.append(item["text"])
        return "".join(parts)
    raise GrokApiError("xAI API returned unsupported assistant content")


def _positive_int(value: Optional[str], fallback: int) -> int:
    if not value:
        return fallback
    try:
        parsed = int(value)
    except ValueError as error:
        raise GrokApiError(f"Expected an integer, got: {value}") from error
    if parsed <= 0:
        raise GrokApiError("Numeric configuration values must be positive")
    return parsed


def _optional_positive_int(value: Optional[str]) -> Optional[int]:
    if not value:
        return None
    return _positive_int(value, 1)
