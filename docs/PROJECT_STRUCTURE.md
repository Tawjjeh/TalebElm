# TalebElm Project Structure

This reflects the current source layout and separates implemented code from remaining MVP work.

## Current Solution

```text
TalebElm/
├── src/
│   ├── TalebElm.Domain/
│   │   ├── Entities/        BaseEntity, User, Track, Module, Lesson, LessonResource,
│   │   │                    Exam, ExamQuestion, ExamOption, TrackEnrollment, UserProgress,
│   │   │                    ModuleProblem, GitHubTask, UserExternalAccount,
│   │   │                    UserProblemCompletion, Technology, Resource,
│   │   │                    CapstoneProject, ProjectMilestone
│   │   ├── Enums/           TrackStatus, LessonType, LessonResourceKind, UserRole,
│   │   │                    ExternalPlatform, AccountVerificationStatus, ProblemKind,
│   │   │                    TechnologyKind, ResourceKind, ResourceLevel, ResourceStatus
│   │   ├── Exceptions/      DomainException, NotFoundException, ValidationException,
│   │   │                    ConflictException, ExternalServiceUnavailableException,
│   │   │                    NotImplementedException
│   │   └── Interfaces/      Repository contracts and IUnitOfWork
│   ├── TalebElm.Application/
│   │   ├── DTOs/            Auth, Track, Module, Lesson, Resource, Exam, Progress,
│   │   │                    Enrollment, and External client DTOs
│   │   ├── Services/Abstraction/  Service contracts, IPasswordHasher, ICodeforcesClient, IGitHubClient
│   │   ├── Services/Concrete/     Service implementations and PasswordHasher
│   │   ├── Validators/      Track, Module, Lesson, and Exam request validators
│   │   └── DependencyInjection.cs AddApplication() service and validator registration
│   ├── TalebElm.Infrastructure/
│   │   ├── Persistence/DbContext/        AppDbContext
│   │   ├── Persistence/Configurations/   Per-entity EF Core configurations
│   │   ├── Persistence/Repositories/     Repository implementations and UnitOfWork
│   │   ├── Migrations/                   EF Core SQLite migration chain
│   │   └── DependencyInjection.cs        AddInfrastructure() SQLite, UoW, and repository registration
│   └── TalebElm.Api/
│       ├── Controllers/    Auth, Health, Users, Tracks, Modules, Lessons, Exams, Progress
│       ├── Middlewares/    ExceptionHandlingMiddleware
│       └── Program.cs      Composition root (AddInfrastructure + AddApplication + OpenAPI in Dev)
└── tests/TalebElm.Tests/
    ├── Infrastructure/    SQLite test fixture
    ├── UnitTests/         Entity, DTO, validator, service, repository, and configuration tests
    └── IntegrationTests/  Controller, middleware, DI, migration-chain, and constraint tests
```

## Implemented Today

- Domain entities and enums exist for the core learning path (`User`, `Track`, `Module`, `Lesson`, `LessonResource`, `Exam`, `ExamQuestion`, `ExamOption`, `TrackEnrollment`, `UserProgress`), external verification (`ModuleProblem`, `GitHubTask`, `UserExternalAccount`, `UserProblemCompletion`), Resource Hub (`Technology`, `Resource`), and Capstone (`CapstoneProject`, `ProjectMilestone`).
- EF Core configurations and SQLite migrations exist for Users, Tracks, Modules, Lessons, LessonResources, Exams/Questions/Options, TrackEnrollments, UserProgress (including `CompletedRequiredProblems`), Technology/Resource, UserExternalAccount (filtered unique indexes), and `ModuleProblem.GitHubTaskId` constraints.
- DI composition is consolidated into `AddApplication()` and `AddInfrastructure(configuration)` (Decision 38).
- Application services implemented: `TrackService` (`GetPagedAsync`, `GetAllAsync`, `CreateAsync`), `ModuleService`, `LessonService`, `ExamService` (`GetByIdAsync`, server-side `SubmitAsync`), `TrackEnrollmentService`, `UserProgressService`, and `PasswordHasher`.
- `ExceptionHandlingMiddleware` maps domain and FluentValidation exceptions to ProblemDetails with camelCase `errors` and `traceId`, and logs unhandled `500` failures safely.
- `GET /api/health` and `GET /api/tracks` (paged) are wired; Swashbuckle OpenAPI/Swagger is enabled in `Development`.

## Remaining MVP Work (Tracked in Milestones M0 to M5)

- **M0 Foundations:** Remaining `CancellationToken` propagation (`#432`, `#435`), content-rule enforcement (`#251`, `#422`), and optional `X-Correlation-ID` header (`#325`).
- **M1 Identity:** Auth service, JWT configuration/validation, login throttling, `StudentOnly`/`AdminOnly` policies, Users endpoints, and auth integration tests.
- **M2 Learning flow:** Remaining controller wiring, next-module unlock on exam pass (`#208`), `ExamService.CreateAsync`, `LessonType` column (`#446`), and admin authoring endpoints.
- **M3 Automated Verification:** `TaskVersion` entity, remaining EF configurations/repositories (`#343`, `#345`, `#346`, `#415`), `CodeforcesClient`, `GitHubClient`, test fakes, verifiers, dual-gate unlock (`#444`), and endpoints.
- **M4 Public Resource Hub:** `ITechnologyRepository` / `IResourceRepository` (`#418`), Hub read/admin services and endpoints, safe link-health checker, and shared `Resource` migration.
- **M5 Capstone Projects:** EF mapping/migration (`#374`, `#388`), automated milestone verification, derived track completion evaluator, and learner Capstone endpoints.

## Architecture Boundaries

- `Domain` depends on nothing.
- `Application` depends only on `Domain` abstractions and `IUnitOfWork` (never `DbContext` or `HttpContext`).
- `Infrastructure` owns EF Core persistence and external HTTP clients with no business logic. `UnitOfWork.SaveChangesAsync()` is the single commit point.
- `Api` controllers call Application service interfaces and translate results to HTTP.
- `Tests` verify all layers offline with SQLite in-memory and fakes.

See [`FEATURE_INDEX.md`](FEATURE_INDEX.md) for per-feature status, [`MVP_LEARNING_JOURNEY.md`](MVP_LEARNING_JOURNEY.md) for the learner flow, and [`ARCHITECTURE.md`](ARCHITECTURE.md) for conventions.
