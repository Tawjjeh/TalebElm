# Coding Standards

TalebElm targets .NET 10 with nullable contexts enabled, warnings-as-errors
preferred for new code. These standards extend `ARCHITECTURE.md`.

---

## 1. General style

- File-scoped namespaces.
- One public type per file; `sealed` by default for concrete service and repository classes.
- Async methods end in `Async`, accept `CancellationToken` as the last parameter, and do not use `async void` outside event handlers.
- Express time as UTC; resolve clocks through injectable abstractions — never call `DateTime.UtcNow` directly in domain or application code.
- Never call `.Result`, `.Wait()`, or fire-and-forget tasks without a tracked background mechanism.
- `record` types for immutable DTOs; `class` types for entities and services.
- Structured logging only; never interpolate secrets, tokens, or user data into log messages.

---

## 2. Architecture boundaries

- `Application` depends on `IUnitOfWork`, **never** on `DbContext` or any EF Core type.
- Controllers are thin: bind input, call an Application service interface, translate to HTTP. No business logic in action methods.
- EF Core mappings, migrations, and repository implementations live exclusively in `Infrastructure`.
- Do not add repository properties to `IUnitOfWork` until both the Domain interface and Infrastructure implementation are present and tested.
- External service clients (`ICodeforcesClient`, `IGitHubClient`) are Application interfaces; their implementations live in Infrastructure. Tests use fakes — no real network calls.

---

## 3. Naming conventions

| Artifact | Location | Convention | Example |
|---|---|---|---|
| Service interface | `Application/Services/Abstraction/` | `I<Feature>Service` | `IExamService` |
| Service implementation | `Application/Services/Concrete/` | `<Feature>Service` | `ExamService` |
| Repository interface | `Domain/Interfaces/` | `I<Entity>Repository` | `IModuleRepository` |
| Repository implementation | `Infrastructure/Persistence/Repositories/` | `<Entity>Repository` | `ModuleRepository` |
| EF configuration | `Infrastructure/Persistence/Configurations/` | `<Entity>Configuration` | `ExamConfiguration` |
| DTO (request) | `Application/DTOs/` | `Create<Entity>Request`, `Submit<Entity>Request` | `CreateTrackRequest` |
| DTO (response) | `Application/DTOs/` | `<Entity>Response`, `<Entity>ResultResponse` | `TrackResponse` |
| Validator | `Application/Validators/` | `<Request>Validator` | `CreateTrackRequestValidator` |

---

## 4. Persistence rules

- Migrations are explicit, reviewed as code, and committed under `Infrastructure/Migrations/`.
- Every migration must be accompanied by updated model snapshots; never edit a migration after applying it without reverting first.
- Use `DeleteBehavior.Restrict` for all learning-path foreign keys (see [`docs/business/BUSINESS_RULES.md`](../business/BUSINESS_RULES.md)).
- Use parameterized queries (EF LINQ); never concatenate user input into SQL.
- `UnitOfWork.SaveChangesAsync()` is the **single commit point**. Do not call `DbContext.SaveChangesAsync()` from repositories.
- `UnitOfWork.SaveChangesAsync()` translates `DbUpdateException` for unique-index and FK violations into `ConflictException` (`409`); the Application layer never sees raw EF exceptions.

---

## 5. Error handling

- Use the project's domain exception hierarchy:
  - `NotFoundException` → 404
  - `ValidationException` → 400
  - `ConflictException` → 409
  - `DomainException` → 400 (base for other business errors)
  - `NotImplementedException` (domain) → 501 (placeholder only)
- Do not leak provider details, stack traces, or user data in problem-detail responses.
- `ExceptionHandlingMiddleware` handles the mapping; do not duplicate HTTP status translation in service code.

---

## 6. DTO rules

- DTOs are `record` types — immutable, with positional or named parameters.
- Never serialize `IsCorrect` or any server-side answer key to learners.
- `UserResponse` must expose only `Id`, `Name`, and `Email` — never `PasswordHash` or `Role` in a learner-facing response. A reflection test enforces this.
- Response DTOs must not include EF navigation properties or internal entity state.

---

## 7. Validation

- All input validation uses **FluentValidation 12**.
- Validators live in `Application/Validators/` and are registered by `AddApplication()`.
- Validate at the Application boundary; controllers may also use `[ApiController]` model binding for basic binding errors.
- Field-keyed `400` error responses (target: issue #321) will expose individual field errors; until that lands, validation failures return a single `detail` string.

---

## 8. CancellationToken

New code must propagate `CancellationToken` through the entire call stack:

- Controller actions pass `HttpContext.RequestAborted` to the Application service.
- Application services pass it through to `IUnitOfWork` methods and external client calls.
- Repository methods accept and forward the token to EF Core async methods.

---

## 9. Tests

- Add or update tests for every behavior change; see [`testing.md`](testing.md).
- Failed tests must be reported with the exact command output — never claim tests passed without running them.
- Do not add a new public service method without at least one unit test covering the happy path.

---

## 10. What requires a documented decision

The following changes must be recorded in [`docs/OPEN_DECISIONS.md`](../OPEN_DECISIONS.md) before code is written:

- A new external dependency (new NuGet package or external service).
- A change to an approved business rule in `docs/business/BUSINESS_RULES.md`.
- A new domain entity or relationship.
- A breaking change to the API contract (status codes, route paths, required fields).
- A change to the database schema that is not expand/contract safe.
