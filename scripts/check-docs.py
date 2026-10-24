#!/usr/bin/env python3
"""Validate the public documentation links for TalebElm."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
MD_LINK = re.compile(r"(?<!\!)\[[^\]]*\]\(([^)]+)\)")

errors = []

def check_link(source: Path, target_text: str) -> None:
    target = target_text.strip()
    if not target or target.startswith("#") or target.startswith(("http://", "https://", "mailto:", "urn:")):
        return
    path_part, _, anchor = target.partition("#")
    if not path_part:
        return
    resolved = (source.parent / path_part).resolve()
    try:
        resolved.relative_to(ROOT)
    except ValueError:
        errors.append(f"{source}: link leaves repository: {target}")
        return
    if not resolved.exists():
        errors.append(f"{source}: broken link -> {target}")
    # detect private workspace links
    if ".local/" in target.replace("\\", "/"):
        errors.append(f"{source}: public link to private workspace: {target}")

md_files = []
for path in ROOT.rglob("*.md"):
    if "/.local/" in str(path) or "\\node_modules\\" in str(path) or "/.git/" in str(path):
        continue
    md_files.append(path)

for path in md_files:
    for line in path.read_text(encoding="utf-8").splitlines():
        for match in MD_LINK.finditer(line):
            check_link(path, match.group(1))

# required entry points
for expected in ("README.md", "docs/README.md", "AGENTS.md", "ARCHITECTURE.md"):
    if not (ROOT / expected).exists():
        errors.append(f"Missing required documentation file: {expected}")

if errors:
    print("\n".join(errors))
    raise SystemExit(1)

print(f"Checked {len(md_files)} public Markdown files; all relative links resolve.")
