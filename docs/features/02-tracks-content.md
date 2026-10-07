# Feature Specification: Tracks & Content Management

## 1. MVP Scope & Objective

Let a learner choose a software-learning Track, then study its Modules and Lessons in order. Enrollment and lesson-source links are separate contracts; neither is currently implemented.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|
| Track | Id | `Guid` | No | PK, inherited from `BaseEntity` |
| Track | CreatedAt | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC |
| Track | Name | `string` | No | Initialized `default!`; no max length |
| Track | Description | `string` | No | Initialized `default!`; no max length |
| Track | Status | `TrackStatus` | No | Enum: `Draft=0`, `Published=1`, `Archived=2` |
| Module | Id | `Guid` | No | PK, inherited from `BaseEntity` |
| Module | CreatedAt | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC |
| Module | Title | `string` | No | Initialized `string.Empty` |
| Module | Summary | `string` | No | Initialized `string.Empty` |
| Module | Order | `int` | No | Position within Track; no uniqueness rule |
| Module | TrackId | `Guid` | No | FK → `Track.Id` |
| Lesson | Id | `Guid` | No | PK, inherited from `BaseEntity` |
| Lesson | CreatedAt | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC |
| Lesson | Title | `string` | No | Initialized `string.Empty` |
| Lesson | Content | `string` | No | Initialized `string.Empty` |
| Lesson | LessonType | `LessonType` | Planned | Existing enum should become an entity field for content rendering and validation |
| Lesson | Order | `int` | No | Position within Module; no uniqueness rule |
| Lesson | ModuleId | `Guid` | No | FK → `Module.Id` |

### Enums

| Enum | Values |
|---|---|
| `TrackStatus` | `Draft = 0`, `Published = 1`, `Archived = 2` |
| `LessonType` | `Text = 0`, `Video = 1`, `Exercise = 2` |

> `LessonType` enum exists in code but `Lesson` entity does not yet have a `LessonType` property. Treat the field above as the intended MVP contract and keep implementation notes explicit until the entity is updated.

Lessons may cite books, official documentation, videos, or other references. The current `Lesson.Content` is the only lesson-material field; source links are specified separately in [Lessons & Learning Resources](03-lessons-and-learning-resources.md).

### Relationships

- `Track` → `Module`: one-to-many via `Module.TrackId`.
- `Module` → `Lesson`: one-to-many via `Lesson.ModuleId`.
- No navigation properties declared on any entity.

### Repository Interfaces

```csharp
public interface ITrackRepository : IRepository<Track> { }
public interface IModuleRepository : IRepository<Module> { }
public interface ILessonRepository : IRepository<Lesson> { }
```

### EF Core Notes

- `TrackConfiguration` and `ModuleConfiguration` exist, but their `Configure` methods are empty.
- `LessonConfiguration` file does not exist in the current source tree.
- `AppDbContext` exposes the sets, but Track-to-Module and Module-to-Lesson relationships and delete rules are not configured.
- Ordering is applied by `ModuleRepository.GetByTrackIdAsync`; unique Order values are not enforced.

## 3. Application Contracts (Shared / Joint Ownership)

### DTOs

| DTO | Type | Fields |
|---|---|---|
| `CreateTrackRequest` | Input | `string Name`, `string Description` |
| `TrackResponse` | Output | `Guid Id`, `string Name`, `string Description`, `int Status` |
| `LessonResponse` | Output | `Guid Id`, `string Title`, `Guid ModuleId` |

> Module request/response DTOs do not exist yet. `UpdateTrackRequest` is planned but not present.

### Service Interface

`ITrackService` exists in `Application/Services/`:

```csharp
public interface ITrackService
{
    Task<IReadOnlyList<TrackResponse>> GetAllAsync();
    Task<TrackResponse> CreateAsync(CreateTrackRequest request);
}
```

**Implementation status:** `TrackService.GetAllAsync` and `CreateAsync` are implemented. `TrackRepository.AddAsync` and `ModuleRepository` methods are implemented, but TrackRepository reads are placeholders. No `IModuleService` or `ILessonService` exists.

### Update and delete behavior (planned)

- Reject deleting a Track while it still has Modules. Return `409 Conflict` until an explicit archive/cascade-delete policy is approved.
- Reject deleting a Track that has learner enrollment/progress data. Preserve learner history instead of cascading deletes.
- Reject deleting a Module while it still has Lessons, an assigned Exam, or learner progress rows. Return `409 Conflict`.
- Updating a Module must preserve unique ordering within its Track; changing `Order` may require shifting sibling Modules.
- Deleting a Lesson is allowed only after confirming no downstream content linkage depends on it; re-sequence remaining Lessons in the Module after deletion.
- Updating a Lesson can change title/content/type/order, but it must remain scoped to one Module unless a separate move operation is designed.

### Validators

- `CreateTrackRequestValidator` validates that `Name` and `Description` are not empty.

## 4. API Endpoints Contract (Owned by API Team)

### Endpoints Table

| HTTP Method | Route | Request DTO | Response DTO | Status Codes |
|---|---|---|---|---|
| `GET` | `/api/tracks` | — | `TrackResponse[]` (planned) | Throws `NotImplementedException` |
| `POST` | `/api/tracks` | `CreateTrackRequest` (planned) | `TrackResponse` (planned) | Throws `NotImplementedException` |
| `GET` | `/api/tracks/{id}` | — | `TrackResponse` (planned) | Planned: `200`, `404` |
| `PUT` | `/api/tracks/{id}` | `UpdateTrackRequest` (planned) | `TrackResponse` (planned) | Planned: `200`, `400`, `404` |
| `DELETE` | `/api/tracks/{id}` | — | — | Planned: `204`, `404` |
| `GET` | `/api/tracks/{id}/modules` | — | `ModuleResponse[]` (planned) | Planned: `200`, `404` |
| `POST` | `/api/modules` | (planned) | (planned) | Planned: `201`, `400` |
| `GET` | `/api/modules/{id}` | — | (planned) | Planned: `200`, `404` |
| `GET` | `/api/modules` | — | — | Throws `NotImplementedException` |
| `PUT` | `/api/modules/{id}` | `UpdateModuleRequest` (planned) | `ModuleResponse` (planned) | Planned: `200`, `400`, `404`, `409` |
| `DELETE` | `/api/modules/{id}` | — | — | Planned: `204`, `404`, `409` |
| `GET` | `/api/modules/{id}/lessons` | — | `LessonResponse[]` (planned) | Planned: `200`, `404` |
| `POST` | `/api/lessons` | (planned) | (planned) | Planned: `201`, `400` |
| `GET` | `/api/lessons` | — | — | Throws `NotImplementedException` |
| `GET` | `/api/lessons/{id}` | — | `LessonResponse` (planned) | Planned: `200`, `404` |
| `PUT` | `/api/lessons/{id}` | `UpdateLessonRequest` (planned) | `LessonResponse` (planned) | Planned: `200`, `400`, `404`, `409` |
| `DELETE` | `/api/lessons/{id}` | — | — | Planned: `204`, `404`, `409` |

### Endpoint behavior notes

- `DELETE /api/tracks/{id}` should fail with `409 Conflict` when the Track still has Modules or any learner enrollment/progress references.
- `DELETE /api/modules/{id}` should fail with `409 Conflict` when Lessons, Exams, or learner progress depend on that Module.
- `PUT` and `DELETE` routes for Modules and Lessons are planned contracts only; no controller actions or DTOs exist in the current code.
- Learner-facing browse endpoints should return `404` for missing Track/Module/Lesson ids and `400` for invalid request payloads.

### Input Validation

- TrackService validates CreateTrackRequest with FluentValidation; Name and Description must not be empty.
- No Module or Lesson request validation is implemented.
- The learner-facing Track browse/detail and nested Module/Lesson routes in the table are planned; current controller actions are parameterless placeholders.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| `NotFoundException` | Track / Module / Lesson lookup by ID returns `null` |
| `ValidationException` | Missing required fields, duplicate `Order` values (planned) |
| `NotImplementedException` | All current controller actions and repository methods (placeholder) |
