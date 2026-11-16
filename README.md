# TalebElm

[![.NET CI](https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml/badge.svg)](https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download)

TalebElm is an open-source backend for a software engineering learning platform that enforces
real progress. A learner studies ordered Modules inside a Track and cannot move forward until the
server verifies two things independently: the Module exam was passed and every required practice
problem was solved on an external platform (Codeforces or GitHub).

No self-reporting. No "mark as done" button. The same rule applies to every learner, enforced by
the same server code, so a completed Module always means the same thing.

The stack is .NET 10, ASP.NET Core, Entity Framework Core with SQLite, and Clean Architecture
across four layers: Domain, Application, Infrastructure, and Api.

## How verification works

A learner cannot unlock the next Module until both gates clear in the same transaction:

```
examGate    = no exam on this Module  OR  exam score >= PassThreshold
problemGate = no required problems    OR  all required problems solved externally
```

For Codeforces problems, the server calls the Codeforces API and checks for an `OK` verdict on
the learner's linked account. For GitHub tasks, it checks that the linked repository was created
from the official template, that the latest CI run succeeded, and that the workflow file and test
tree SHA match a registered version — so editing the tests to force a pass is detected.

A pass is sticky: retaking the exam and scoring lower does not revert the unlock. An external
service outage returns `503` and leaves the learner's state unchanged.

## What works today

| Working | Still to build |
|---|---|
| Solution builds with 0 warnings; 305 tests pass | Registration, login, JWT issuance, and role policies |
| Domain entities, EF Core configurations, and 16 SQLite migrations | Wired controllers for modules, lessons, exams, progress, and users |
| Password hashing (PBKDF2) | Next-Module unlock after a passed exam |
| Track listing and creation (`GET /api/tracks`, `TrackService.CreateAsync`) | Codeforces and GitHub automated problem verification |
| Module and lesson reads with ordered content | Public Resource Hub endpoints |
| Exam read and server-side grading (`ExamService.SubmitAsync`) | Capstone milestone verification and derived Track completion |
| Track enrollment and progress services | |
| Field-keyed validation errors, exception middleware, and `traceId` in every error response | |
| `GET /api/health`, OpenAPI/Swagger in Development | |

[`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) tracks every feature one by one.

## MVP milestones

Six milestones, each finished when tests pass and docs are updated.

| Milestone | Scope |
|---|---|
| **M0 Foundations** | DI composition, validation errors, `CancellationToken` pipeline, logging, test and migration baseline |
| **M1 Identity** | Registration, login, JWT, password policy, throttling, `StudentOnly` and `AdminOnly` policies |
| **M2 Learning flow** | Tracks, Modules, Lessons, Exams, Progress, enrollment, sticky unlock, admin authoring |
| **M3 Automated verification** | External account linking, Codeforces solve checks, GitHub OAuth, task SHA integrity, dual-gated unlock |
| **M4 Resource Hub** | Technology tree, anonymous browse and filters, admin curation, safe link-health checks |
| **M5 Capstone projects** | One capstone project per Track, milestone verification via GitHub Actions, derived Track completion |

## Quick start

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download), Git, and Python 3.

```bash
git clone https://github.com/Tawjjeh/TalebElm.git
cd TalebElm
dotnet restore TalebElm.slnx
dotnet build TalebElm.slnx
dotnet test TalebElm.slnx
dotnet run --project src/TalebElm.Api
```

```bash
curl http://localhost:5236/api/health
# healthy
```

Swagger UI is at `/swagger` while the app runs in Development. The database file is
`talebelm.db` next to the app binary; set `ConnectionStrings__TalebElm` to change it.

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
| Product rules and decisions | [`docs/business/BUSINESS_RULES.md`](docs/business/BUSINESS_RULES.md), [`docs/OPEN_DECISIONS.md`](docs/OPEN_DECISIONS.md) |
| API endpoints and database schema | [`docs/API_ENDPOINTS.md`](docs/API_ENDPOINTS.md), [`docs/DATABASE_ERD.md`](docs/DATABASE_ERD.md) |
| How to contribute | [`CONTRIBUTING.md`](CONTRIBUTING.md) |
| Full docs index | [`docs/README.md`](docs/README.md) |

## Contributing

Read [`CONTRIBUTING.md`](CONTRIBUTING.md), pick an open issue labeled `help wanted`, describe
your approach in a comment, and open a pull request against `main` with a test for the new
behavior.

## License

GNU General Public License v3.0. See [LICENSE](LICENSE).
