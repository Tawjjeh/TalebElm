# TalebElm Project Structure

This is the current source layout, not a promise that every MVP workflow is implemented. Planned behavior is called out explicitly below.

## Current Solution

```text
TalebElm/
├── src/
│   ├── TalebElm.Domain/
│   │   ├── Entities/        BaseEntity, User, Track, Module, Lesson, LessonResource,
│   │   │                    Exam, TrackEnrollment, UserProgress
│   │   ├── Enums/           TrackStatus, LessonType, LessonResourceKind, UserRole
│   │   ├── Exceptions/      DomainException, NotFoundException, ValidationException,
│   │   │                    ConflictException, NotImplementedException
│   │   └── Interfaces/      Repository contracts and IUnitOfWork
│   ├── TalebElm.Application/
│   │   ├── DTOs/            User, Track, Module, Lesson, Resource, Exam, and Progress contracts
│   │   ├── Services/Abstraction/  IUserService, ITrackService, IModuleService, ILessonService,
│   │   │                          IExamService, IUserProgressService, IPasswordHasher
│   │   ├── Services/Concrete/     Service implementations and PasswordHasher
│   │   └── Validators/      Track, Module, Lesson, and basic Exam request validators
│   ├── TalebElm.Infrastructure/
│   │   ├── Persistence/DbContext/        AppDbContext
│   │   ├── Persistence/Configurations/   Per-entity EF Core configurations
│   │   ├── Persistence/Repositories/     Repository implementations and UnitOfWork
│   │   ├── Migrations/                   Initial schema, hardening, enrollment, and resource migrations
│   │   └── DependencyInjection.cs        SQLite persistence registration
│   └── TalebElm.Api/
│       ├── Controllers/    Auth, Health, Users, Tracks, Modules, Lessons, Exams, Progress
│       ├── Middlewares/    ExceptionHandlingMiddleware
│       └── Program.cs      Composition root
└── tests/TalebElm.Tests/
    ├── Infrastructure/    SQLite test fixture
    ├── UnitTests/         Entity, DTO, validator, service, repository, and registration tests
    └── IntegrationTests/  Controller and relational-constraint tests
```

## Implemented or Partly Implemented

- Domain entities and repository contracts exist for Users, Tracks, Modules, Lessons, LessonResources, Exams, TrackEnrollments, and UserProgress.
- EF Core uses SQLite for local development. All current configurations are present, including unique indexes, field lengths, check constraints, and `Restrict` foreign keys. The migration chain includes the initial schema, entity hardening, TrackEnrollment, and LessonResource.
- Application services: `TrackService` list/create, `ModuleService` ordered reads, `LessonService` ordered reads with resources, and `ExamService.GetByIdAsync` are implemented. `UserService`, `UserProgressService`, and exam create/submit are still placeholders.
- Validators exist for Track, Module, Lesson, and basic Exam input. They are registered through `AddApplication`, but controllers do not invoke the use cases yet.
- Controllers exist for Auth, Users, Tracks, Modules, Lessons, Exams, Progress, and Health. Most feature actions remain placeholders (throwing `TalebElm.Domain.Exceptions.NotImplementedException`, mapped to 501 by `ExceptionHandlingMiddleware`; Progress returns 501 directly). Only the health endpoint is fully wired.
- Registration is currently split: `AddApplication` registers the password hasher, validators, and Module/Lesson services; `Program.cs` registers `IUnitOfWork`, Track, Exam, User, and UserProgress services. Consolidating this graph is tracked by issue #274.
- SQLite-backed repository tests, service tests, validator tests, controller tests, migration-guard tests, and relational-constraint tests exist. The full register → enroll → study → exam → unlock journey is not yet covered because the workflow is not implemented.

## Planned Product Concepts Not Yet in the Model

- `ExamQuestion` and `ExamOption`: store questions and server-side answer keys; correct answers must not be returned to learners (issue #246).
- Authentication and authorization workflows: registration, login, JWT issuance, and the `StudentOnly`/`AdminOnly` policies (issues #248 and #278).
- Enrollment, exam submission, progression, and the HTTP surface for content reads/writes (issues #244, #250, #254, #255, #256).
- `UserExternalAccount`, `ModuleProblem`, `UserProblemCompletion`: Codeforces handle token verification, solve checking via public API (`user.status`), and dual-gated progression (Feature 07).
- `GitHubTask`, `TaskVersion`: CodeCrafters-style practical tasks with automated workflow and test SHA verification (Feature 10).
- `Technology`, `Resource`: 3-click anonymous public Resource Hub taxonomy, shared resource citations, and scheduled link checker (Feature 06).
- `CapstoneProject`, `ProjectMilestone`, `UserMilestoneProgress`: cumulative track-level projects with automated milestone verification via GitHub tasks (Feature 09).

## Architecture Boundaries

- Domain contains business entities, rules, exceptions, and repository interfaces; it must not reference EF Core or ASP.NET Core.
- Application owns DTOs, validators, `I<Name>Service` contracts **and their `<Name>Service` implementations**. Business logic lives exclusively here; Application depends only on Domain abstractions (normally `IUnitOfWork`) and never on EF Core or `HttpContext`.
- Infrastructure implements repository contracts and owns EF Core (`AppDbContext`, configurations, migrations) plus SQLite registration. It must not contain business logic. There is no `IApplicationDbContext`: `IUnitOfWork.SaveChangesAsync()` is the single commit point.
- API controllers call Application service interfaces and translate results to HTTP. Do not access repositories directly from controllers.
- Tests may reference the layers they verify; production projects must not reference Tests.

See [`FEATURE_INDEX.md`](FEATURE_INDEX.md) for the full list of documents, [`MVP_LEARNING_JOURNEY.md`](MVP_LEARNING_JOURNEY.md) for the workflow, and [`ARCHITECTURE.md`](ARCHITECTURE.md) for layer conventions.
