# TalebElm

[![.NET CI](https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml/badge.svg)](https://github.com/Tawjjeh/TalebElm/actions/workflows/ci.yml)
[![Release Build](https://github.com/Tawjjeh/TalebElm/actions/workflows/dotnet-ci.yml/badge.svg)](https://github.com/Tawjjeh/TalebElm/actions/workflows/dotnet-ci.yml)
[![Security Scan](https://github.com/Tawjjeh/TalebElm/actions/workflows/security-scan.yml/badge.svg)](https://github.com/Tawjjeh/TalebElm/actions/workflows/security-scan.yml)
[![License: GPL v3](https://img.shields.io/badge/License-GPLv3-blue.svg)](LICENSE)
[![.NET 10](https://img.shields.io/badge/.NET-10.0-512BD4.svg)](https://dotnet.microsoft.com/download)

**TalebElm** is a scalable, enterprise-grade backend platform for structured learning delivery — tracks, modules, lessons, exams, and learner progression — built on **ASP.NET Core** with **Clean Architecture**, **Entity Framework Core**, and automated DevOps quality gates.

The platform exposes a JSON HTTP API backed by a strictly layered domain model, explicit persistence boundaries, and a comprehensive automated test suite. It is designed for extensibility across content domains, field-level data integrity, and safe evolution of business rules independent of infrastructure concerns.

---

## Table of Contents

- [Platform Capabilities](#platform-capabilities)
- [Architecture](#architecture)
- [Technology Stack](#technology-stack)
- [Solution Structure](#solution-structure)
- [API Surface](#api-surface)
- [Persistence](#persistence)
- [Testing Strategy](#testing-strategy)
- [DevOps and Quality Gates](#devops-and-quality-gates)
- [Getting Started](#getting-started)
- [Configuration](#configuration)
- [Database Migrations](#database-migrations)
- [Documentation Index](#documentation-index)
- [Contributing](#contributing)
- [License](#license)

---

## Platform Capabilities

| Capability | Description |
| --- | --- |
| **Content hierarchy** | Ordered `Track → Module → Lesson` content model with sequencing constraints and restricted delete semantics |
| **Assessment engine** | Module-level exam model and read contract; question storage and server-side grading are planned |
| **Learner progression** | Per-learner, per-module progress persistence (`UNIQUE(UserId, ModuleId)`) with documented forward-only unlock rules; the submit/unlock workflow is planned |
| **Identity foundation** | User profiles with `Student`/`Instructor`/`Admin` roles (`Student` default), password hashing, and unique identity indexes |
| **Operational health** | Health-check endpoint and centralized domain-exception → HTTP status mapping |
| **Extensibility** | Persistence contracts for enrollment (`TrackEnrollment`) and lesson resources (`LessonResource`); their workflows and endpoints are planned, as are question banks (`ExamQuestion`/`ExamOption`) — see `docs/FEATURE_INDEX.md` |

> Implementation maturity per capability is tracked explicitly in [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md). A controller or entity class documents a contract; it does not imply the full workflow is wired end to end.

---

## Architecture

TalebElm follows **Clean Architecture** with strict inward-only dependencies:

```text
Api → Infrastructure → Application → Domain
```

- **Domain** (`src/TalebElm.Domain`) — Entities, value constraints, enums, domain exceptions, and repository interfaces. Zero dependencies on EF Core or ASP.NET Core.
- **Application** (`src/TalebElm.Application`) — Use cases: service interfaces (`Services/Abstraction`), service implementations (`Services/Concrete`), DTO contracts (`DTOs/`), and input validators (`Validators/`). Depends only on Domain abstractions, normally via `IUnitOfWork`.
- **Infrastructure** (`src/TalebElm.Infrastructure`) — EF Core persistence: `AppDbContext`, per-entity `IEntityTypeConfiguration<T>` classes, repository implementations, `UnitOfWork`, and DI wiring. Contains no business logic.
- **Api** (`src/TalebElm.Api`) — Thin HTTP layer: controllers, exception-handling middleware, and composition root (`Program.cs`). Delegates to Application services; never accesses repositories directly.
- **Tests** (`tests/TalebElm.Tests`) — Unit, repository (SQLite-backed), and integration coverage that may reference any production layer.

Reference documentation:

- [`ARCHITECTURE.md`](ARCHITECTURE.md) — Layer responsibilities, dependency rules, and request lifecycle.
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — Technical reference: service/repository conventions, DTO standards, EF Core constraints, validation, and controller rules.
- [`docs/PROJECT_STRUCTURE.md`](docs/PROJECT_STRUCTURE.md) — Current source layout and implemented vs. planned scope.

---

## Technology Stack

| Concern | Technology |
| --- | --- |
| Runtime | .NET 10 / ASP.NET Core 10 |
| Persistence | Entity Framework Core 10, SQLite (local development) |
| Validation | FluentValidation 12 |
| Testing | xUnit, Moq, EF Core SQLite provider |
| DevOps | GitHub Actions (build, release validation, dependency review, NuGet audit, PR validation, labeling, reviewer assignment) |
| Conventions | Nullable reference types, implicit usings, conventional commits, REST over JSON |

---

## Solution Structure

```text
TalebElm/
├── src/
│   ├── TalebElm.Domain/            Entities, Enums, Exceptions, repository interfaces
│   ├── TalebElm.Application/       DTOs, Services/Abstraction, Services/Concrete, Validators
│   ├── TalebElm.Infrastructure/    Persistence/{DbContext, Configurations, Repositories}, Migrations, DI
│   └── TalebElm.Api/               Controllers, Middlewares, Program.cs, appsettings
├── tests/
│   └── TalebElm.Tests/             UnitTests, IntegrationTests, Infrastructure fixtures
├── docs/                           Feature specs, ERD, API matrix, ADRs (OPEN_DECISIONS)
├── .github/workflows/              CI, release validation, security, PR governance
└── TalebElm.slnx                   Solution definition
```

---

## API Surface

Controllers in `src/TalebElm.Api/Controllers`:

| Controller | Scope |
| --- | --- |
| `HealthController` | Liveness probe (`GET /api/health`) |
| `AuthController` | Identity and authentication contracts |
| `UsersController` | User profile management |
| `TracksController` | Track catalog and content administration |
| `ModulesController` | Module sequencing within a track |
| `LessonsController` | Lesson content delivery |
| `ExamsController` | Exam retrieval and submission |
| `ProgressController` | Authenticated learner progress |

Full route matrix, status codes, and implemented-vs-planned markings: [`docs/API_ENDPOINTS.md`](docs/API_ENDPOINTS.md).

---

## Persistence

- Single `AppDbContext` with per-entity `IEntityTypeConfiguration<T>` classes under `Infrastructure/Persistence/Configurations/`.
- SQLite for local development via `ConnectionStrings:TalebElm` (`Data Source=talebelm.db` in `appsettings.Development.json`).
- EF Core migration chain committed under `Infrastructure/Migrations/` with a design-time `AppDbContextFactory`.
- Enforced relational constraints:
  - `UNIQUE(UserId, ModuleId)` on `UserProgress`
  - `UNIQUE(UserId, TrackId)` on `TrackEnrollment`
  - `UNIQUE(TrackId, Order)` on `Module`
  - `UNIQUE(ModuleId, Order)` on `Lesson`
  - `UNIQUE(ModuleId)` on `Exam` (at most one exam per module)
  - `DeleteBehavior.Restrict` on all learning-path foreign keys; learner data is never cascade-deleted. `UnitOfWork.SaveChangesAsync` translates unique and foreign-key violations into `ConflictException`, so a duplicate or a blocked delete returns `409 Conflict`.
- `IUnitOfWork.SaveChangesAsync()` is the single commit point. Application services depend exclusively on `IUnitOfWork`; direct `DbContext` abstractions in Application are not permitted.

See [`docs/DATABASE_ERD.md`](docs/DATABASE_ERD.md) for the entity-relationship model.

---

## Testing Strategy

| Level | Coverage |
| --- | --- |
| Unit | Entity invariants, DTO contracts, validators, service use cases (mocked `IUnitOfWork`) |
| Repository | SQLite-backed persistence tests: configurations, unique indexes, relationship behavior |
| Integration | Controller and relational-constraint tests over a real SQLite database |

```bash
dotnet test TalebElm.slnx
```

Release-mode validation (as executed in CI):

```bash
dotnet test TalebElm.slnx --configuration Release
```

---

## DevOps and Quality Gates

| Workflow | Trigger | Purpose |
| --- | --- | --- |
| `.NET CI` (`ci.yml`) | Push / PR to `main` | Debug restore → build → test on .NET 10 |
| `Release Build & Test` (`dotnet-ci.yml`) | Push / PR to `main` | Validates shippable Release binaries |
| `Security & Vulnerability Scan` (`security-scan.yml`) | PR, weekly schedule, manual dispatch | Dependency-review (fails on high/critical) + full-tree NuGet transitive audit |
| `PR Validation` (`pr-validation.yml`) | PR opened / edited / synchronized | Enforces conventional-commit titles; requires `Closes #NNN` linkage for `feat:` / `fix:` |
| `Pull Request Labeler` (`labeler.yml` + `labeler.yml` rules) | PR events | Path-based automatic labeling |
| `Auto Assign Reviewers` (`auto-assign.yml`) | PR opened / ready for review | Round-robin reviewer rotation |

Conventional-commit types: `feat`, `fix`, `chore`, `refactor`, `docs`, `test`, `ci`, `build`, `perf`, `style` (optional scope, e.g. `fix(auth): hash passwords`).

---

## Getting Started

### Prerequisites

- [.NET 10 SDK](https://dotnet.microsoft.com/download) (`dotnet --version` should report `10.x`)
- Git
- A .NET-compatible IDE or editor (Visual Studio, Visual Studio Code, or JetBrains Rider)

### Clone, build, and run

```bash
git clone https://github.com/Tawjjeh/TalebElm.git
cd TalebElm
dotnet restore
dotnet build
dotnet run --project src/TalebElm.Api
```

Verify liveness:

```text
GET http://localhost:5000/api/health
```

Expected response: service reports healthy.

### Run the test suite

```bash
dotnet test
```

---

## Configuration

| Setting | Source | Notes |
| --- | --- | --- |
| `ConnectionStrings:TalebElm` | `appsettings.Development.json` (dev) / environment (all other environments) | Defaults to `Data Source=talebelm.db` locally |
| `ConnectionStrings__TalebElm` | Environment variable override | Takes precedence over JSON configuration |

Rules:

- `appsettings.json` intentionally carries no connection string; non-development environments must inject it via the environment.
- `*.db`, `*.db-shm`, `*.db-wal` files are git-ignored and must never be committed.
- Secrets and production connection strings must never be stored in `appsettings*.json`.

Full reference: [`docs/LOCAL_SETUP.md`](docs/LOCAL_SETUP.md).

---

## Database Migrations

```bash
# Create a migration
dotnet ef migrations add <MigrationName> --project src/TalebElm.Infrastructure --startup-project src/TalebElm.Infrastructure

# Apply migrations to the configured database
dotnet ef database update --project src/TalebElm.Infrastructure --startup-project src/TalebElm.Infrastructure
```

---

## Documentation Index

| Document | Contents |
| --- | --- |
| [`ARCHITECTURE.md`](ARCHITECTURE.md) | Layer guide, dependency rules, request lifecycle |
| [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) | Technical conventions and contracts reference |
| [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) | Feature-to-implementation status matrix (entry point for product docs) |
| [`docs/MVP_LEARNING_JOURNEY.md`](docs/MVP_LEARNING_JOURNEY.md) | End-to-end learner flow and implementation gaps |
| [`docs/API_ENDPOINTS.md`](docs/API_ENDPOINTS.md) | Route matrix and authorization expectations |
| [`docs/DATABASE_ERD.md`](docs/DATABASE_ERD.md) | Data model and integrity constraints |
| [`docs/PROJECT_STRUCTURE.md`](docs/PROJECT_STRUCTURE.md) | Source layout and scope boundaries |
| [`docs/USER_STORIES.md`](docs/USER_STORIES.md) | Learner and administrator stories |
| [`docs/OPEN_DECISIONS.md`](docs/OPEN_DECISIONS.md) | Pending product and technical decisions |
| [`docs/CLASS_FLOW.md`](docs/CLASS_FLOW.md) | Request flow across layers |
| [`docs/LOCAL_SETUP.md`](docs/LOCAL_SETUP.md) | Connection-string and environment setup |
| [`CONTRIBUTING.md`](CONTRIBUTING.md) | Contribution workflow and quality standards |

---

## Contributing

Contributions follow a fork-based workflow with conventional-commit titles, mandatory checks (build, tests, architecture boundaries), and automated reviewer assignment.

See [`CONTRIBUTING.md`](CONTRIBUTING.md) for the full workflow, branch naming, commit conventions, and PR requirements.

---

## License

Distributed under the **GNU General Public License v3.0**. See [LICENSE](LICENSE) for the full terms.
