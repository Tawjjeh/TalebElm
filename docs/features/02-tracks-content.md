# Feature Specification: Tracks & Content Management

## 1. MVP Scope & Objective

Represent learning paths as Tracks containing ordered Modules and Lessons. This is the core content structure of the platform — all other features (exams, enrollment, progress) depend on tracks and modules existing first.

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
| Lesson | Order | `int` | No | Position within Module; no uniqueness rule |
| Lesson | ModuleId | `Guid` | No | FK → `Module.Id` |

### Enums

| Enum | Values |
|---|---|
| `TrackStatus` | `Draft = 0`, `Published = 1`, `Archived = 2` |
| `LessonType` | `Text = 0`, `Video = 1`, `Exercise = 2` |

> `LessonType` enum exists in code but `Lesson` entity does not yet have a `LessonType` property.

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

- `TrackConfiguration`, `ModuleConfiguration` exist with empty `Configure` bodies.
- `LessonConfiguration` file does not exist in the current source tree.
- No ordering, FK behavior, or field-length constraints configured.

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

**Implementation status:** `TrackService` implements `GetAllAsync` and `CreateAsync`. `TrackRepository.AddAsync` is implemented, but its inherited `GetByIdAsync` and `GetAllAsync` methods still throw `NotImplementedException`. No `IModuleService` or `ILessonService` exists in the current source tree.

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
| `GET` | `/api/modules/{id}/lessons` | — | `LessonResponse[]` (planned) | Planned: `200`, `404` |
| `POST` | `/api/lessons` | (planned) | (planned) | Planned: `201`, `400` |
| `GET` | `/api/lessons` | — | — | Throws `NotImplementedException` |
| `GET` | `/api/lessons/{id}` | — | `LessonResponse` (planned) | Planned: `200`, `404` |

### Input Validation

- No request validation is implemented for Track, Module, or Lesson actions.
- `CreateTrackRequestValidator` is commented out; when active it should enforce non-empty `Name` and `Description`.
- No module-by-track or lesson-by-module filtering routes exist in the current controllers.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| `NotFoundException` | Track / Module / Lesson lookup by ID returns `null` |
| `ValidationException` | Missing required fields, duplicate `Order` values (planned) |
| `NotImplementedException` | All current controller actions and repository methods (placeholder) |
