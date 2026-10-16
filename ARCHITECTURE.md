# TalebElm Architecture Guide

This document defines the architectural boundaries, dependency rules, and request lifecycle for the TalebElm backend platform. It is the conceptual companion to the normative technical reference in [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).

---

## 1. Architectural style

TalebElm implements **Clean Architecture**: the system is decomposed into concentric layers, each with a single, well-defined responsibility. Dependencies point **inward only** — outer layers may depend on inner layers; inner layers never depend on outer layers.

The core invariant:

> Each layer owns exactly one concern, and dependencies always point toward the Domain.

This separation keeps business rules independent of persistence technology and delivery mechanism, enabling the Domain and Application layers to evolve without being coupled to Entity Framework Core, ASP.NET Core, or any external service.

---

## 2. Layer overview

```text
                   +---------------------+
                   |         Api         |   Presentation / HTTP
                   +---------------------+
                        |        |
               +---------------------+
               |   Infrastructure    |   Persistence and external integrations
               +---------------------+
                        |        |
               +---------------------+
               |    Application      |   Use cases and orchestration
               +---------------------+
                        |        |
               +---------------------+
               |       Domain        |   Business rules and contracts
               +---------------------+

   Tests are cross-cutting and may reference any production layer.
```

| Project | Path | Responsibility |
| --- | --- | --- |
| Domain | `src/TalebElm.Domain` | Entities, enums, domain exceptions, repository interfaces |
| Application | `src/TalebElm.Application` | Service interfaces and implementations, DTOs, validators |
| Infrastructure | `src/TalebElm.Infrastructure` | `AppDbContext`, entity configurations, repository implementations, `UnitOfWork`, DI wiring |
| Api | `src/TalebElm.Api` | Controllers, middleware, composition root (`Program.cs`) |
| Tests | `tests/TalebElm.Tests` | Unit, repository, and integration verification |

---

## 3. Layer 1 — Domain (business rules)

**Path:** `src/TalebElm.Domain`

The Domain layer models the platform's core concepts — `User`, `Track`, `Module`, `Lesson`, `Exam`, `UserProgress` — along with their invariants, enumerations (`TrackStatus`, `LessonType`, `UserRole`), domain exceptions, and repository contracts (`IRepository<T>`, entity-specific interfaces, `IUnitOfWork`).

Containment rules:

- Entities express state and invariants only (e.g., required names, ordering, uniqueness expectations).
- Persistence needs are expressed as **interfaces**, never implementations.
- The Domain references no other project and imports neither Entity Framework Core nor ASP.NET Core.

Anti-patterns: persistence logic inside entities, HTTP or serialization concerns in entities, direct dependencies on infrastructure types.

---

## 4. Layer 2 — Application (use cases)

**Path:** `src/TalebElm.Application`

The Application layer implements the platform's use cases — for example, create/track retrieval, exam submission, progress evaluation — as service classes behind explicit interfaces:

- **Interfaces** (`Services/Abstraction/`): `IUserService`, `ITrackService`, `IExamService`, `IUserProgressService`.
- **Implementations** (`Services/Concrete/`): use-case orchestration depending only on Domain abstractions, normally `IUnitOfWork`. All business-logic decisions reside here.
- **DTOs** (`DTOs/`): immutable `record` request/response contracts decoupling the HTTP surface from Domain entities.
- **Validators** (`Validators/`): FluentValidation input rules.

Containment rules:

- No Entity Framework Core references (`DbContext`, `EntityTypeBuilder`).
- No ASP.NET Core references (`HttpContext`, controllers).
- No persistence technology assumptions (SQL Server vs. SQLite vs. managed storage are Infrastructure decisions).
- Persistence access exclusively through `IUnitOfWork`; a separate `DbContext` abstraction in Application services is redundant and prohibited. `IUnitOfWork.SaveChangesAsync()` is the sole commit point.

---

## 5. Layer 3 — Infrastructure (persistence and integration)

**Path:** `src/TalebElm.Infrastructure`

Infrastructure provides the concrete implementations requested by the inner layers:

- `Persistence/DbContext/AppDbContext` — the single EF Core context.
- `Persistence/Configurations/` — one `IEntityTypeConfiguration<T>` per entity (column constraints, relationships, `DeleteBehavior.Restrict`, unique indexes).
- `Persistence/Repositories/` — repository implementations plus the `UnitOfWork` commit point.
- `DependencyInjection.cs` — persistence and repository service registration.
- `Migrations/` — committed EF Core migrations with a design-time context factory.
- External integrations (messaging, file storage, email) when introduced.

Containment rules: no business-rule decisions, no use-case orchestration, no controllers. Infrastructure answers **how** data is stored and retrieved; it does not define **what** the business rules are.

---

## 6. Layer 4 — Api (presentation)

**Path:** `src/TalebElm.Api`

The Api layer is a thin HTTP adapter over Application use cases:

- Controllers (`Controllers/`) translate HTTP requests into service calls and map results to HTTP responses.
- Middleware (`Middlewares/`) implements cross-cutting pipeline behavior, including centralized domain-exception → status-code mapping.
- `Program.cs` is the composition root: service registration, middleware ordering, and endpoint mapping.
- `appsettings*.json` carries environment-specific, non-secret configuration.

Containment rules: no business logic, no direct repository or `DbContext` access, no persistence decisions. Controller actions delegate to injected `I<Feature>Service` contracts and remain free of domain orchestration.

---

## 7. Layer 5 — Tests (verification)

**Path:** `tests/TalebElm.Tests`

The test suite verifies behavior at three levels:

- **Unit** — entity invariants, DTO contracts, validators, and service logic against mocked `IUnitOfWork`.
- **Repository** — SQLite-backed verification of configurations, constraints, and relationship behavior.
- **Integration** — controller and cross-layer relational-constraint coverage.

Tests are intentionally placed outside the production dependency graph: they may reference any layer, while no production project references Tests.

```bash
dotnet test TalebElm.slnx
```

---

## 8. Project references

Dependency direction is enforced at the project level:

1. **Domain** references nothing.
2. **Application** references **Domain**.
3. **Infrastructure** references **Application** and **Domain**.
4. **Api** references **Infrastructure** (transitively reaching Application and Domain).
5. **Tests** may reference **Api**, **Application**, and **Domain** for verification purposes.

These references are declared in the respective `.csproj` files and must not be inverted or bypassed.

---

## 9. Request lifecycle

Example — `GET /api/tracks` (list tracks):

1. **Api** receives the HTTP request and binds/validates input.
2. **Api** delegates to the injected Application service (e.g., `ITrackService`).
3. **Application** executes the use case, querying persistence through `IUnitOfWork` without any EF Core coupling.
4. **Infrastructure** executes the query via its repository implementation and returns mapped entities.
5. **Application** maps the result to response DTOs and returns them.
6. **Api** serializes the DTOs to JSON with the appropriate status code.

Domain rules participate at every step; no layer bypasses its adjacent boundary.

---

## 10. Placement heuristic

| Concern | Location |
| --- | --- |
| Business invariant, entity, or repository contract | Domain |
| Use case, orchestration, DTO, or validator | Application |
| EF Core mapping, repository implementation, or external integration | Infrastructure |
| HTTP endpoint, middleware, or composition-root wiring | Api |
| Behavioral verification | Tests |

For normative conventions — service and repository patterns, DTO naming, EF Core constraint catalog, validation, controller standards, and current implementation status — see [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) and [`docs/PROJECT_STRUCTURE.md`](docs/PROJECT_STRUCTURE.md).
