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
│   │   ├── Services/        IUserService, ITrackService, IExamService, IUserProgressService
│   │   ├── Interfaces/      IApplicationDbContext
│   │   └── Validators/      CreateTrackRequestValidator
│   ├── TalebElm.Infrastructure/
│   │   ├── Persistence/     AppDbContext and entity configurations
│   │   ├── Repositories/    User, Track, Module, Exam, UserProgress, UnitOfWork
│   │   └── Services/        User, Track, Exam, UserProgress services
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
- Track, Module, and Exam repositories have some implemented methods. Track read methods and UserRepository methods remain placeholders.
- Controllers exist for Auth, Users, Tracks, Modules, Lessons, Progress, and Health. Most feature actions remain placeholders; there is no ExamsController.
- SQLite-backed repository tests exist for selected implementations; they do not yet cover the whole student journey.

## Planned Product Concepts Not Yet in the Model

- `TrackEnrollment`: records that a learner selected a Track. Do not use `UserProgress` as an enrollment record.
- `LessonResource`: stores links/citations for books, official documentation, videos, and other lesson sources.
- `ExamQuestion` and `ExamOption`: store questions and server-side answer keys; correct answers must not be returned to learners.
- Center and Room: optional operational features with no approved domain contract yet.

## Architecture Boundaries

- Domain contains business entities, rules, exceptions, and repository interfaces; it must not reference EF Core or ASP.NET Core.
- Application owns DTOs, validators, and `I<Name>Service` contracts.
- Infrastructure implements services and repository contracts, and owns EF Core.
- API controllers call Application service interfaces and translate results to HTTP. Do not access repositories directly from controllers.
- Tests may reference the layers they verify; production projects must not reference Tests.

See [`FEATURE_INDEX.md`](FEATURE_INDEX.md) for the full list of documents, [`MVP_LEARNING_JOURNEY.md`](MVP_LEARNING_JOURNEY.md) for the workflow, and [`ARCHITECTURE.md`](ARCHITECTURE.md) for layer conventions.
