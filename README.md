<div align="center">
  <picture>
    <source media="(prefers-color-scheme: dark)" srcset=".github/assets/banner-dark.svg">
    <img alt="طالب علم, TalebElm" src=".github/assets/banner-light.svg" width="600">
  </picture>

  <p>
    <a href="https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml"><img src="https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml/badge.svg" alt=".NET CI"></a>
    <a href="LICENSE"><img src="https://img.shields.io/badge/License-GPL--3.0-blue.svg" alt="License: GPL-3.0"></a>
    <a href="https://github.com/Tawjjeh/TalebElm/discussions"><img src="https://img.shields.io/badge/Discussions-Join-0969da" alt="Discussions"></a>
  </p>

  [How it works](#how-it-works) · [What's inside](#whats-inside) · [Status](#status) · [Quick start](#quick-start) · [Architecture](#architecture) · [Documentation](#documentation) · [Contributing](#contributing)
</div>

---

**Learn software engineering. Earn every step.**

TalebElm is an open-source learning path for software engineers with ordered tracks, real exams, and practice that gets checked before the next step opens.

## How it works

- Enroll in an ordered Track and study its Lessons.
- At the end of each Module, take a server-graded conceptual Exam.
- Solve assigned practice problems verified automatically via the Codeforces API and GitHub Actions.
- When both the exam and problem gates pass, the next Module unlocks forward-only.
- Track completion requires passing every Module and completing all Capstone milestones.

```mermaid
flowchart LR
    A["Enroll in Track"] --> B["Study Lessons"]
    B --> C["Submit Exam"]
    B --> D["Solve Problems"]
    C --> E{"Dual Gate"}
    D --> E
    E -->|"Both pass"| F["Unlock Next Module"]
    E -->|"Incomplete"| B
```

## Progression unlock rule

A module unlocks its successor if and only if both its exam gate and its problem gate hold, evaluated inside an atomic transaction as specified in the [business rules](docs/business/BUSINESS_RULES.md#6-progress-tracking-unlock) (rule P-2) and [architectural decisions](docs/OPEN_DECISIONS.md):

```text
examGate    = !hasExam || PassedExam
problemGate = !hasRequiredProblems || CompletedRequiredProblems
unlockNext  = (hasExam || hasRequiredProblems) && examGate && problemGate && nextModuleExists
```

Progress is forward-only and sticky: passing an exam or completing required problems cannot be undone by a later attempt.

## What's inside

- **Guided tracks:** Clear sequences with zero guesswork. Learners follow ordered Tracks, Modules, and Lessons with structured explanations and curated references.
- **Server-graded exams:** Instant conceptual feedback without leaked answers. Assessments evaluate understanding against hidden answer keys, preserving passes permanently.
- **Verified practice:** Authentic engineering competence. Algorithmic solutions check against Codeforces, and hands-on repository tasks verify against GitHub Actions runs and commit SHAs.
- **Open resource hub:** Frictionless technical references. A searchable catalog organized by language, technology, and topic that visitors can browse anonymously.

## Status

All current work is tracked under six MVP milestones (see [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) for detailed implementation status):

| Milestone | Scope | In MVP | Status |
|---|---|---|---|
| [M0 Foundations](https://github.com/Tawjjeh/TalebElm/milestone/1) | DI baseline, error mapping, cancellation tokens, CI health | Yes | Open |
| [M1 Identity](https://github.com/Tawjjeh/TalebElm/milestone/2) | Registration, login, password hashing, signed JWTs, authorization policies | Yes | Open |
| [M2 Learning flow](https://github.com/Tawjjeh/TalebElm/milestone/3) | Tracks, Modules, Lessons, Exams, Progress, server-side grading, sticky unlocking | Yes | Open |
| [M3 Automated Verification](https://github.com/Tawjjeh/TalebElm/milestone/4) | External accounts, Codeforces solve checks, GitHub OAuth, task SHA integrity, dual-gate unlock | Yes | Open |
| [M4 Public Resource Hub](https://github.com/Tawjjeh/TalebElm/milestone/5) | Technology tree, Resource persistence, anonymous search and filters, admin curation, link checker | Yes | Open |
| [M5 Capstone Projects](https://github.com/Tawjjeh/TalebElm/milestone/6) | Capstone persistence, ordered milestone verification via GitHub tasks, derived Track completion | Yes | Open |

## Quick start

Prerequisites: [.NET 10 SDK](https://dotnet.microsoft.com/download), Git, Python 3 (for documentation audits).

```bash
git clone https://github.com/Tawjjeh/TalebElm.git
cd TalebElm
dotnet restore TalebElm.slnx
dotnet build TalebElm.slnx
dotnet test TalebElm.slnx
dotnet run --project src/TalebElm.Api
```

Verify health in another terminal: `curl http://localhost:5236/api/health` (`healthy`). Swagger UI is at <http://localhost:5236/swagger>. Run doc audits with `python3 scripts/check-docs.py`.

## Architecture

Dependencies point strictly inward. Outer layers depend on inner layers; inner layers have zero knowledge of outer layers.

| Layer | Project | Responsibilities | May depend on |
|---|---|---|---|
| **Api** | `src/TalebElm.Api` | HTTP controllers, middleware, exception handling, Swagger, DI host | Infrastructure, Application, Domain |
| **Infrastructure** | `src/TalebElm.Infrastructure` | EF Core `AppDbContext`, migrations, configurations, repositories, HTTP clients | Application, Domain |
| **Application** | `src/TalebElm.Application` | DTOs, service implementations, FluentValidation validators, client interfaces | Domain |
| **Domain** | `src/TalebElm.Domain` | Entities, enums, domain exceptions, repository contracts, `IUnitOfWork` | *None (zero dependencies)* |

## Documentation

| Guide | Content |
|---|---|
| [`docs/README.md`](docs/README.md) | Central documentation directory and reading order |
| [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) | Current implementation state versus MVP target per feature |
| [`docs/business/ROADMAP.md`](docs/business/ROADMAP.md) | Definitive MVP boundaries, milestone exit criteria, and post-MVP directions |
| [`docs/business/BUSINESS_RULES.md`](docs/business/BUSINESS_RULES.md) | Binding product rules with implementation status |
| [`docs/OPEN_DECISIONS.md`](docs/OPEN_DECISIONS.md) | Architectural and product decisions (Decisions 1 through 40) |
| [`docs/LOCAL_SETUP.md`](docs/LOCAL_SETUP.md) | Local database setup, connection strings, and EF Core migrations |

## Roadmap

The MVP scope is defined in [`docs/business/ROADMAP.md`](docs/business/ROADMAP.md) with explicit exit criteria. Community input on post-MVP tracks, platforms, and tool additions is welcome in [Discussions](https://github.com/Tawjjeh/TalebElm/discussions).

## Contributing

1. **Pick an issue:** Find an open issue with `help wanted`, or [`good first issue`](https://github.com/Tawjjeh/TalebElm/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22+label%3A%22help+wanted%22+no%3Aassignee) if you are new.
2. **Wait for the green light:** Comment on the issue to claim it. Work begins only after an owner assigns the issue to you (the green light is the `help wanted` label plus an assignment).
3. **Make a focused change:** Keep the change under about 200 lines with automated tests proving the behavior.
4. **Open a pull request:** Reference the issue with `Closes #N` and verify build, tests, and doc checks pass.

Read [`CONTRIBUTING.md`](CONTRIBUTING.md) for full guidelines.

## License and security

TalebElm is licensed under [GPL-3.0](LICENSE). Brand assets in `.github/assets/` are governed by their respective licenses (see [`.github/assets/README.md`](.github/assets/README.md)). Report security vulnerabilities privately following [`SECURITY.md`](SECURITY.md). Community interactions follow our [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md).
