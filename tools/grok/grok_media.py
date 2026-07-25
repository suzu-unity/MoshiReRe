"""Small dependency-free clients for xAI Imagine image and video generation."""

from __future__ import annotations

import json
import os
import time
import urllib.error
import urllib.request
from typing import Any, Dict, Optional

from grok_client import GrokApiError, GrokConfig


class XaiMediaClient:
    def __init__(self, config: GrokConfig):
        self.config = config

    def generate_image(self, prompt: str, *, resolution: str = "1k", n: int = 1) -> Dict[str, Any]:
        body = {
            "model": os.environ.get("XAI_IMAGE_MODEL", "grok-imagine-image-quality"),
            "prompt": prompt,
            "resolution": resolution,
            "n": max(1, min(int(n), 4)),
        }
        return self._request("/images/generations", body)

    def generate_video(
        self,
        prompt: str,
        *,
        duration: int = 5,
        aspect_ratio: str = "16:9",
        resolution: str = "720p",
        image_data_url: Optional[str] = None,
    ) -> Dict[str, Any]:
        duration = max(2, min(int(duration), 15))
        body: Dict[str, Any] = {
            "model": os.environ.get("XAI_VIDEO_MODEL", "grok-imagine-video"),
            "prompt": prompt,
            "duration": duration,
            "aspect_ratio": aspect_ratio,
            "resolution": resolution,
        }
        if image_data_url:
            body["image"] = {"url": image_data_url}
        started = self._request("/videos/generations", body)
        request_id = started.get("request_id")
        if not isinstance(request_id, str) or not request_id:
            raise GrokApiError("xAI video API did not return request_id")

        deadline = time.monotonic() + _positive_int(os.environ.get("XAI_VIDEO_TIMEOUT_SECONDS"), 900)
        while time.monotonic() < deadline:
            result = self._request(f"/videos/{request_id}", None, method="GET")
            status = result.get("status")
            if status == "done":
                return result
            if status in {"failed", "expired"}:
                raise GrokApiError(f"xAI video generation {status}: {result}")
            time.sleep(5)
        raise GrokApiError("xAI video generation timed out; the request may still be processing")

    def _request(self, path: str, body: Optional[Dict[str, Any]], *, method: str = "POST") -> Dict[str, Any]:
        headers = {
            "Authorization": f"Bearer {self.config.api_key}",
            "Content-Type": "application/json",
            "Accept": "application/json",
            "User-Agent": "MoshiReRe-GrokTools/1.0",
        }
        data = json.dumps(body, ensure_ascii=False).encode("utf-8") if body is not None else None
        request = urllib.request.Request(
            f"{self.config.base_url}{path}", data=data, headers=headers, method=method
        )
        try:
            with urllib.request.urlopen(request, timeout=self.config.timeout_seconds) as response:
                raw = response.read().decode("utf-8")
        except urllib.error.HTTPError as error:
            detail = error.read().decode("utf-8", errors="replace")
            raise GrokApiError(f"xAI media API returned HTTP {error.code}: {detail[:1000]}") from error
        except urllib.error.URLError as error:
            raise GrokApiError(f"Could not reach xAI media API: {error.reason}") from error
        try:
            payload = json.loads(raw)
        except json.JSONDecodeError as error:
            raise GrokApiError("xAI media API returned invalid JSON") from error
        if not isinstance(payload, dict):
            raise GrokApiError("xAI media API returned an unexpected response")
        if payload.get("error"):
            raise GrokApiError(f"xAI media API error: {payload['error']}")
        return payload


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
