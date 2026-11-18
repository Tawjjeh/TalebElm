# TalebElm

[![.NET CI](https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml/badge.svg)](https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml)

A software engineering learning platform where the server, not a checkbox, decides when you unlock the next module.

## What it is

TalebElm is an open-source backend for learning software engineering in a fixed order. A learner
enrolls in a Track: a sequence of Modules, each with ordered Lessons, an optional Exam, and optional
practice problems. Progress is earned, not declared: exams are graded on the server, and practice
solves are checked against the learner's linked Codeforces or GitHub account. A finished Module
therefore means the same thing for every learner.

## How it works

1. Enroll in a Track and start at its first Module.
2. Study the Module's Lessons in order.
3. Submit the Module Exam; the server grades every answer and stores the score and pass flag.
4. Solve the Module's required practice problems on the linked external account.
5. When both gates hold, the next Module unlocks. Unlocks are forward-only and sticky.

The unlock rule, in one sentence: a Module unlocks its successor only when there is a next Module,
the exam is passed or the Module has none, and every required problem is verified or the Module has
none ([Business Rule P-2](docs/business/BUSINESS_RULES.md), section 6). The
exam side is implemented and covered by `ExamProgressionTests`; the problem-side trigger is still
open in [#444](https://github.com/Tawjjeh/TalebElm/issues/444).

## The idea

| Part | One line |
|---|---|
| **Tracks** | Ordered Modules and Lessons, with at most one Exam per Module. |
| **Verified problems** | Codeforces solves and GitHub tasks, checked against a linked account instead of self-reported. |
| **Resource Hub** | A public, browsable Technology tree of curated resources. Planned. |
| **Capstone** | One project per Track, delivered in milestones that CI verifies. Planned. |

## Status

| Area | Works today | Next |
|---|---|---|
| Identity | Password hashing (PBKDF2), unique email, repositories | Register, login, JWT, role policies |
| Tracks and lessons | Ordered read services, `GET /api/tracks` | Module, lesson, and authoring endpoints |
| Exams | Server-side grading, score and pass persisted, the exam-side unlock | Exam endpoints, admin authoring, the problem-side trigger ([#444](https://github.com/Tawjjeh/TalebElm/issues/444)) |
| Enrollment and progress | `TrackEnrollment` and `UserProgress` persistence, the enroll service | Enroll and progress endpoints, the shared gate |
| Verification | Entities, configurations, repositories, Codeforces and GitHub contracts | Clients, verifiers, endpoints |
| Resource Hub | Entities, configurations, repositories | Read and admin endpoints, link health |
| Capstone | Entities, configurations | Milestone verification, track completion, endpoints |
| Platform | Health check, exception mapping, field-keyed validation errors, Swagger, CI | Replacing the `501` controllers as features land |

Every feature, with its exact state, is in [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md). The
work is grouped into six milestones: M0 Foundations, M1 Identity, M2 Learning flow, M3 Automated
Verification, M4 Public Resource Hub, M5 Capstone Projects. See the
[milestones page](https://github.com/Tawjjeh/TalebElm/milestones).

## Quick start

You need Git and the [.NET 10 SDK](https://dotnet.microsoft.com/download). There is no
`global.json`; every project targets `net10.0`. Python 3 is only needed for the docs check.

```bash
git clone https://github.com/Tawjjeh/TalebElm.git
cd TalebElm
dotnet restore TalebElm.slnx
dotnet build TalebElm.slnx
dotnet test TalebElm.slnx
```

Run the API in one terminal:

```bash
dotnet run --project src/TalebElm.Api
```

Then call it from another:

```bash
curl http://localhost:5236/api/health
# healthy
```

In Development (the default) the app reads `appsettings.Development.json` and writes
`talebelm.db` under `src/TalebElm.Api/`. Set `ConnectionStrings__TalebElm` to use another database
file; see [`docs/LOCAL_SETUP.md`](docs/LOCAL_SETUP.md). Swagger UI is at
<http://localhost:5236/swagger>.

Before pushing Markdown changes, run the documentation check:

```bash
python3 scripts/check-docs.py
```

## Architecture

Dependencies point inward; an outer layer may use an inner layer, never the other way round.

| Layer | Project | May depend on |
|---|---|---|
| Api | `src/TalebElm.Api` | Infrastructure, Application, Domain |
| Infrastructure | `src/TalebElm.Infrastructure` | Application, Domain |
| Application | `src/TalebElm.Application` | Domain |
| Domain | `src/TalebElm.Domain` | nothing |

[`ARCHITECTURE.md`](ARCHITECTURE.md) covers the responsibilities, project references, and the
rules each layer must respect.

## Documentation

| To read about | Document |
|---|---|
| The full docs index | [`docs/README.md`](docs/README.md) |
| Feature status | [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) |
| Layers, patterns, conventions | [`ARCHITECTURE.md`](ARCHITECTURE.md) |
| Product rules and open decisions | [`docs/business/BUSINESS_RULES.md`](docs/business/BUSINESS_RULES.md), [`docs/OPEN_DECISIONS.md`](docs/OPEN_DECISIONS.md) |
| Current and target routes | [`docs/API_ENDPOINTS.md`](docs/API_ENDPOINTS.md) |
| Local database setup | [`docs/LOCAL_SETUP.md`](docs/LOCAL_SETUP.md) |

## Contributing

1. Pick an open issue with `help wanted`, plus `good first issue` if this is your first
   contribution: [beginner issues](https://github.com/Tawjjeh/TalebElm/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22+label%3A%22help+wanted%22+no%3Aassignee).
2. Comment with your plan and wait until the issue is assigned to you. The assignment is the green
   light; do not start before it.
3. Branch from `main` with a `feat/`, `fix/`, `docs/`, `test/`, `refactor/`, `chore/`, or `ci/`
   prefix, and write conventional commits.
4. Open a pull request against `main` with `Closes #N` in the description, tests, and the template
   filled in. CI and one approval are required before merge.

[`CONTRIBUTING.md`](CONTRIBUTING.md) has the commands, the review rules, and what to avoid.
Coding standards are in [`docs/development/coding-standards.md`](docs/development/coding-standards.md);
"done" is defined in [`docs/development/definition-of-done.md`](docs/development/definition-of-done.md).

## License and community

TalebElm means "seeker of knowledge". The code is licensed under the
[GNU General Public License v3.0](LICENSE). Report vulnerabilities privately as described in
[`SECURITY.md`](SECURITY.md), and follow the [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md).
