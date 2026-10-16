# Feature Specification: Lessons & Learning Resources

## Status
`Lesson` exists in Domain with `Title`, `Content`, `Order`, and `ModuleId`. It has no resource collection or source metadata. The Lesson controller is a placeholder, and no Lesson service exists.

## MVP Objective
Let a learner study an authored explanation for each Lesson and open the books, official documentation, videos, or other sources selected by the instructor. Keep lesson content and external resource links as separate data so a lesson can cite more than one source.

## Current Domain Contract

| Entity | Field | Type | Nullable | Constraints / Notes |
|---|---|---|---|---|
| Lesson | Id | Guid | No | Inherited primary key |
| Lesson | CreatedAt | DateTimeOffset | No | Inherited creation timestamp |
| Lesson | Title | string | No | Required by the non-nullable model; no max length configured |
| Lesson | Content | string | No | Lesson explanation; currently the only learning material field |
| Lesson | Order | int | No | Display order within a Module |
| Lesson | ModuleId | Guid | No | Parent Module identifier |

### Relationship
- `Lesson.ModuleId` identifies the parent Module. The current entity has no navigation property and EF relationship configuration is not complete.

## Proposed MVP Resource Contract

Add a `LessonResource` entity rather than packing external links into `Lesson.Content`:

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

### Suggested Application Contract

- Add `LessonResourceResponse` with `Id`, `Title`, `Kind`, `Url`, `Citation`, and `Order`.
- Add `LessonDetailResponse` with `Id`, `ModuleId`, `Title`, `Content`, `Order`, and an ordered resource list.
- Add a create/update request only when lesson authoring is scheduled; no Lesson service or request DTO currently exists.

### Suggested API Contract (Not Implemented)

| Method | Route | Purpose | Status |
|---|---|---|---|
| GET | `/api/modules/{moduleId}/lessons` | List lessons in `Order` | Planned; no matching controller action |
| GET | `/api/lessons/{lessonId}` | Return explanation and resources | Planned; current `LessonsController.Get()` has no id parameter and throws `NotImplementedException` |

### Resource validation rules (proposed)

- `Title` is required and has a planned max length of **200**.
- At least one of `Url` or `Citation` must be provided; both may be provided.
- When `Url` is present it must be a valid **absolute** URL using the `http` or
  `https` scheme. Reject relative paths, empty strings, and unsupported schemes
  with `ValidationException` (HTTP `400`).
- `Citation` is free text with a planned max length of **500**.
- `Order` must be a positive integer; ordering is re-sequenced within the Lesson
  on create/update/delete, mirroring the Module/Lesson ordering rule.

## Acceptance Criteria for the MVP Implementation

- A Lesson can contain authored text independently of its references.
- A Lesson can have zero or more resources, returned in `Order`.
- A resource has a readable title and either a valid URL or a citation.
- A URL resource is rejected with `400` when its URL is missing, relative, or not `http`/`https`.
- Deleting a Lesson follows an explicit resource-delete rule.
- Tests cover no resources, multiple ordered resources, URL resources, and citation-only resources.
