# AGENTS.md — TalebElm

TalebElm is a .NET 10 Clean Architecture learning-platform backend: Track, Module,
Lesson, Exam, Progress, Enrollment, practice problems, the public Resource Hub, and
Capstone projects in Domain → Application → Infrastructure → Api, with tests in
`TalebElm.Tests`. Read the docs, not memory: `docs/README.md` maps them.

## Rules

- Read `docs/FEATURE_INDEX.md`, `docs/OPEN_DECISIONS.md`, and
  `docs/business/BUSINESS_RULES.md` before changing behavior.
- Smallest reversible change. Topic branch; never commit to `main`.
- Do not silently change an approved rule; record it in the open-decisions table.
- No secrets, tokens, connection strings, hostnames, or customer data anywhere.
- Never trust a client score, return `IsCorrect`, use `DbContext` in Application,
  call real external APIs in tests, commit `*.db` files or secrets, or push to main.
- Feature docs are specs, not proof; check the code and the tests.

## Layers

- Domain: entities, enums, exceptions, repositories, `IUnitOfWork`; no dependencies.
- Application: DTOs, services, validators, client interfaces; uses `IUnitOfWork`.
- Infrastructure: EF Core, configurations, migrations, repositories.
- Api: controllers, middleware, `Program.cs`.
- Tests: unit and integration tests on SQLite.

## Checks

`dotnet build TalebElm.slnx`, `dotnet test TalebElm.slnx`, and
`python3 scripts/check-docs.py`. Standards live in `docs/development/`.

## Conventions

- Conventional commits: `feat`, `fix`, `chore`, `docs`, `test`, `refactor`, `ci`.
- One vertical slice per PR; about 200 lines including tests.
- Server verifies progression; keep `Implemented` / `Target` / `Proposed` accurate.
- One type, one layer, and the owning milestone per item (`docs/GITHUB_LABELS.md`).
- Unlock follows Decision 32: `examGate`, `problemGate`, and a next Module. External
  clients are Application interfaces with fakes; async methods take `CancellationToken`.
- Report as `Changed:` / `Verified:` / `Risks:` / `Next:`.
