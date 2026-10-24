#!/usr/bin/env python3
"""Validate public Markdown documentation for TalebElm."""

from pathlib import Path
import re
import sys

ROOT = Path(__file__).resolve().parents[1]
README = ROOT / "docs" / "README.md"
MD_LINK = re.compile(r"(?<!\!)\[[^\]]*\]\(([^)]+)\)")

errors = []

PRIVATE_WORKSPACE = ".local"  # public docs must never mention this folder


def gh_slug(text: str) -> str:
    # GitHub heading slug rules (lowercase, punctuation dropped, spaces -> -)
    s = text.strip().lower()
    s = re.sub(r"[^\w\s-]", "", s)
    s = re.sub(r"\s+", "-", s)
    return s


def strip_frontiers(line: str) -> bool:
    return line.startswith("```") or line.startswith("~~~")


def validate_tables(lines, path):
    lines = lines
    i = 0
    while i < len(lines):
        line = lines[i]
        if "|" in line and i + 1 < len(lines) and re.match(r"^\s*\|?[\s:-]+\|", lines[i + 1]):
            masked_header = re.sub(r"`[^`]*`", lambda mm: "x"*len(mm.group(0)), line)
            header = [c.strip() for c in masked_header.strip().strip("|").split("|")]
            ncols = len(header)
            # validate separator
            masked_sep = re.sub(r"`[^`]*`", lambda mm: "x"*len(mm.group(0)), lines[i+1])
            sep = [c.strip() for c in masked_sep.strip().strip("|").split("|")]
            if len(sep) != ncols:
                errors.append(f"{path}:{i+2}: table separator column count differs from header")
            j = i + 2
            while j < len(lines) and "|" in lines[j] and lines[j].strip():
                masked_row = re.sub(r"`[^`]*`", lambda mm: "x"*len(mm.group(0)), lines[j])
                row = [c.strip() for c in masked_row.strip().strip("|").split("|")]
                if len(row) != ncols:
                    errors.append(f"{path}:{j+1}: table row column count differs (expected {ncols}, got {len(row)})")
                j += 1
            i = j
        else:
            i += 1


def headings_slugs(lines):
    seen = {}
    slugs = set()
    for line in lines:
        m = re.match(r"^(#{1,6})\s+(.*)$", line.strip())
        if not m:
            continue
        base = gh_slug(m.group(2))
        if base not in seen:
            seen[base] = 0
        else:
            seen[base] += 1
        slug = base if seen[base] == 0 else f"{base}-{seen[base]}"
        slugs.add(slug)
    return slugs


# Optional private forbidden list (never printed)
extra_terms = []
private_list = ROOT / PRIVATE_WORKSPACE / "forbidden-terms.txt"
if private_list.exists():
    for raw in private_list.read_text(encoding="utf-8").splitlines():
        term = raw.strip()
        if term and not term.startswith("#"):
            extra_terms.append(term)


public_files = []
for path in ROOT.rglob("*.md"):
    if f"/{PRIVATE_WORKSPACE}/" in str(path).replace("\\", "/"):
        continue
    if "/node_modules/" in str(path).replace("\\", "/"):
        continue
    if "/.git/" in str(path).replace("\\", "/"):
        continue
    public_files.append(path)

for path in public_files:
    raw = path.read_text(encoding="utf-8")
    lines = raw.splitlines()

    # fenced code block balance
    fence_state = None
    code_ranges = []  # (start, end) exclusive lines
    i = 0
    in_code = False
    code_start = None
    while i < len(lines):
        if lines[i].strip().startswith("```") or lines[i].strip().startswith("~~~"):
            if not in_code:
                in_code = True
                code_start = i
            else:
                code_ranges.append((code_start, i + 1))
                in_code = False
            i += 1
            continue
        i += 1
    if in_code:
        errors.append(f"{path}:{code_start + 1}: unbalanced fenced code block")

    in_code_set = set()
    for s, e in code_ranges:
        for line_no in range(s, e):
            in_code_set.add(line_no)

    slugs = headings_slugs(lines)
    validate_tables(lines, path)

    for idx, line in enumerate(lines, start=1):
        if idx - 1 in in_code_set:
            continue
        if PRIVATE_WORKSPACE in line:
            errors.append(f"{path}:{idx}: forbidden private workspace reference")
        for term in extra_terms:
            if term in line:
                errors.append(f"{path}:{idx}: forbidden private term referenced")
        if "$\\" in line:
            errors.append(f"{path}:{idx}: LaTeX token detected")
        for m in MD_LINK.finditer(line):
            target = m.group(1).strip()
            # handle angle-bracketed links
            if target.startswith("<") and target.endswith(">"):
                target = target[1:-1].strip()
            # strip title
            title_match = re.match(r'^(.*?)(\s+"[^"]*"\s*|\s+\'[^\']*\'\s*)?$', target)
            if title_match:
                target = title_match.group(1).strip()
            if not target or target.startswith("#") or target.startswith(("http://", "https://", "mailto:", "urn:")):
                continue
            path_part, _, anchor = target.partition("#")
            if not path_part:
                if anchor and anchor not in slugs:
                    errors.append(f"{path}:{idx}: missing anchor #{anchor}")
                continue
            resolved = (path.parent / path_part).resolve()
            try:
                resolved.relative_to(ROOT)
            except ValueError:
                errors.append(f"{path}:{idx}: link leaves repository: {target}")
                continue
            if not resolved.exists():
                errors.append(f"{path}:{idx}: broken link -> {target}")
                continue
            if anchor:
                if resolved.suffix.lower() != ".md":
                    continue
                target_lines = resolved.read_text(encoding="utf-8").splitlines()
                target_slugs = headings_slugs(target_lines)
                if anchor.lstrip("#") not in target_slugs:
                    errors.append(f"{path}:{idx}: missing anchor #{anchor.lstrip('#')} in {path_part}")

# Required entry points
for expected in ("README.md", "docs/README.md", "AGENTS.md", "ARCHITECTURE.md"):
    if not (ROOT / expected).exists():
        errors.append(f"Missing required documentation file: {expected}")

# Every docs markdown must be linked from docs/README.md
if README.exists():
    readme_text = README.read_text(encoding="utf-8")
    linked = set()
    for line in readme_text.splitlines():
        for m in MD_LINK.finditer(line):
            linked.add(m.group(1).strip().lstrip("<").rstrip(">").split("#", 1)[0])
    for md in ROOT.joinpath("docs").rglob("*.md"):
        rel = md.relative_to(ROOT).as_posix()
        if rel == "docs/README.md":
            continue
        expected = md.relative_to(ROOT.joinpath("docs")).as_posix()
        # accept a few equivalent forms
        candidates = {md.relative_to(ROOT).as_posix(), expected, "docs/" + expected}
        if not candidates.intersection(linked):
            errors.append(f"docs/README.md does not link: {rel}")

if errors:
    print("\n".join(errors))
    raise SystemExit(1)

print(f"Checked {len(public_files)} public Markdown files; all relative links and anchors resolve.")
