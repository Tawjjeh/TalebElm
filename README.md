# TalebElm

[![.NET CI](https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml/badge.svg)](https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download)

TalebElm is an open-source backend learning platform for software engineering students. Learners progress through ordered tracks and modules, unlocking the next stage only when the server verifies two conditions: they passed the module exam and they solved the required practice problems. There is no manual progress review or "mark as done" button.

Built with .NET 10, ASP.NET Core, Entity Framework Core (SQLite), and Clean Architecture across four layers: Domain, Application, Infrastructure, and Api.

## What works today

| Implemented | MVP in progress |
|---|---|
| Solution build and 274+ unit/integration tests | Auth registration, login, JWT tokens, and role policies |
| Domain entities, EF Core configurations, and SQLite migrations | Module unlock gate and remaining controller endpoints |
| Track paging/listing, ordered module and lesson reads, enrollment service | Codeforces and GitHub automated problem verification |
| Server-side exam grading (`ExamService.SubmitAsync`) | Public Resource Hub browse/admin endpoints and link checker |
| `GET /api/health`, OpenAPI/Swagger in Development, and exception middleware | Capstone milestone verification and derived Track completion |

See [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) for the exact status of every feature.

## MVP scope (M0 to M5)

All active work is organized into six self-contained MVP milestones:

1. **M0 Foundations** — DI composition, validation errors, `CancellationToken` pipeline, request logging, and test/migration baseline.
2. **M1 Identity** — User registration, login, JWT issuance, password policy, throttling, and `StudentOnly` / `AdminOnly` policies.
3. **M2 Learning flow** — Tracks, Modules, Lessons, Exams, Progress, enrollment, sticky unlocking, and admin authoring.
4. **M3 Automated Verification** — External account linking, Codeforces solve verification, GitHub OAuth, `GitHubTask` / `TaskVersion` SHA integrity checks, and dual-gated unlocking.
5. **M4 Public Resource Hub** — Technology tree, anonymous browse/search/filter API, admin curation, safe link-health checking, and shared `Resource` migration.
6. **M5 Capstone Projects** — Cumulative track capstone projects, milestone verification via GitHub tasks, and derived Track completion.

## Quick start

Requires the [.NET 10 SDK](https://dotnet.microsoft.com/download), Git, and Python 3 (for the docs check).

```bash
git clone https://github.com/Tawjjeh/TalebElm.git
cd TalebElm
dotnet restore TalebElm.slnx
dotnet build TalebElm.slnx
dotnet test TalebElm.slnx
dotnet run --project src/TalebElm.Api
```

Verify the API is running:

```bash
curl http://localhost:5236/api/health
```

It returns plain text `healthy`. Local development uses SQLite (`Data Source=talebelm.db` in `src/TalebElm.Api/appsettings.Development.json`). Set `ConnectionStrings__TalebElm` to override it; see [`docs/LOCAL_SETUP.md`](docs/LOCAL_SETUP.md) for details.

Run the documentation link check before opening a pull request:

```bash
python3 scripts/check-docs.py
```

## Documentation

| Topic | Document |
|---|---|
| Feature implementation status | [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) |
| Architecture and layer boundaries | [`ARCHITECTURE.md`](ARCHITECTURE.md), [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) |
| Roadmap and milestones | [`docs/business/ROADMAP.md`](docs/business/ROADMAP.md) |
| Business rules and open decisions | [`docs/business/BUSINESS_RULES.md`](docs/business/BUSINESS_RULES.md), [`docs/OPEN_DECISIONS.md`](docs/OPEN_DECISIONS.md) |
| Endpoints and database schema | [`docs/API_ENDPOINTS.md`](docs/API_ENDPOINTS.md), [`docs/DATABASE_ERD.md`](docs/DATABASE_ERD.md) |
| Contribution and governance guide | [`CONTRIBUTING.md`](CONTRIBUTING.md) |
| Full documentation index | [`docs/README.md`](docs/README.md) |

## Contributing

Contributions are welcome. Pick an issue tagged `good first issue` or `help wanted`, comment with your plan so we can assign it, branch from an up-to-date `main`, keep changes focused on one slice with tests, and open a pull request using conventional commits. Read [`CONTRIBUTING.md`](CONTRIBUTING.md) for branch conventions, automated reviewer assignment, and merge governance.

## License

Licensed under the GNU General Public License v3.0. See [LICENSE](LICENSE).
