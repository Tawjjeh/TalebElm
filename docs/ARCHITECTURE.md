# Technical Architecture Reference

> This is the normative technical reference for implementation conventions.
> For the conceptual layer guide and request lifecycle, see the root
> [`ARCHITECTURE.md`](../ARCHITECTURE.md).

---

## 1. Layer Responsibilities

```
┌─────────────────────────────────────────────────┐
│  TalebElm.Api          (presentation)           │
│  Controllers, Middlewares, Program.cs            │
├─────────────────────────────────────────────────┤
│  TalebElm.Infrastructure  (implementation)      │
│  Persistence (DbContext, Configurations,        │
│  Repositories), DI wiring                       │
├─────────────────────────────────────────────────┤
│  TalebElm.Application     (use cases)           │
│  DTOs, Service Interfaces + Implementations,    │
│  Validators                                    │
├─────────────────────────────────────────────────┤
│  TalebElm.Domain           (core rules)         │
│  Entities, Enums, Exceptions, Repo Interfaces   │
└─────────────────────────────────────────────────┘
```

| Layer | References | Owns |
|---|---|---|
| **Domain** | Nothing | Entities, Enums, Exceptions, Repository interfaces (`IRepository<T>`, `IUnitOfWork`) |
| **Application** | Domain | DTOs (records), Service interfaces (`I<Name>Service`) + service implementations (`<Name>Service`), Validators |
| **Infrastructure** | Application, Domain | `AppDbContext`, EF Core configurations, Repository implementations, DI registration |
| **Api** | Infrastructure | Controllers, Middlewares, `Program.cs`, `appsettings.json` |

### Dependency rule

Inner layers never reference outer layers. The dependency arrow always points inward:

```
Api → Infrastructure → Application → Domain
```

## Current Implementation Status

The layers and contracts are present, but the full learning journey is not wired end to end.

| Area | Current state |
|---|---|
| Domain | User, Track, Module, Lesson, Exam, and UserProgress entities exist. Enrollment, LessonResource, and ExamQuestion do not. |
| Application | DTOs and service interfaces for User, Track, Exam, and progress exist. |
| Infrastructure | SQLite registration, repositories for Track/Module/Exam, UnitOfWork, and parts of Track/Exam services exist. Several methods remain placeholders. |
| API | Controllers are mostly placeholders. `Program.cs` registers infrastructure, `IUnitOfWork`, and Application services; feature controllers do not yet delegate to service interfaces. There is no enrollment endpoint or authentication setup. |
| Tests | SQLite-backed repository and service tests exist, but the full register → enroll → study → exam → unlock journey is not tested or implemented. |

See [`MVP_LEARNING_JOURNEY.md`](MVP_LEARNING_JOURNEY.md) for the target learner flow and [`FEATURE_INDEX.md`](FEATURE_INDEX.md) for feature status.

---

## 2. Service Pattern Conventions

We use the **Interface + Service Class** pattern. No CQRS, no MediatR.

### Naming

| Artifact | Location | Convention | Example |
|---|---|---|---|
| Service interface | `Application/Services/Abstraction/` | `I<Feature>Service` | `IExamService` |
| Service implementation | `Application/Services/Concrete/` | `<Feature>Service` | `ExamService` |

### Rules

1. Service interfaces live in `Application/Services/Abstraction/` and expose DTOs or explicit identifiers — never persistence types.
2. Service implementations (business logic / use cases) live in `Application/Services/Concrete/`. They depend only on Application contracts plus Domain repository contracts, normally through `IUnitOfWork`. They must never reference EF Core (`DbContext`, `EntityTypeBuilder`) or ASP.NET Core (`HttpContext`) types.
3. Infrastructure owns persistence only: `AppDbContext`, `IEntityTypeConfiguration<T>` classes, repository implementations, and DI wiring. It must never contain business logic or use-case orchestration.
4. Service methods accept request DTOs or explicit values such as IDs and return response DTOs. Do not pass `HttpContext` into Application.
5. Controllers call service interfaces only. Controllers must not access repositories or `AppDbContext` directly.

### Interface shape

```csharp
public interface IExamService
{
    Task<ExamResponse> CreateAsync(CreateExamRequest request);
    Task<ExamResponse> GetByIdAsync(Guid id);
    Task<ExamResultResponse> SubmitAsync(SubmitExamRequest request);
}
```

### Registration (Target)

Service implementations are registered in `Program.cs` via DI:

```csharp
builder.Services.AddScoped<IExamService, ExamService>();
```

**Current status:** `Program.cs` registers `AddInfrastructure`, `IUnitOfWork`, Application services, and the track request validator, and adds `ExceptionHandlingMiddleware` to the pipeline. The existing feature controllers do not yet delegate to service interfaces. Treat controller delegation as the required pattern, not as current behavior.

---

## 3. Repository Pattern

### Generic base

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
}
```

### Specific repositories

Each entity gets a dedicated interface inheriting `IRepository<T>`:

```csharp
public interface ITrackRepository : IRepository<Track> { }
```

Custom query methods are added to the specific interface when needed.

### Unit of Work

`IUnitOfWork` aggregates repositories and exposes `SaveChangesAsync()`:

```csharp
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ITrackRepository Tracks { get; }
    IModuleRepository Modules { get; }
    IExamRepository Exams { get; }
    Task<int> SaveChangesAsync();
}
```

Repositories should share one scoped `AppDbContext`. Repository `AddAsync` methods add entities to that context; `UnitOfWork.SaveChangesAsync()` is the commit point. Add a repository property only when its repository contract and implementation are ready.

### Data-access rule: `IUnitOfWork` only (no `IApplicationDbContext`)

Application services must access persistence exclusively through `IUnitOfWork` (Domain contract). A direct `IApplicationDbContext` / `IAppDbContext` dependency in application services is redundant and forbidden:

- `IUnitOfWork` already exposes the required repositories plus the single `SaveChangesAsync()` commit point, keeping multi-repository updates atomic.
- Depending on a DbContext abstraction leaks EF Core specifics (change tracking, `DbSet<T>`, queryable surface) into the Application layer and defeats the repository boundary.
- Tests then mock one seam (`IUnitOfWork` + `IRepository<T>`) instead of a DbContext-shaped API.

```csharp
// Correct: Application service depends on the Domain Unit of Work.
public class TrackService(IUnitOfWork unitOfWork) : ITrackService { ... }

// Forbidden: do not inject a DbContext abstraction into Application services.
// public class TrackService(IApplicationDbContext db) : ITrackService { ... }
```

---

## 4. Error Handling & Domain Exceptions

### Exception hierarchy

```
System.Exception
 └── DomainException              → 400 Bad Request (general domain error)
      ├── NotFoundException       → 404 Not Found
      ├── ValidationException     → 400 Bad Request (input validation)
      └── NotImplementedException → 501 Not Implemented (placeholder)
```

### HTTP mapping strategy

The target `ExceptionHandlingMiddleware` (in `Api/Middlewares/`) should catch domain exceptions
and map them to standard HTTP responses:

| Exception | HTTP Status | When to throw |
|---|---|---|
| `NotFoundException` | 404 | Entity lookup returned `null` |
| `ValidationException` | 400 | Business rule violated (e.g., duplicate email) |
| `DomainException` | 400 | Any other business error |
| `NotImplementedException` | 501 | Placeholder for unfinished features |

### Usage pattern

```csharp
var track = await _unitOfWork.Tracks.GetByIdAsync(id)
    ?? throw new NotFoundException($"Track {id} not found.");
```

**Current status:** `ExceptionHandlingMiddleware` maps domain exceptions to `application/problem+json` responses and is registered in `Program.cs`. Use the custom `TalebElm.Domain.Exceptions.NotImplementedException` only when a deliberate placeholder response is needed; do not confuse it with `System.NotImplementedException`.

---

## 5. DTO Conventions

| Type | Naming | Location |
|---|---|---|
| Create input | `Create<Entity>Request` | `Application/DTOs/` |
| Update input | `Update<Entity>Request` | `Application/DTOs/` |
| Submit input | `Submit<Entity>Request` | `Application/DTOs/` |
| Read output | `<Entity>Response` | `Application/DTOs/` |
| Result output | `<Entity>ResultResponse` | `Application/DTOs/` |

DTOs are C# `record` types — immutable, with positional parameters.

```csharp
public record CreateTrackRequest(string Name, string Description);
public record TrackResponse(Guid Id, string Name, string Description, int Status);
```

---

## 6. EF Core Persistence

| Artifact | Location | Convention |
|---|---|---|
| DbContext | `Infrastructure/Persistence/DbContext/AppDbContext.cs` | Single context for all entities |
| Configuration | `Infrastructure/Persistence/Configurations/<Entity>Configuration.cs` | One `IEntityTypeConfiguration<T>` per entity |
| Repository implementation | `Infrastructure/Persistence/Repositories/<Entity>Repository.cs` | Implements the Domain repository contract via `AppDbContext` |

Configurations define field constraints, relationships, indexes, and FK behavior.
`AppDbContext` applies all configurations via `OnModelCreating`.

### Mandatory MVP constraints

- `UNIQUE(UserId, ModuleId)` on `UserProgress` (one progress row per learner + Module).
- `UNIQUE(TrackId, Order)` on `Module` (no two Modules share an `Order` within one Track).
- `UNIQUE(ModuleId, Order)` on `Lesson` (no two Lessons share an `Order` within one Module).
- `UNIQUE(ModuleId)` on `Exam` (at most one Exam per Module).
- `UNIQUE(UserId, TrackId)` on `TrackEnrollment` (one enrollment per learner + Track, planned entity).
- All learning-path foreign keys use `DeleteBehavior.Restrict` (see `DATABASE_ERD.md` §5); deletes that would orphan dependent data fail with `409 Conflict` instead of cascading.

---

## 7. Cross-Team Contract Boundaries

The codebase splits ownership across two conceptual teams:

| Boundary | Owns | Key files |
|---|---|---|
| **DB / Domain Team** | Entities, Enums, Exceptions, Repository Interfaces, EF Configurations | `Domain/Entities/*`, `Domain/Interfaces/*`, `Infrastructure/Persistence/Configurations/*`, `Infrastructure/Persistence/DbContext/*` |
| **API / Application Team** | DTOs, Service Interfaces + Implementations, Validators, Controllers, Middleware | `Application/DTOs/*`, `Application/Services/Abstraction/*`, `Application/Services/Concrete/*`, `Api/Controllers/*` |

### Handoff rules

- The DB/Domain team defines entities and their repository contracts.
- The API/Application team consumes those contracts via service interfaces.
- DTOs are **shared** — both teams agree on request/response shapes.
- Neither team modifies the other's entities or controllers without discussion.
- Keep planned fields and relationships labeled as proposed until the Domain contract is merged.
- API endpoint docs must distinguish implemented routes from planned routes; a controller class alone does not mean the use case works.

---

## 8. Controller Conventions

| Rule | Detail |
|---|---|
| Base class | `ControllerBase` (API-only, no view support) |
| Route prefix | `[Route("api/[controller]")]` |
| Attributes | `[ApiController]` on every controller |
| Action returns | `IActionResult` or `ActionResult<T>` |
| Thin controllers | Controllers delegate to service interfaces; no business logic in actions |

---

## 9. Validation

Input validation uses **FluentValidation** (when enabled):

```csharp
public class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    public CreateTrackRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty();
        RuleFor(x => x.Description).NotEmpty();
    }
}
```

Validators live in `Application/Validators/` and are registered via DI in `Program.cs`.
