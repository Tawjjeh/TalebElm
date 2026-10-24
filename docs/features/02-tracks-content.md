# Feature Specification: Tracks & Content Management

## 1. MVP Scope & Objective

Let a learner choose a software-learning Track, then study its Modules and Lessons in order. Enrollment and lesson-source links have persistence contracts now, but their workflows and HTTP endpoints are not implemented.

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
| Lesson | LessonType | `LessonType` | Planned | Existing enum should become an entity field for content rendering and validation |
| Lesson | Order | `int` | No | Position within Module (`>= 1`); `UNIQUE(ModuleId, Order)` — no two Lessons share an `Order` in one Module |
| Lesson | ModuleId | `Guid` | No | FK → `Module.Id` with `DeleteBehavior.Restrict` |

### Enums

| Enum | Values |
|---|---|
| `TrackStatus` | `Draft = 0`, `Published = 1`, `Archived = 2` |
| `LessonType` | `Text = 0`, `Video = 1`, `Exercise = 2` |

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
- Ordering is applied by `ModuleRepository.GetByTrackIdAsync`; duplicate `Order` values are rejected by both Application validators and the unique indexes.

## 3. Application Contracts (Shared / Joint Ownership)

### DTOs

| DTO | Type | Fields |
|---|---|---|
| `CreateTrackRequest` | Input | `string Name`, `string Description` |
| `TrackResponse` | Output | `Guid Id`, `string Name`, `string Description`, `int Status` |
| `CreateModuleRequest` | Input | `string Title`, `string Summary`, `int Order`, `Guid TrackId` |
| `UpdateModuleRequest` | Input | `string Title`, `string? Summary`, `int Order`, `Guid TrackId` |
| `ModuleResponse` | Output | `Guid Id`, `string Title`, `string Summary`, `int Order`, `Guid TrackId` |
| `CreateLessonRequest` | Input | `string Title`, `Guid ModuleId`, `int Order` |
| `UpdateLessonRequest` | Input | `string Title`, `Guid ModuleId`, `int Order` |
| `LessonResponse` | Output | `Guid Id`, `string Title`, `Guid ModuleId` |
| `LessonDetailResponse` | Output | Lesson fields plus ordered `LessonResourceResponse` items |

> `UpdateTrackRequest` is still planned. `CreateLessonRequest`/`UpdateLessonRequest` do not yet carry `Content`; that gap is tracked with the Lesson authoring work (#284).

### Service Interfaces

`ITrackService`, `IModuleService`, and `ILessonService` live in `Application/Services/Abstraction/`:

```csharp
public interface ITrackService
{
    Task<IReadOnlyList<TrackResponse>> GetAllAsync();
    Task<TrackResponse> CreateAsync(CreateTrackRequest request);
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

### Update and delete behavior (planned)

- Reject deleting a Track while it still has Modules. Return `409 Conflict` until an explicit archive/cascade-delete policy is approved.
- Reject deleting a Track that has learner enrollment/progress data. Preserve learner history instead of cascading deletes.
- Reject deleting a Module while it still has Lessons, an assigned Exam, or learner progress rows. Return `409 Conflict`.
- Updating a Module must preserve unique ordering within its Track; changing `Order` may require shifting sibling Modules.
- Deleting a Module must re-sequence the remaining sibling Modules so their
  `Order` values stay contiguous (`1, 2, 3, ...`) and free of gaps, otherwise
  learner progress rows keyed to old orders become confusing.
- Deleting a Lesson is allowed only after confirming no downstream content linkage depends on it; re-sequence remaining Lessons in the Module after deletion.
- Updating a Lesson can change title/content/type/order, but it must remain scoped to one Module unless a separate move operation is designed.
- When a Module or Lesson `Order` changes, shift siblings within the same parent
  in the same `UnitOfWork` transaction so no two siblings share an `Order`.

### Validators

- `CreateTrackRequestValidator` validates that `Name` and `Description` are not empty. Maximum-length rules are database-enforced for Track input today.
- `CreateModuleRequestValidator` / `UpdateModuleRequestValidator` enforce title, summary, order, and track id rules.
- `CreateLessonRequestValidator` / `UpdateLessonRequestValidator` enforce title, module id, and order rules. They do not yet validate `Content` because the DTO does not carry it.
- All validators are registered by `AddApplication()`.

## 4. API Endpoints Contract (Owned by API Team)

### Endpoints Table

| HTTP Method | Route | Request DTO | Response DTO | Status Codes |
|---|---|---|---|---|
| `GET` | `/api/tracks` | — | `TrackResponse[]` (planned) | Service exists; controller throws domain `NotImplementedException` |
| `POST` | `/api/tracks` | `CreateTrackRequest` (planned) | `TrackResponse` (planned) | Service exists; controller action has no body and throws |
| `GET` | `/api/tracks/{id}` | — | `TrackResponse` (planned) | Planned: `200`, `404` |
| `PUT` | `/api/tracks/{id}` | `UpdateTrackRequest` (planned) | `TrackResponse` (planned) | Planned: `200`, `400`, `404` |
| `DELETE` | `/api/tracks/{id}` | — | — | Planned: `204`, `404` |
| `GET` | `/api/tracks/{id}/modules` | — | `ModuleResponse[]` (planned) | Planned: `200`, `404` |
| `POST` | `/api/modules` | (planned) | (planned) | Planned: `201`, `400` |
| `GET` | `/api/modules/{id}` | — | (planned) | Planned: `200`, `404` |
| `GET` | `/api/modules` | — | — | Service exists; controller is a parameterless placeholder |
| `PUT` | `/api/modules/{id}` | `UpdateModuleRequest` (planned) | `ModuleResponse` (planned) | Planned: `200`, `400`, `404`, `409` |
| `DELETE` | `/api/modules/{id}` | — | — | Planned: `204`, `404`, `409` |
| `GET` | `/api/modules/{id}/lessons` | — | `LessonResponse[]` (planned) | Planned: `200`, `404` |
| `POST` | `/api/lessons` | (planned) | (planned) | Planned: `201`, `400` |
| `GET` | `/api/lessons` | — | — | Service exists; controller is a parameterless placeholder |
| `GET` | `/api/lessons/{id}` | — | `LessonResponse` (planned) | Planned: `200`, `404` |
| `PUT` | `/api/lessons/{id}` | `UpdateLessonRequest` (planned) | `LessonResponse` (planned) | Planned: `200`, `400`, `404`, `409` |
| `DELETE` | `/api/lessons/{id}` | — | — | Planned: `204`, `404`, `409` |

### Endpoint behavior notes

- `DELETE /api/tracks/{id}` should fail with `409 Conflict` when the Track still has Modules or any learner enrollment/progress references.
- `DELETE /api/modules/{id}` should fail with `409 Conflict` when Lessons, Exams, or learner progress depend on that Module.
- `PUT` and `DELETE` routes for Modules and Lessons are planned contracts only. Update DTOs and validators exist, but no controller actions are implemented.
- Learner-facing browse endpoints should return `404` for missing Track/Module/Lesson ids and `400` for invalid request payloads.

### Input Validation

- TrackService validates `CreateTrackRequest` with FluentValidation; Name and Description must not be empty.
- Module and Lesson create/update validators exist and are registered, but no controller invokes them yet.
- The learner-facing Track browse/detail and nested Module/Lesson routes in the table are planned; current controller actions are parameterless placeholders.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| `NotFoundException` | Track / Module / Lesson lookup by ID returns `null` |
| `ValidationException` | Missing required fields, duplicate `Order` values (planned) |
| `NotImplementedException` | Remaining placeholder controller actions and unimplemented use cases |

## Related external resources
Practice problems are configured per Module per Feature 07; external task verification is specified per Decision 35.
