# Feature Specification: Tracks & Content Management

## 1. MVP Scope & Objective

Let a learner choose a software-learning Track, then study its Modules and Lessons in order. Track listing is live at `GET /api/tracks`. Enrollment and lesson reads exist as Application services, but their HTTP endpoints are still target.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|
| Track | Id | `Guid` | No | PK, inherited from `BaseEntity` |
| Track | CreatedAt | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC |
| Track | Name | `string` | No | Initialized `string.Empty`; required, max length **150**, non-blank check |
| Track | Description | `string` | No | Initialized `string.Empty`; required, max length **1000**, non-blank check |
| Track | Status | `TrackStatus` | No | Enum: `Draft=0`, `Published=1`, `Archived=2`; new tracks must default to `Draft` |
| Module | Id | `Guid` | No | PK, inherited from `BaseEntity` |
| Module | CreatedAt | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC |
| Module | Title | `string` | No | Initialized `string.Empty`; required, max length **150**, non-blank check |
| Module | Summary | `string` | No | Initialized `string.Empty`; required, max length **500** |
| Module | Order | `int` | No | Position within Track (`>= 1`); `UNIQUE(TrackId, Order)` — no two Modules share an `Order` in one Track |
| Module | TrackId | `Guid` | No | FK → `Track.Id` with `DeleteBehavior.Restrict` |
| Lesson | Id | `Guid` | No | PK, inherited from `BaseEntity` |
| Lesson | CreatedAt | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC |
| Lesson | Title | `string` | No | Initialized `string.Empty`; required, max length **150**, non-blank check |
| Lesson | Content | `string` | No | Initialized `string.Empty`; required, free text (no max length) |
| Lesson | LessonType | `LessonType` | No | Enum: `Text=1`, `Video=2`, `Exercise=3`; new lessons must default to `Text` |
| Lesson | Order | `int` | No | Position within Module (`>= 1`); `UNIQUE(ModuleId, Order)` — no two Lessons share an `Order` in one Module |
| Lesson | ModuleId | `Guid` | No | FK → `Module.Id` with `DeleteBehavior.Restrict` |

### Enums

| Enum | Values |
|---|---|
| `TrackStatus` | `Draft = 0`, `Published = 1`, `Archived = 2` |
| `LessonType` | `Text = 1`, `Video = 2`, `Exercise = 3` |

> `LessonType` enum exists in code but `Lesson` entity does not yet have a `LessonType` property. Treat the field above as the intended MVP contract and keep implementation notes explicit until the entity is updated.

Lessons may cite books, official documentation, videos, or other references. The current `Lesson.Content` is the only lesson-material field; source links are specified separately in [Lessons & Learning Resources](03-lessons-and-learning-resources.md).

### Relationships

- `Track` → `Module`: one-to-many via `Module.TrackId` with `DeleteBehavior.Restrict` and navigation pair `Track.Modules` / `Module.Track`.
- `Module` → `Lesson`: one-to-many via `Lesson.ModuleId` with `DeleteBehavior.Restrict` and navigation pair `Module.Lessons` / `Lesson.Module`.
- `Module` → `Exam`: one-to-zero-or-one via `Exam.ModuleId` with `DeleteBehavior.Restrict` and `UNIQUE(ModuleId)`.

### Repository Interfaces

```csharp
public interface ITrackRepository : IRepository<Track> { }
public interface IModuleRepository : IRepository<Module> { }
public interface ILessonRepository : IRepository<Lesson> { }
```

### EF Core Notes

- `TrackConfiguration`, `ModuleConfiguration`, and `LessonConfiguration` map the relationships above through navigation properties with `DeleteBehavior.Restrict`.
- Enforce `UNIQUE(TrackId, Order)` (Modules), `UNIQUE(ModuleId, Order)` (Lessons), and `UNIQUE(ModuleId)` (Exams) as unique indexes.
- `AppDbContext` lives at `Infrastructure/Persistence/DbContext/AppDbContext.cs`; configurations at `Infrastructure/Persistence/Configurations/`; repository implementations at `Infrastructure/Persistence/Repositories/`.
- Ordering is applied by `ModuleRepository.GetByTrackIdAsync` for reads; ordering maintenance loads tracked siblings via `GetByTrackIdForUpdateAsync` / `GetByModuleIdForUpdateAsync` and shifts them with `SiblingOrdering`. Duplicate `Order` values are rejected by both Application validators and the unique indexes.

## 3. Application Contracts (Shared / Joint Ownership)

### DTOs

| DTO | Type | Fields |
|---|---|---|
| `CreateTrackRequest` | Input | `string Name`, `string Description` |
| `TrackResponse` | Output | `Guid Id`, `string Name`, `string Description`, `int Status` |
| `PagedTracksResponse` | Output | `IReadOnlyList<TrackResponse> Items`, `int Page`, `int PageSize`, `int TotalCount` |
| `CreateModuleRequest` | Input | `string Title`, `string Summary`, `int Order`, `Guid TrackId` |
| `UpdateModuleRequest` | Input | `string Title`, `string? Summary`, `int Order`, `Guid TrackId` |
| `ModuleResponse` | Output | `Guid Id`, `string Title`, `string Summary`, `int Order`, `Guid TrackId` |
| `CreateLessonRequest` | Input | `string Title`, `Guid ModuleId`, `int Order`, `string Content`, `LessonType Type` |
| `UpdateLessonRequest` | Input | `string Title`, `Guid ModuleId`, `int Order`, `string Content`, `LessonType Type` |
| `LessonResponse` | Output | `Guid Id`, `string Title`, `Guid ModuleId` |
| `LessonDetailResponse` | Output | Lesson fields plus ordered `LessonResourceResponse` items |

> `UpdateTrackRequest` is still planned. Lesson authoring (write methods on `ILessonService`) is still Target; the remaining work is tracked in #284.

### Service Interfaces

`ITrackService`, `IModuleService`, and `ILessonService` live in `Application/Services/Abstraction/`:

```csharp
public interface ITrackService
{
    Task<IReadOnlyList<TrackResponse>> GetAllAsync();
    Task<TrackResponse> CreateAsync(CreateTrackRequest request);
    Task<PagedTracksResponse> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default);
}

public interface IModuleService
{
    Task<IReadOnlyList<ModuleResponse>> GetByTrackAsync(Guid trackId);
    Task<ModuleResponse> GetByIdAsync(Guid id);
}

public interface ILessonService
{
    Task<IReadOnlyList<LessonResponse>> GetByModuleAsync(Guid moduleId);
    Task<LessonDetailResponse> GetByIdAsync(Guid id);
}
```

**Implementation status:** `TrackService.GetAllAsync` and `CreateAsync`, `ModuleService.GetByTrackAsync`/`GetByIdAsync`, and `LessonService.GetByModuleAsync`/`GetByIdAsync` are implemented. `TrackRepository` reads, `ModuleRepository` queries, and `LessonRepository` queries are implemented. The services are not yet wired to controllers, and the published/unlocked filters belong to the API/progression work.

### Field length and default status rules

- The max lengths in the entity table are enforced by `TrackConfiguration`,
  `ModuleConfiguration`, and `LessonConfiguration`. The Module/Lesson validators
  also check them; invoking those validators from HTTP is still target work.
- `TrackService.CreateAsync` creates every new Track with
  `Status = TrackStatus.Draft`; keep this default so a Track is never published
  accidentally. Publishing is a separate explicit operation.

### Update and delete behavior

- **Track edit policy (Draft-only):** `PUT /api/tracks/{id}` edits a track's name
  and description only while its `Status` is `Draft`; editing a `Published` or
  `Archived` track returns `400 ValidationException`. A track that left Draft is
  live for learners, so its name and description are frozen. Status changes go
  through the separate `PATCH /api/tracks/{id}/status` workflow (#266).
- Deleting a Track is rejected with `409 Conflict` while it still has Modules,
  learner enrollments, or learner progress rows. The guards run before any
  modification, and the database's `Restrict` FKs are only the backstop.
- Deleting a Module is rejected with `409 Conflict` while it still has Lessons,
  an assigned Exam, or learner progress rows (same guard-first rule).
- Creating a Module at `Order` k inserts it there and shifts siblings with
  `Order >= k` up by one (`SiblingOrdering.PrepareInsert`); the `Order` in the
  route's track is authoritative and the module's track must exist (`404`).
- Updating a Module changes title, summary, and `Order` within its track; an
  `Order` change moves it via `SiblingOrdering.MoveAsync` (two saves in one
  transaction). Moving a Module to another track is not supported (`400`).
- Deleting a Module re-sequences the remaining sibling Modules so their
  `Order` values stay contiguous (`1, 2, 3, ...`) and free of gaps, otherwise
  learner progress rows keyed to old orders become confusing. `SiblingOrdering.PrepareDelete` does the compaction after the #251 conflict guards.
- Deleting a Lesson is allowed only after confirming no downstream content linkage depends on it; re-sequence remaining Lessons in the Module after deletion (`SiblingOrdering.PrepareDelete`).
- Updating a Lesson can change title/content/type/order, but it must remain scoped to one Module unless a separate move operation is designed.
- When a Module or Lesson `Order` changes, shift siblings within the same parent
  in the same `UnitOfWork` transaction so no two siblings share an `Order`.
  `SiblingOrdering` implements this: insert/delete shifts run in one
  `SaveChangesAsync` (EF Core orders the updates so no transient duplicate
  `Order` exists), while a move runs as two saves inside one
  `IUnitOfWork.ExecuteInTransactionAsync` transaction — a single save cannot
  work because the mover and the intermediate range form a dependency cycle
  that EF Core refuses with a circular-dependency error. Callers load the
  siblings tracked via `IModuleRepository.GetByTrackIdForUpdateAsync` /
  `ILessonRepository.GetByModuleIdForUpdateAsync`; the read
  `GetByTrackIdAsync` / `GetByModuleIdAsync` return detached copies whose
  mutations would never be saved.

### Validators

- `CreateTrackRequestValidator` validates that `Name` and `Description` are not empty. Maximum-length rules are database-enforced for Track input today.
- `CreateModuleRequestValidator` / `UpdateModuleRequestValidator` enforce title, summary, order, and track id rules.
- `CreateLessonRequestValidator` / `UpdateLessonRequestValidator` enforce title, module id, order, content, and type rules. Invoking these validators from HTTP controllers is still target work (#284).
- All validators are registered by `AddApplication()`.

## 4. API Endpoints Contract (Owned by API Team)

### Endpoints Table

| HTTP Method | Route | Request DTO | Response DTO | Status Codes |
|---|---|---|---|---|
| `GET` | `/api/tracks` | Query params: `page` (default 1), `pageSize` (default 20, max 100) | `PagedTracksResponse` | `200`, `400` |
| `POST` | `/api/tracks` | `CreateTrackRequest` | `TrackResponse` | `201`, `400`, `401`, `403` (`AdminOnly`) |
| `GET` | `/api/tracks/{id}` | — | `TrackResponse` (planned) | Planned: `200`, `404` |
| `PUT` | `/api/tracks/{id}` | `UpdateTrackRequest` | `TrackResponse` | `200`, `400` (also when the track is not Draft), `401`, `403` (`AdminOnly`), `404` |
| `DELETE` | `/api/tracks/{id}` | — | — | `204`, `401`, `403` (`AdminOnly`), `404`, `409` (Modules, enrollments, or progress exist) |
| `GET` | `/api/tracks/{id}/modules` | — | `ModuleResponse[]` (planned) | Planned: `200`, `404` |
| `POST` | `/api/tracks/{trackId}/modules` | `CreateModuleRequest` (route track id authoritative) | `ModuleResponse` | `201`, `400`, `401`, `403` (`AdminOnly`), `404` (track missing) |
| `GET` | `/api/modules/{id}` | — | (planned) | Planned: `200`, `404` |
| `GET` | `/api/modules` | — | — | Service exists; controller is a parameterless placeholder |
| `PUT` | `/api/modules/{id}` | `UpdateModuleRequest` | `ModuleResponse` | `200`, `400` (also on cross-track move), `401`, `403` (`AdminOnly`), `404` |
| `DELETE` | `/api/modules/{id}` | — | — | `204`, `401`, `403` (`AdminOnly`), `404`, `409` (Lessons, Exam, or progress exist) |
| `GET` | `/api/modules/{id}/lessons` | — | `LessonResponse[]` (planned) | Planned: `200`, `404` |
| `POST` | `/api/lessons` | (planned) | (planned) | Planned: `201`, `400` |
| `GET` | `/api/lessons` | — | — | Service exists; controller is a parameterless placeholder |
| `GET` | `/api/lessons/{id}` | — | `LessonResponse` (planned) | Planned: `200`, `404` |
| `PUT` | `/api/lessons/{id}` | `UpdateLessonRequest` (planned) | `LessonResponse` (planned) | Planned: `200`, `400`, `404`, `409` |
| `DELETE` | `/api/lessons/{id}` | — | — | Planned: `204`, `404`, `409` |

### Endpoint behavior notes

- `DELETE /api/tracks/{id}` fails with `409 Conflict` when the Track still has Modules, enrollments, or learner progress references.
- `DELETE /api/modules/{id}` fails with `409 Conflict` when Lessons, an Exam, or learner progress depend on that Module.
- `PUT` and `DELETE` routes for Lessons are planned contracts only. Update DTOs and validators exist, but no controller actions are implemented (#284).
- Learner-facing browse endpoints should return `404` for missing Track/Module/Lesson ids and `400` for invalid request payloads.

### Input Validation

- TrackService validates `CreateTrackRequest` and `UpdateTrackRequest` with FluentValidation; Name and Description must not be empty.
- Module create/update validators are invoked by `ModuleService` on every write.
- The learner-facing Track browse/detail and nested Module/Lesson routes in the table are planned; current controller actions are parameterless placeholders.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| `NotFoundException` | Track / Module / Lesson lookup by ID returns `null` |
| `ValidationException` | Missing required fields, duplicate `Order` values (planned) |
| `NotImplementedException` | Remaining placeholder controller actions and unimplemented use cases |

## Related external resources
Practice problems are configured per Module per Feature 07; external task verification is specified per Decision 35.
