# TalebElm

[![.NET CI](https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml/badge.svg)](https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml)
[![Release build](https://github.com/Tawjjeh/TalebElm/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/Tawjjeh/TalebElm/actions/workflows/dotnet-ci.yml)
[![Security scan](https://github.com/Tawjjeh/TalebElm/actions/workflows/security-scan.yml/badge.svg)](https://github.com/Tawjjeh/TalebElm/actions/workflows/security-scan.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download)

TalebElm is a learning platform for people teaching themselves software
engineering. You work through ordered tracks, and the next module opens only
when the server can confirm two things: you passed the module exam and you
solved the practice problems. There is no "mark as done" button, and nobody
reviews your progress by hand.

The name is Arabic for *seeker of knowledge* (طالب علم).

Built with .NET 10, ASP.NET Core, Entity Framework Core, and a strict Clean
Architecture split between Domain, Application, Infrastructure, and Api.

## What works today

| Working now | Specified, in progress (most endpoints still return `501`) |
|---|---|
| Build and test suite, green in CI on `main` | Registration, login, JWT, and authorization policies |
| Domain model, EF Core configurations, migrations, repositories | Exam questions, server-side grading, and results |
| Track list/create, plus ordered module and lesson reads | Enrollment and the dual-gated module unlock |
| `GET /api/health` and domain exception → HTTP status mapping | Codeforces and GitHub task verification |
| PBKDF2 password hashing and a unique email index | Public Resource Hub and capstone projects |

The status above comes from [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md).
That file is the source of truth: if this page and that one ever disagree, the
index is right.

## The three pillars

1. **Guided tracks** — ordered tracks, modules, and lessons, with an exam and a
   set of practice problems closing every module.
2. **Verified problems** — every unlock is checked by the server against
   Codeforces or GitHub, never by a click.
3. **Resource Hub** — a public, anonymous index where any topic is at most
   three clicks away.

The reasoning behind each one is in
[`docs/business/PRODUCT_VISION.md`](docs/business/PRODUCT_VISION.md).

## Quick start

You need the [.NET 10 SDK](https://dotnet.microsoft.com/download) and git.

```bash
git clone https://github.com/Tawjjeh/TalebElm.git
cd TalebElm
dotnet restore TalebElm.slnx
dotnet build TalebElm.slnx
dotnet run --project src/TalebElm.Api
```

In a second terminal, check that it is alive:

```bash
curl http://localhost:5236/api/health
```

You should get `healthy`.

Local development uses SQLite.
`src/TalebElm.Api/appsettings.Development.json` already points at
`Data Source=talebelm.db`, so there is nothing to configure to get started. To
use a different file, set the `ConnectionStrings__TalebElm` environment
variable; [`docs/LOCAL_SETUP.md`](docs/LOCAL_SETUP.md) has the details and the
fix for the "connection string does not exist" error.

Run the tests and the documentation check before you open a pull request:

```bash
dotnet test TalebElm.slnx
python3 scripts/check-docs.py
```

## Documentation map

| If you want to… | Read |
|---|---|
| See what is implemented and what is not | [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) |
| Understand the layers and conventions | [`ARCHITECTURE.md`](ARCHITECTURE.md), [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) |
| Follow the plan | [`docs/business/ROADMAP.md`](docs/business/ROADMAP.md) |
| Read the product rules | [`docs/business/BUSINESS_RULES.md`](docs/business/BUSINESS_RULES.md), [`docs/OPEN_DECISIONS.md`](docs/OPEN_DECISIONS.md) |
| Walk the learner journey | [`docs/MVP_LEARNING_JOURNEY.md`](docs/MVP_LEARNING_JOURNEY.md) |
| Check the routes | [`docs/API_ENDPOINTS.md`](docs/API_ENDPOINTS.md) |
| See the data model | [`docs/DATABASE_ERD.md`](docs/DATABASE_ERD.md) |
| Find any other document | [`docs/README.md`](docs/README.md) |

## Contributing

Everyone is welcome, including people opening their first pull request. The
short version:

1. **Pick an issue.** Start with
   [issues labelled `good first issue`](https://github.com/Tawjjeh/TalebElm/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22),
   or anything tagged `help wanted`.
2. **Ask for it and wait for a go-ahead.** Comment on the issue with what you
   plan to change. Work is approved when the issue has `help wanted`, a
   milestone, and a short confirmation from us; then you get assigned. Please
   do not start before that, so two people do not build the same thing.
3. **Make the change.** Fork, branch from `main`, keep it to one small slice,
   and run the build, the tests, and the docs check locally.
4. **Open the pull request.** Use a conventional title such as
   `fix(auth): reject duplicate email`, fill in the template, and write
   `Closes #123` so the issue closes on merge.

[`CONTRIBUTING.md`](CONTRIBUTING.md) has the full workflow, the quality gates,
and what to do when you get stuck. Questions are fine in
[Discussions](https://github.com/Tawjjeh/TalebElm/discussions).

## License

TalebElm is distributed under the GNU General Public License v3.0. See
[LICENSE](LICENSE) for the full terms.
