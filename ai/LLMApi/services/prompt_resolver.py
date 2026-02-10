from __future__ import annotations

import logging
from dataclasses import dataclass
from datetime import datetime, timedelta, timezone
from threading import Lock
from typing import Optional

from sqlalchemy.orm import Session

from models.database import Prompt

logger = logging.getLogger(__name__)


@dataclass(frozen=True)
class PromptRef:
    """
    Selector used by upstream (C# / HTTP / queue message).

    Preferred:
    - name + version (exact prompt)

    Supported fallbacks:
    - name only (latest version)
    - category + version (legacy)
    - category only (latest version, legacy)
    """

    name: Optional[str] = None
    category: Optional[str] = None
    version: Optional[int] = None


@dataclass(frozen=True)
class ResolvedPrompt:
    content: str
    resolved_by: str  # e.g. "db:name+version", "db:latest-by-name", "default"
    name: Optional[str] = None
    category: Optional[str] = None
    version: Optional[int] = None


class PromptResolver:
    """Resolve prompt content from the `Prompts` table (simple + predictable)."""

    _LATEST_TTL = timedelta(seconds=60)

    def __init__(self) -> None:
        # Exact prompts are immutable (name+version) -> safe to cache forever in-process
        self._exact_cache: dict[tuple[str, int], ResolvedPrompt] = {}
        # Latest prompts can change -> cache with a short TTL
        self._latest_cache: dict[str, tuple[ResolvedPrompt, datetime]] = {}
        self._lock = Lock()

    def resolve(
        self,
        db: Session,
        prompt_ref: PromptRef,
        *,
        request_id: str,
        default_content: str,
        allow_latest: bool = True,
    ) -> ResolvedPrompt:
        name = (prompt_ref.name or "").strip() or None
        category = (prompt_ref.category or "").strip() or None
        version = prompt_ref.version if isinstance(prompt_ref.version, int) else None

        # 1) Best: exact (name + version)
        if name and version and version >= 1:
            cache_key = (name, version)
            with self._lock:
                cached = self._exact_cache.get(cache_key)
            if cached is not None:
                return cached

            prompt = db.query(Prompt).filter(Prompt.Name == name, Prompt.Version == version).first()
            if prompt and prompt.Content:
                resolved = ResolvedPrompt(
                    content=prompt.Content,
                    resolved_by="db:name+version",
                    name=prompt.Name,
                    category=prompt.Category,
                    version=prompt.Version,
                )
                with self._lock:
                    self._exact_cache[(prompt.Name, prompt.Version)] = resolved
                return resolved

        # 2) Next: latest by name
        if allow_latest and name:
            latest_key = f"name:{name}"
            cached_latest = self._get_latest(latest_key)
            if cached_latest is not None:
                return cached_latest

            prompt = db.query(Prompt).filter(Prompt.Name == name).order_by(Prompt.Version.desc()).first()
            if prompt and prompt.Content:
                resolved = ResolvedPrompt(
                    content=prompt.Content,
                    resolved_by="db:latest-by-name",
                    name=prompt.Name,
                    category=prompt.Category,
                    version=prompt.Version,
                )
                self._set_latest(latest_key, resolved)
                return resolved

        # 3) Legacy: category + version
        if category and version and version >= 1:
            prompt = db.query(Prompt).filter(Prompt.Category == category, Prompt.Version == version).first()
            if prompt and prompt.Content:
                return ResolvedPrompt(
                    content=prompt.Content,
                    resolved_by="db:category+version",
                    name=prompt.Name,
                    category=prompt.Category,
                    version=prompt.Version,
                )

        # 4) Legacy: latest by category
        if allow_latest and category:
            latest_key = f"category:{category}"
            cached_latest = self._get_latest(latest_key)
            if cached_latest is not None:
                return cached_latest

            prompt = db.query(Prompt).filter(Prompt.Category == category).order_by(Prompt.Version.desc()).first()
            if prompt and prompt.Content:
                resolved = ResolvedPrompt(
                    content=prompt.Content,
                    resolved_by="db:latest-by-category",
                    name=prompt.Name,
                    category=prompt.Category,
                    version=prompt.Version,
                )
                self._set_latest(latest_key, resolved)
                return resolved

        logger.warning(f"[{request_id}] Prompt not found; using default. (name={name}, category={category}, version={version})")
        return ResolvedPrompt(content=default_content, resolved_by="default", name=name, category=category, version=version)

    def _get_latest(self, key: str) -> ResolvedPrompt | None:
        now = datetime.now(timezone.utc)
        with self._lock:
            cached = self._latest_cache.get(key)
            if not cached:
                return None
            value, cached_at = cached
            if now - cached_at > self._LATEST_TTL:
                self._latest_cache.pop(key, None)
                return None
            return value

    def _set_latest(self, key: str, value: ResolvedPrompt) -> None:
        now = datetime.now(timezone.utc)
        with self._lock:
            self._latest_cache[key] = (value, now)


# Singleton resolver (safe per-process, reduces DB chatter)
prompt_resolver = PromptResolver()