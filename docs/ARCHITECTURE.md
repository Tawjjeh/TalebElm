# Technical Architecture Reference

> This is the concise technical reference for contributors already familiar with
> Clean Architecture. For the beginner-friendly explanation, see the root
> [`ARCHITECTURE.md`](../ARCHITECTURE.md).

---

## 1. Layer Responsibilities

```
┌─────────────────────────────────────────────────┐
│  TalebElm.Api          (presentation)           │
│  Controllers, Middlewares, Program.cs            │
├─────────────────────────────────────────────────┤
│  TalebElm.Infrastructure  (implementation)      │
│  Persistence, Repositories, Services            │
├─────────────────────────────────────────────────┤
│  TalebElm.Application     (use cases)           │
│  DTOs, Service Interfaces, Validators           │
├─────────────────────────────────────────────────┤
│  TalebElm.Domain           (core rules)         │
│  Entities, Enums, Exceptions, Repo Interfaces   │
└─────────────────────────────────────────────────┘
```

| Layer | References | Owns |
|---|---|---|
| **Domain** | Nothing | Entities, Enums, Exceptions, Repository interfaces (`IRepository<T>`, `IUnitOfWork`) |
| **Application** | Domain | DTOs (records), Service interfaces (`I<Name>Service`), `IApplicationDbContext`, Validators |
| **Infrastructure** | Application, Domain | `AppDbContext`, EF Core configurations, Repository implementations, Service implementations |
| **Api** | Infrastructure | Controllers, Middlewares, `Program.cs`, `appsettings.json` |

### Dependency rule

Inner layers never reference outer layers. The dependency arrow always points inward:

```
Api → Infrastructure → Application → Domain
```

---

## 2. Service Pattern Conventions

We use the **Interface + Service Class** pattern. No CQRS, no MediatR.

### Naming

| Artifact | Location | Convention | Example |
|---|---|---|---|
| Service interface | `Application/Services/` | `I<Feature>Service` | `IExamService` |
| Service implementation | `Infrastructure/Services/` | `<Feature>Service` | `ExamService` |

### Rules

1. Service interfaces live in `Application/Services/` and expose only DTOs — never entities.
2. Service implementations live in `Infrastructure/Services/` and depend on repository interfaces from Domain.
3. Each service method accepts a request DTO (or primitive) and returns a response DTO.
4. Controllers call service interfaces only. Controllers never access repositories directly.

### Interface shape

```csharp
public interface IExamService
{
    Task<ExamResponse> CreateAsync(CreateExamRequest request);
    Task<ExamResponse> GetByIdAsync(Guid id);
    Task<ExamResultResponse> SubmitAsync(SubmitExamRequest request);
}
```

### Registration

Service implementations are registered in `Program.cs` via DI:

```csharp
builder.Services.AddScoped<IExamService, ExamService>();
```

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
    Task<int> SaveChangesAsync();
}
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

`ExceptionHandlingMiddleware` (in `Api/Middlewares/`) catches domain exceptions
and maps them to standard HTTP responses:

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
| DbContext | `Infrastructure/Persistence/AppDbContext.cs` | Single context for all entities |
| Configuration | `Infrastructure/Persistence/<Entity>Configuration.cs` | One `IEntityTypeConfiguration<T>` per entity |

Configurations define field constraints, relationships, indexes, and FK behavior.
`AppDbContext` applies all configurations via `OnModelCreating`.

---

## 7. Cross-Team Contract Boundaries

The codebase splits ownership across two conceptual teams:

| Boundary | Owns | Key files |
|---|---|---|
| **DB / Domain Team** | Entities, Enums, Exceptions, Repository Interfaces, EF Configurations | `Domain/Entities/*`, `Domain/Interfaces/*`, `Infrastructure/Persistence/*` |
| **API / Application Team** | DTOs, Service Interfaces, Service Implementations, Controllers, Middleware | `Application/DTOs/*`, `Application/Services/*`, `Infrastructure/Services/*`, `Api/Controllers/*` |

### Handoff rules

- The DB/Domain team defines entities and their repository contracts.
- The API/Application team consumes those contracts via service interfaces.
- DTOs are **shared** — both teams agree on request/response shapes.
- Neither team modifies the other's entities or controllers without discussion.

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
