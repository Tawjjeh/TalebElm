# TalebElm Project Structure

This is the current source layout, not a promise that every MVP workflow is implemented. Planned entities and services are listed separately below.

## Current Solution

```text
TalebElm/
├── src/
│   ├── TalebElm.Domain/
│   │   ├── Entities/        BaseEntity, User, Track, Module, Lesson, Exam, UserProgress
│   │   ├── Enums/           TrackStatus, LessonType
│   │   ├── Exceptions/      DomainException, NotFoundException, ValidationException, NotImplementedException
│   │   └── Interfaces/      Repositories and IUnitOfWork
│   ├── TalebElm.Application/
│   │   ├── DTOs/            User, Track, Lesson, Exam, and Progress contracts
│   │   ├── Services/Abstraction/  IUserService, ITrackService, IExamService, IUserProgressService
│   │   ├── Services/Concrete/     User, Track, Exam, UserProgress service implementations (business logic lives here)
│   │   └── Validators/      CreateTrackRequestValidator
│   ├── TalebElm.Infrastructure/
│   │   ├── Persistence/DbContext/        AppDbContext
│   │   ├── Persistence/Configurations/   Entity configurations (field lengths, FKs, Restrict, unique indexes)
│   │   ├── Persistence/Repositories/     User, Track, Module, Exam, UserProgress, UnitOfWork implementations
│   │   └── DependencyInjection.cs        Persistence + repository DI wiring
│   └── TalebElm.Api/
│       ├── Controllers/    Auth, Health, Users, Tracks, Modules, Lessons, Progress
│       ├── Middlewares/    ExceptionHandlingMiddleware
│       └── Program.cs
└── tests/TalebElm.Tests/
    ├── Infrastructure/    SqliteTestBase
    ├── UnitTests/         Entity, DTO, service, repository, and registration tests
    └── IntegrationTests/  Controller and relational-constraint tests
```

## Implemented or Partly Implemented

- Domain entities and repository contracts exist for Users, Tracks, Modules, Lessons, Exams, and UserProgress.
- EF Core uses SQLite for local development; AppDbContext exposes the current entity sets.
- TrackService list/create and ExamService get-by-id have implementations.
- Track, Module, Exam, User, and UserProgress repositories implement their current contracts. Collection queries use `AsNoTracking`; single-entity lookups remain tracked to support read-modify-write flows.
- Controllers exist for Auth, Users, Tracks, Modules, Lessons, Exams, Progress, and Health. Most feature actions remain placeholders (throwing `TalebElm.Domain.Exceptions.NotImplementedException`, mapped to `501` by `ExceptionHandlingMiddleware`; Progress returns 501 directly). `Program.cs` registers infrastructure, `IUnitOfWork`, Application services, and the track validator.
- SQLite-backed repository tests exist for selected implementations; they do not yet cover the whole student journey.

## Planned Product Concepts Not Yet in the Model

- `TrackEnrollment`: records that a learner selected a Track. Do not use `UserProgress` as an enrollment record.
- `LessonResource`: stores links/citations for books, official documentation, videos, and other lesson sources.
- `ExamQuestion` and `ExamOption`: store questions and server-side answer keys; correct answers must not be returned to learners.
## Architecture Boundaries

- Domain contains business entities, rules, exceptions, and repository interfaces; it must not reference EF Core or ASP.NET Core.
- Application owns DTOs, validators, `I<Name>Service` contracts **and their `<Name>Service` implementations**. Business logic lives exclusively here; Application depends only on Domain abstractions (normally `IUnitOfWork`) and never on EF Core or `HttpContext`.
- Infrastructure implements repository contracts and owns EF Core (`AppDbContext`, configurations) plus DI wiring. It must not contain business logic. There is no `IApplicationDbContext`: `IUnitOfWork.SaveChangesAsync()` is the single commit point, so a DbContext abstraction in Application services is redundant.
- API controllers call Application service interfaces and translate results to HTTP. Do not access repositories directly from controllers.
- Tests may reference the layers they verify; production projects must not reference Tests.

See [`FEATURE_INDEX.md`](FEATURE_INDEX.md) for the full list of documents, [`MVP_LEARNING_JOURNEY.md`](MVP_LEARNING_JOURNEY.md) for the workflow, and [`ARCHITECTURE.md`](ARCHITECTURE.md) for layer conventions.
