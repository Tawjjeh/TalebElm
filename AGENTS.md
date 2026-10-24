# AGENTS.md — TalebElm

TalebElm is a .NET 10 Clean Architecture learning-platform backend.
Track, Module, Lesson, Exam, Progress, Enrollment, external practice problems,
the public Resource Hub, and Capstone projects are built in layers: Domain →
Application → Infrastructure → Api, with tests in `TalebElm.Tests`.

This repository is documentation-heavy and issue-driven. Read the docs (not
memory) before editing.

## Operating rules

- Read `docs/README.md`, `docs/FEATURE_INDEX.md`, `docs/OPEN_DECISIONS.md`, and
  `docs/business/BUSINESS_RULES.md` before changing behavior.
- Prefer the smallest, reversible change. Open a topic branch; do not commit to
  `main` directly.
- No secrets, tokens, real connection strings, hostnames, or customer data in
  code, tests, logs, docs, or issues.
- Product behavior is governed by `docs/business/BUSINESS_RULES.md` and
  `docs/OPEN_DECISIONS.md`. Do not silently alter an approved rule; record a
  change in the open-decisions table.
- Auto-generated knowledge is never assumed to be live: feature docs are specs,
  not proof of implementation. Check the code and tests.
- Never trust a client-supplied score, return `IsCorrect`, use a `DbContext` in Application,
  hit real external APIs in tests, commit secrets or `*.db` files, or push to main.

## Code boundaries

- `TalebElm.Domain`: entities, enums, exceptions, repository interfaces and
  `IUnitOfWork`. Depends on nothing.
- `TalebElm.Application`: DTOs, services, validators, external client interfaces.
- `TalebElm.Infrastructure`: EF Core, `AppDbContext`, configurations, migrations,
  repositories.
- `TalebElm.Api`: controllers, middleware, `Program.cs`.
- `TalebElm.Tests`: unit and integration tests.
- `Application` never takes a direct `DbContext`; it uses `IUnitOfWork`.

## Build, test, and docs checks

- Build: `dotnet build TalebElm.slnx`
- Test: `dotnet test TalebElm.slnx`
- Docs: `python3 scripts/check-docs.py`
- Coding standards: `docs/development/coding-standards.md`
- Definition of done: `docs/development/definition-of-done.md`
- EF Core uses SQLite for development and integration tests.

## Conventions

- Conventional commits with a type (`feat`, `fix`, `chore`, `docs`, `test`,
  `refactor`, `ci`).
- One vertical slice per PR where possible; about 200 lines changed including
  tests is a good upper bound.
- Zero manual gating: learner progression is verified by the server, not by
  humans.
- Existing progress markers (`Implemented` / `Target` / `Proposed`) stay accurate.
- The private owner workspace is never committed, pushed, exposed, or linked in public docs.

## AI assistant session protocol

- Start: read the private workspace state file (if present), then this file.
- End: write or update the Arabic explanation in `[private-workspace]/learn/NNN-<topic>.ar.md`
  for code changes; update `[private-workspace]/progress.md` and `[private-workspace]/handoff/claude-handoff.md`
  for a phase close; keep them local.
## Unlock and external services

Unlock follows Decision 32 (`docs/business/BUSINESS_RULES.md` P-2): a Module unlocks its successor only when `examGate` and `problemGate` hold and the next Module exists. External clients are Application interfaces with fakes in tests; new async methods accept `CancellationToken`.

- Report in the form:
  `Changed: ...`
  `Verified: ...`
  `Risks: ...`
  `Next: ...`
