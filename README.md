# TalebElm

[![.NET CI](https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml/badge.svg)](https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download)

TalebElm is an open-source backend for a programming learning platform. A learner picks a
Track, studies its Modules in order, and moves to the next Module only when the server confirms
two things: the Module exam was passed and the required practice problems were solved.

Nothing is marked "done" by hand. The server decides, so progress means the same thing for
everyone.

The code is .NET 10 with ASP.NET Core, Entity Framework Core, and SQLite, split into four layers:
Domain, Application, Infrastructure, and Api.

## What works today

| Working | Still to build |
|---|---|
| Solution builds with 0 warnings; 305 tests pass | Registration, login, JWT issuance, and role policies |
| Domain entities, EF Core configurations, and 16 SQLite migrations | Controllers for users, modules, lessons, exams, and progress |
| Password hashing (PBKDF2) | Next-Module unlock after a passed exam |
| Track listing and creation (`GET /api/tracks`, `TrackService.CreateAsync`) | Codeforces and GitHub automated problem verification |
| Module and lesson reads with ordered content | Public Resource Hub endpoints |
| Exam read and server-side grading (`ExamService.SubmitAsync`) | Capstone milestone verification and derived Track completion |
| Track enrollment and progress services | |
| Field-keyed validation errors and exception middleware | |
| `GET /api/health`, Swagger in Development | |

[`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) tracks every feature one by one.

## MVP milestones

Work is grouped into six milestones. Each one is finished when its tests pass and its docs are
updated.

1. **M0 Foundations** — dependency injection, validation errors, `CancellationToken` pipeline,
   logging, test and migration baseline.
2. **M1 Identity** — registration, login, JWT, password policy, throttling, `StudentOnly` and
   `AdminOnly` policies.
3. **M2 Learning flow** — Tracks, Modules, Lessons, Exams, Progress, enrollment, sticky
   unlocking, admin authoring.
4. **M3 Automated verification** — external accounts, Codeforces solve checks, GitHub OAuth,
   task version integrity, dual-gated unlock.
5. **M4 Resource Hub** — technology tree, anonymous browse and filters, admin curation, link
   health checks.
6. **M5 Capstone projects** — one project per Track, milestones verified on GitHub, Track
   completion derived from both gates.

## Quick start

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download), Git, and Python 3 for the
documentation check.

```bash
git clone https://github.com/Tawjjeh/TalebElm.git
cd TalebElm
dotnet restore TalebElm.slnx
dotnet build TalebElm.slnx
dotnet test TalebElm.slnx
dotnet run --project src/TalebElm.Api
```

Check that the API is up:

```bash
curl http://localhost:5236/api/health
```

It answers with the plain text `healthy`. Development uses SQLite in a file named `talebelm.db`
next to the app; set `ConnectionStrings__TalebElm` to use another file. Swagger UI is at
`/swagger` while the app runs in Development.

Before opening a pull request, run the documentation link check:

```bash
python3 scripts/check-docs.py
```

Full setup notes, migration commands, and troubleshooting are in
[`docs/development/getting-started.md`](docs/development/getting-started.md).

## Documentation

| Topic | Document |
|---|---|
| Feature status (implemented vs. planned) | [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) |
| Layers, patterns, and conventions | [`ARCHITECTURE.md`](ARCHITECTURE.md) |
| Roadmap and milestones | [`docs/business/ROADMAP.md`](docs/business/ROADMAP.md) |
| Product rules and open decisions | [`docs/business/BUSINESS_RULES.md`](docs/business/BUSINESS_RULES.md), [`docs/OPEN_DECISIONS.md`](docs/OPEN_DECISIONS.md) |
| Endpoints and database schema | [`docs/API_ENDPOINTS.md`](docs/API_ENDPOINTS.md), [`docs/DATABASE_ERD.md`](docs/DATABASE_ERD.md) |
| How to contribute | [`CONTRIBUTING.md`](CONTRIBUTING.md) |
| Everything else | [`docs/README.md`](docs/README.md) |

## Contributing

Contributions are welcome. Read [`CONTRIBUTING.md`](CONTRIBUTING.md), pick an issue, say how you
plan to solve it, and open a pull request against `main` with a test for the new behavior.

## License

GNU General Public License v3.0. See [LICENSE](LICENSE).
