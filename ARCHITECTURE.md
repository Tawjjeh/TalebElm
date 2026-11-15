# TalebElm architecture

TalebElm uses **Clean Architecture**. Code is split into four layers, and dependencies point
inward: an outer layer may use an inner layer, never the other way round. This keeps business
rules independent of the database and of HTTP.

## 1. The layers

```text
Api             controllers, middleware, Program.cs, appsettings
  ↓
Infrastructure  EF Core, configurations, repositories, external HTTP clients
  ↓
Application     services, DTOs, validators, client interfaces
  ↓
Domain          entities, enums, exceptions, repository interfaces
```

| Project | Path | Owns |
|---|---|---|
| Domain | `src/TalebElm.Domain` | Entities, enums, domain exceptions, repository interfaces, `IUnitOfWork` |
| Application | `src/TalebElm.Application` | Service interfaces and implementations, DTOs, validators, external client interfaces |
| Infrastructure | `src/TalebElm.Infrastructure` | `AppDbContext`, entity configurations, repositories, `UnitOfWork`, DI wiring |
| Api | `src/TalebElm.Api` | Controllers, middleware, composition root |
| Tests | `tests/TalebElm.Tests` | Unit, repository, and integration tests |

Project references:

1. **Domain** references nothing.
2. **Application** references **Domain**.
3. **Infrastructure** references **Application** and **Domain**.
4. **Api** references **Infrastructure**, which brings Application and Domain with it.
5. **Tests** may reference any production project. No production project references Tests.

## 2. What each layer may and may not do

**Domain** describes the business. Entities hold state and their own rules, enums name fixed sets
of values, exceptions describe failures, and repository interfaces say what data is needed. Domain
never imports Entity Framework Core or ASP.NET Core.

**Application** runs use cases. A service loads what it needs through `IUnitOfWork`, applies the
rules, and returns DTOs. It never sees `DbContext`, `HttpContext`, or SQL.

**Infrastructure** answers "how is data stored". It maps entities to tables, implements the
repository interfaces, and calls external services over HTTP. It contains no business rules.

**Api** translates HTTP. A controller binds input, calls one Application service, and returns a
status code. Business decisions stay out of controllers.

**Tests** prove the behavior offline. EF Core tests use in-memory SQLite, external services use
fakes, and nothing touches the network.

## 3. One request, step by step

Example: `GET /api/tracks?page=1`.

1. **Api** binds the query string and calls `ITrackService.GetPagedAsync`.
2. **Application** validates the paging values and asks `IUnitOfWork.Tracks` for the page.
3. **Infrastructure** runs the query through EF Core and returns the entities.
4. **Application** maps entities to `TrackResponse` records.
5. **Api** serializes the records and returns `200 OK`.

If a rule fails, the service throws a domain exception and `ExceptionHandlingMiddleware` turns it
into the right HTTP status. No layer skips the one next to it.

## 4. Where does this code go?

| The code is about… | Put it in |
|---|---|
| A business rule, entity, enum, or repository contract | Domain |
| A use case, its orchestration, a DTO, or an input validator | Application |
| A table mapping, query, migration, or external API call | Infrastructure |
| An HTTP route, middleware, or startup wiring | Api |
| Proof that behavior works | Tests |

## 5. Services

The pattern is one interface plus one class. There is no CQRS and no MediatR.

| Artifact | Location | Name | Example |
|---|---|---|---|
| Interface | `Application/Services/Abstraction/` | `I<Feature>Service` | `IExamService` |
| Implementation | `Application/Services/Concrete/` | `<Feature>Service` | `ExamService` |

Rules:

1. Interfaces expose DTOs or plain values, never persistence types.
2. Implementations depend on `IUnitOfWork` and other Application contracts only.
3. Dependencies arrive through the constructor. Current services use a primary constructor:
   `public class TrackService(IUnitOfWork unitOfWork, IValidator<CreateTrackRequest> validator)`.
4. Services accept and return DTOs. `HttpContext` and user IDs are passed as values, never as
   framework objects.
5. Controllers call services. They never touch repositories or `DbContext`.

Registration lives in two places, and `Program.cs` calls only these:

```csharp
builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddApplication();
```

`AddApplication()` registers services and FluentValidation validators; `AddInfrastructure()`
registers the `DbContext`, repositories, and `IUnitOfWork`.

## 6. Repositories and the unit of work

```csharp
public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync(CancellationToken ct = default);
    Task AddAsync(T entity, CancellationToken ct = default);
}
```

Each entity gets an interface that inherits it, plus any query methods it needs:

```csharp
public interface ITrackRepository : IRepository<Track>
{
    Task<(IReadOnlyList<Track> Items, int TotalCount)> GetPagedAsync(
        int page, int pageSize, bool publishedOnly, CancellationToken ct = default);
}
```

`IUnitOfWork` exposes the repositories as properties and is the only place that saves:

```csharp
public interface IUnitOfWork
{
    IUserRepository Users { get; }
    ITrackRepository Tracks { get; }
    // … one property per repository …
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
```

Rules:

- Repositories add and query. `SaveChangesAsync()` is the single commit point, so several changes
  are written together or not at all.
- Application services never inject a `DbContext` or any DbContext-shaped interface. `IUnitOfWork`
  is the seam; it is also the one thing tests have to mock.
- Add a repository property only when its interface and implementation exist.

## 7. Entity Framework Core

| Artifact | Location |
|---|---|
| Context | `Infrastructure/Persistence/DbContext/AppDbContext.cs` |
| Configurations | `Infrastructure/Persistence/Configurations/<Entity>Configuration.cs` |
| Repositories | `Infrastructure/Persistence/Repositories/<Entity>Repository.cs` |
| Migrations | `Infrastructure/Migrations/` |

Rules:

1. One `IEntityTypeConfiguration<T>` per entity, applied in `OnModelCreating`.
2. Name the `DbSet` in plural. Entity Framework uses that name as the table name, so
   `DbSet<UserProblemCompletion> UserProblemCompletions` creates `UserProblemCompletions`, not
   `userProblemCompletions`.
3. Generate migrations from `TalebElm.Infrastructure`, with Infrastructure as both project and
   startup project:

   ```bash
   dotnet ef migrations add <Name> \
     --project src/TalebElm.Infrastructure \
     --startup-project src/TalebElm.Infrastructure
   ```

4. Never edit a migration that has already run on a real database. Add a new one.
5. Use `EnsureCreated` in tests only, never in the app.

Constraints that must stay in place:

| Table | Constraint | Means |
|---|---|---|
| `Modules` | `UNIQUE(TrackId, Order)` | No two Modules share an order in one Track |
| `Lessons` | `UNIQUE(ModuleId, Order)` | No two Lessons share an order in one Module |
| `Exams` | `UNIQUE(ModuleId)` | At most one Exam per Module |
| `TrackEnrollments` | `UNIQUE(UserId, TrackId)` | One enrollment per learner per Track |
| `UserProgresses` | `UNIQUE(UserId, ModuleId)` | One progress row per learner per Module |
| `UserExternalAccounts` | `UNIQUE(UserId, Platform)` | One linked account per platform |
| `UserProblemCompletions` | `UNIQUE(UserId, ModuleProblemId)` | One completion per learner per problem |
| `CapstoneProjects` | `UNIQUE(TrackId)`, `UNIQUE(TaskId)` | One capstone per Track, one per task |
| `ProjectMilestones` | `UNIQUE(ProjectId, Order)`, `UNIQUE(ProjectId, WorkflowJobName)` | Milestones are ordered and named once |

Learner data is never deleted by cascade. Every learning-path foreign key uses
`DeleteBehavior.Restrict`, so a blocked delete fails in the database and becomes `409 Conflict`
(see [`docs/DATABASE_ERD.md`](docs/DATABASE_ERD.md)).

## 8. Errors

One exception base type, one middleware, one status per kind of failure:

```text
Exception
└── DomainException                 400 Bad Request
     ├── ValidationException         400 Bad Request (invalid input)
     ├── NotFoundException           404 Not Found
     ├── ConflictException           409 Conflict (duplicate or blocked delete)
     └── NotImplementedException     501 Not Implemented (deliberate placeholder)
```

Throw the most specific one where the rule fails:

```csharp
var track = await unitOfWork.Tracks.GetByIdAsync(id, ct)
    ?? throw new NotFoundException($"Track {id} not found.");
```

`ExceptionHandlingMiddleware` writes an `application/problem+json` body. Validation failures add
an `errors` object keyed by camelCase property name, so a client can highlight the right field.
`409` is also returned when the database rejects a duplicate or a blocked delete; `UnitOfWork`
translates those failures without leaking the provider message.

Use `TalebElm.Domain.Exceptions.NotImplementedException` only for a deliberate placeholder, not for
"not written yet".

## 9. DTOs

| Kind | Name | Example |
|---|---|---|
| Create input | `Create<Entity>Request` | `CreateTrackRequest` |
| Update input | `Update<Entity>Request` | `UpdateLessonRequest` |
| Submit input | `Submit<Entity>Request` | `SubmitExamAnswersRequest` |
| Read output | `<Entity>Response` | `TrackResponse` |
| Result output | `<Entity>ResultResponse` | `ExamResultResponse` |

DTOs are `record` types, immutable, and live in `Application/DTOs/`. They never expose an entity,
a navigation property, or a server-only field such as `ExamOption.IsCorrect`.

## 10. Controllers

- Derive from `ControllerBase` and carry `[ApiController]`.
- Route prefix is `[Route("api/[controller]")]`.
- Actions return `IActionResult` or `ActionResult<T>`.
- An action calls one service method, then returns. No loops over repositories, no rules.
- Protected routes get an explicit authorization policy; anonymous routes say so.

## 11. Validation

Input rules use **FluentValidation**. One validator per request DTO, in
`Application/Validators/`, registered automatically by `AddApplication()`:

```csharp
public class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    public CreateTrackRequestValidator()
    {
        RuleFor(x => x.Name).NotEmpty().MaximumLength(150);
        RuleFor(x => x.Description).NotEmpty().MaximumLength(1000);
    }
}
```

Set an upper bound on every string. Length rules and database column lengths must agree; when they
disagree the database wins and the request fails with a `500` instead of a `400`.

## 12. External services and cancellation

External APIs are interfaces in Application (`ICodeforcesClient`, `IGitHubClient`), implemented in
Infrastructure with a typed `HttpClient`. Business rules stay in Application, and tests use fakes
so no test reaches the network.

Every new async method takes `CancellationToken ct = default` as its last parameter and passes it
down to EF Core or the HTTP call. Controllers pass `HttpContext.RequestAborted`.

## 13. Tests

| Level | What it checks |
|---|---|
| Unit | Entity rules, DTO contracts, validators, service logic with a mocked `IUnitOfWork` |
| Repository / configuration | EF Core mapping, constraints, indexes, delete behavior, using in-memory SQLite |
| Integration | Controller behavior, middleware, dependency injection, the full migration chain |

Run everything with `dotnet test TalebElm.slnx`; details are in
[`docs/development/testing.md`](docs/development/testing.md).
