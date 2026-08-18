# TalebElm — Project Structure (Complete MVP)

> **Read this first:** this page shows the exact folder and file layout for the
> full **Minimum Viable Product (MVP)** — the 5 Clean Architecture projects and
> every file that will live in them, including the new `Exam` and `UserProgress`
> files that power the progression lock. The file names marked with ✨ are new
> tasks (51+) that are not yet in `AVAILABLE_TASKS.md` Tasks 1–50.

---

## 1. The full file tree

```
TalebElm/
│
├── TalebElm.slnx                        # solution file (all projects)
│
├── docs/                                # contributor documentation
│   ├── API_ENDPOINTS.md
│   ├── CLASS_FLOW.md
│   ├── DATABASE_ERD.md
│   ├── GITHUB_LABELS.md
│   └── PROJECT_STRUCTURE.md
│
├── src/
│   │
│   ├── TalebElm.Domain/                 # LAYER 1 — the rules (depends on nothing)
│   │   ├── Entities/                    # the "things" of the business
│   │   │   ├── BaseEntity.cs            # shared base (Id, timestamps) — required first ✨
│   │   │   ├── User.cs
│   │   │   ├── Track.cs
│   │   │   ├── Module.cs
│   │   │   ├── Lesson.cs
│   │   │   ├── Exam.cs                  # ✨ MVP block: Exam + UserProgress
│   │   │   └── UserProgress.cs          # ✨ the "progression lock" table
│   │   ├── Enums/                       # fixed choice lists
│   │   │   ├── LessonType.cs            # Text / Video / Exercise
│   │   │   ├── TrackStatus.cs           # Draft / Published / Archived
│   │   │   └── UserRole.cs              # Student / Instructor / Admin
│   │   ├── Exceptions/                  # business errors
│   │   │   ├── DomainException.cs
│   │   │   ├── NotFoundException.cs
│   │   │   └── ValidationException.cs
│   │   ├── Interfaces/                  # contracts for storage (no bodies)
│   │   │   ├── IRepository.cs           # generic IRepository<T>
│   │   │   ├── IUserRepository.cs
│   │   │   ├── ITrackRepository.cs
│   │   │   ├── IUnitOfWork.cs
│   │   │   ├── IExamRepository.cs       # ✨ new MVP interface
│   │   │   └── IUserProgressRepository.cs  # ✨ new MVP interface
│   │   └── TalebElm.Domain.csproj
│   │
│   ├── TalebElm.Application/            # LAYER 2 — the work (references Domain)
│   │   ├── DTOs/                        # data shapes (records)
│   │   │   ├── CreateUserRequest.cs
│   │   │   ├── CreateTrackRequest.cs
│   │   │   ├── UpdateTrackRequest.cs
│   │   │   ├── UserResponse.cs
│   │   │   ├── TrackResponse.cs
│   │   │   ├── LessonResponse.cs
│   │   │   ├── CreateExamRequest.cs     # ✨ new MVP DTOs
│   │   │   ├── ExamResponse.cs          # ✨
│   │   │   ├── SubmitExamRequest.cs     # ✨
│   │   │   ├── ExamResultResponse.cs    # ✨
│   │   │   └── ProgressResponse.cs      # ✨
│   │   ├── Interfaces/                  # the contract for the real database
│   │   │   └── IApplicationDbContext.cs
│   │   ├── Services/                    # "what the app can do" — interfaces
│   │   │   ├── IUserService.cs
│   │   │   ├── ITrackService.cs
│   │   │   ├── IExamService.cs          # ✨ new MVP service interfaces
│   │   │   └── IUserProgressService.cs  # ✨
│   │   ├── Validators/                  # input checks (FluentValidation)
│   │   │   └── CreateTrackRequestValidator.cs
│   │   └── TalebElm.Application.csproj
│   │
│   ├── TalebElm.Infrastructure/         # LAYER 3 — the "how" (references Application + Domain)
│   │   ├── Persistence/                 # EF Core: the database door + table configs
│   │   │   ├── AppDbContext.cs
│   │   │   ├── UserConfiguration.cs
│   │   │   ├── TrackConfiguration.cs
│   │   │   ├── ModuleConfiguration.cs
│   │   │   ├── LessonConfiguration.cs
│   │   │   ├── ExamConfiguration.cs     # ✨ new MVP configurations
│   │   │   └── UserProgressConfiguration.cs  # ✨
│   │   ├── Repositories/                # real implementations of Domain contracts
│   │   │   ├── UnitOfWork.cs
│   │   │   ├── UserRepository.cs
│   │   │   ├── TrackRepository.cs
│   │   │   ├── ExamRepository.cs        # ✨ new MVP repositories
│   │   │   └── UserProgressRepository.cs  # ✨
│   │   ├── Services/                    # real implementations of Application contracts
│   │   │   ├── UserService.cs
│   │   │   ├── TrackService.cs
│   │   │   ├── ExamService.cs           # ✨ new MVP service implementations
│   │   │   └── UserProgressService.cs   # ✨
│   │   └── TalebElm.Infrastructure.csproj
│   │
│   └── TalebElm.Api/                    # LAYER 4 — the door (references Infrastructure)
│       ├── Controllers/                 # one class per group of endpoints
│       │   ├── AuthController.cs        # /api/auth
│       │   ├── UsersController.cs       # /api/users
│       │   ├── TracksController.cs      # /api/tracks
│       │   ├── ModulesController.cs     # /api/modules
│       │   ├── LessonsController.cs     # /api/lessons
│       │   ├── ExamsController.cs       # ✨ /api/exams
│       │   ├── ProgressController.cs    # ✨ /api/progress
│       │   └── HealthController.cs      # /api/health
│       ├── Middlewares/
│       │   └── ExceptionHandlingMiddleware.cs
│       ├── Properties/
│       │   └── launchSettings.json
│       ├── appsettings.json
│       ├── appsettings.Development.json
│       ├── Program.cs                   # wires everything together
│       ├── TalebElm.Api.http
│       └── TalebElm.Api.csproj
│
└── tests/
    └── TalebElm.Tests/                  # LAYER 5 — the checker (references Api/Application/Domain)
        ├── UnitTests/                   # check one small piece of code
        │   ├── UserEntityTests.cs
        │   ├── TrackEntityTests.cs
        │   ├── ModuleEntityTests.cs
        │   ├── LessonEntityTests.cs
        │   ├── TrackStatusEnumTests.cs
        │   ├── UserDtoTests.cs
        │   ├── ExamEntityTests.cs       # ✨ new MVP tests
        │   ├── UserProgressEntityTests.cs  # ✨
        │   ├── ExamDtoTests.cs          # ✨
        │   └── ProgressDtoTests.cs      # ✨
        ├── IntegrationTests/            # check the Api through real HTTP
        │   ├── TracksControllerTests.cs
        │   └── UsersControllerTests.cs
        └── TalebElm.Tests.csproj
```

---

## 2. How files inside each project interact

Each project is internally organized so that related files sit together, and
the *projects themselves* only point inward (Api → Infrastructure →
Application → Domain). Inside one project, files talk to each other like this:

### Domain — *the rules*

- Every entity inherits `BaseEntity` (which takes care of `Id` and timestamps),
  so all entities share the same "shape". `Track`, `Module`, `Lesson`, `Exam`,
  and `UserProgress` are separate classes but all look alike at the base level.
- Entities **use the enums** as property types: `Track.Status` is a
  `TrackStatus`, `Lesson.Type` is a `LessonType`, `User.Role` is a `UserRole`.
- Repository **interfaces depend on entities**: `IRepository<T>` is generic
  (works with any entity), and the concrete interfaces (`ITrackRepository`,
  `IExamRepository`, ...) simply inherit it for one specific entity.
- `IUnitOfWork` is the "big contract": it lists the repository interfaces as
  properties, so anyone holding a `UnitOfWork` can reach every repository
  through one object. This is how Domain becomes a hub of *contracts*.
- `DomainException`, `NotFoundException`, `ValidationException` form a small
  chain: the two specific exceptions inherit from the base one. They are the
  language used to report business errors everywhere else.

### Application — *the work*

- **Service interfaces are built directly on DTOs**: `ITrackService` uses
  `CreateTrackRequest` as input and `TrackResponse` as output. The interfaces
  never show entities to the outside world — DTOs are the only data that
  crosses into Api.
- **DTOs** are grouped together in `DTOs/` and are just records; they carry
  data but hold no behavior.
- `IApplicationDbContext` sits in `Application/Interfaces/`: it is the contract
  for the database context. Application *demands* a "save" method and
  Infrastructure *delivers* it — everything else stays a local decision for
  Infrastructure to make.
- **Validators** live beside the DTOs they check: `CreateTrackRequestValidator`
  validates `CreateTrackRequest`, so "what data" and "is the data OK" live in
  the same layer.

### Infrastructure — *the how*

- **`AppDbContext` is the hub of the database side.** It inherits
  `DbContext` and is where all the entity configurations plug in. Each
  `XConfiguration` class knows how to shape one entity's table
  (`UserConfiguration` → `User`, `ExamConfiguration` → `Exam`, ...).
- **Repositories sit next to each other** in `Repositories/`. Each one
  implements a Domain interface (`TrackRepository` implements `ITrackRepository`)
  and does its work by *using* `AppDbContext` inside. `UnitOfWork` is the
  container that holds every repository together as one shared database
  session.
- **Service implementations sit in `Services/`** and mirror the Application
  interfaces one-to-one: `TrackService` implements `ITrackService`. Each
  service takes the pieces it needs (repositories, the db context) and
  coordinates them — deciding *how* a request gets fulfilled.

### Api — *the door*

- **Controllers group by feature/route prefix**: `TracksController` answers
  everything under `/api/tracks`, `ExamsController` under `/api/exams`, and so
  on. Each controller stays "thin": it only receives the HTTP call, asks a
  service interface for the work, and sends back the result as JSON.
- **Middlewares sit in `Middlewares/`** and are registered in `Program.cs`.
  They wrap the whole request pipeline (for example `ExceptionHandlingMiddleware`)
  — every request passes through them before reaching a controller.
- **`Program.cs` is the only file that wires the wiring**: it registers
  controllers, the service implementations, the database context, and the
  middleware. It is the single place where the *abstract* contracts in
  Application/Domain get matched to their *concrete* implementations in
  Infrastructure.

### Tests — *the checker*

- **`UnitTests/`** contains one class per major piece of Domain/Application
  code: `TrackEntityTests` for the `Track` entity, `ExamDtoTests` for the exam
  DTOs, and so on. Each class groups `[Fact]` methods that check one small
  behavior.
- **`IntegrationTests/`** goes one level higher: it holds `XControllerTests`
  classes that exercise controllers through real HTTP requests, making sure the
  whole pipeline (Api → Application → Infrastructure → Database) ends.
- Because Tests references Api, Application, and Domain, its files may reach
  inward into any of those projects — but none of those projects know Tests
  exist.