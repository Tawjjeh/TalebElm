# Feature Specification: Lessons & Learning Resources

## Status
`Lesson` exists with `Title`, `Content`, `Order`, and `ModuleId`; its EF relationship to `Module` is configured with `Restrict`. `LessonResource`, its configuration, repository, and `AddLessonResources` migration exist, and `LessonService` returns ordered lessons and lesson details with ordered resources. `LessonsController` is still a placeholder, and resource authoring/validation is not implemented.

## MVP Objective
Let a learner study an authored explanation for each Lesson and open the books, official documentation, videos, or other sources selected by the content author. Keep lesson content and external resource links as separate data so a lesson can cite more than one source.

## Current Domain Contract

| Entity | Field | Type | Nullable | Constraints / Notes |
|---|---|---|---|---|
| Lesson | Id | Guid | No | Inherited primary key |
| Lesson | CreatedAt | DateTimeOffset | No | Inherited creation timestamp |
| Lesson | Title | string | No | Required, max length **150**, non-blank check |
| Lesson | Content | string | No | Required lesson explanation |
| Lesson | Order | int | No | Display order within a Module |
| Lesson | ModuleId | Guid | No | Parent Module identifier |

### Relationship
- `Lesson.ModuleId` is a required FK to `Module.Id` with `DeleteBehavior.Restrict`; `Lesson.Module` is the navigation.
- A Lesson can have zero or more `LessonResource` rows. The relationship is uni-directional (`LessonResource.Lesson`, no `Lesson.Resources` collection).

## Current Resource Contract

`LessonResource` exists as a separate entity rather than packing external links into `Lesson.Content`:

| Entity | Field | Type | Nullable | Constraints / Notes |
|---|---|---|---|---|
| LessonResource | Id | Guid | No | Inherited primary key |
| LessonResource | LessonId | Guid | No | Required FK to Lesson |
| LessonResource | Title | string | No | Short label shown to the learner, such as “Official C# docs” |
| LessonResource | Kind | LessonResourceKind | No | Book, Documentation, Video, Article, Repository, or Other |
| LessonResource | Url | string | Yes | External URL; null only when the resource is a bibliographic citation without a URL |
| LessonResource | Citation | string | Yes | Author/title/edition/page for a book or other non-URL reference |
| LessonResource | Order | int | No | Display order within the Lesson |

Keep `LessonType` separate from `LessonResourceKind`: the former describes lesson format if it is added to Lesson; the latter describes a cited source. Do not claim `LessonType` is currently stored on Lesson.

### Application Contract (Implemented)

- `LessonResourceResponse` carries `Id`, `Title`, `Kind`, `Url`, `Citation`, and `Order`.
- `LessonDetailResponse` carries `Id`, `ModuleId`, `Title`, `Content`, `Order`, and an ordered resource list.
- `ILessonService.GetByModuleAsync` returns ordered `LessonResponse` items; `GetByIdAsync` returns the detail with resources. `CreateLessonRequest` and `UpdateLessonRequest` exist but do not yet carry `Content` (#284).

### Suggested API Contract (Not Implemented)

| Method | Route | Purpose | Status |
|---|---|---|---|
| GET | `/api/modules/{moduleId}/lessons` | List lessons in `Order` | Service exists; no matching controller action |
| GET | `/api/lessons/{lessonId}` | Return explanation and resources | Service exists; current `LessonsController.Get()` has no id parameter and throws `NotImplementedException` |

### Resource validation rules (proposed)

The database currently enforces required `Title` (max 200), optional `Citation` (max 500), and `Order > 0`. The remaining rules are target behavior:

- `Title` is required and has a max length of **200**.
- At least one of `Url` or `Citation` must be provided; both may be provided.
- When `Url` is present it must be a valid **absolute** URL using the `http` or
  `https` scheme. Reject relative paths, empty strings, and unsupported schemes
  with `ValidationException` (HTTP `400`).
- `Citation` is free text with a max length of **500** at the database boundary.
- `Order` must be a positive integer; ordering is re-sequenced within the Lesson
  on create/update/delete, mirroring the Module/Lesson ordering rule.

## Acceptance Criteria for the MVP Implementation

- A Lesson can contain authored text independently of its references.
- A Lesson can have zero or more resources, returned in `Order`.
- A resource has a readable title and either a valid URL or a citation.
- A URL resource is rejected with `400` when its URL is missing, relative, or not `http`/`https`.
- Deleting a Lesson follows an explicit resource-delete rule.
- Tests cover no resources, multiple ordered resources, URL resources, and citation-only resources.

## Shared Resource update (Target)
Lesson resources share a `Resource` entity per Decision 25, using a two-step non-destructive migration. See `docs/features/06-resource-hub.md` and `docs/features/10-automated-task-verification.md`.
