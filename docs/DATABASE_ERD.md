# TalebElm Data Model: Current and Proposed

This document distinguishes Domain entities that exist in code from the relational rules the MVP still needs. `AppDbContext` exposes the current entities and applies configuration classes, but most relationship configurations are empty.

## 1. Current Domain Entities

| Entity | Properties | Current constraints/status |
|---|---|---|
| User | `Id: Guid`, `CreatedAt: DateTimeOffset`, `Name: string`, `Email: string`, `JoinedAt: DateTime` | No password, role, or enrollment property. No email uniqueness constraint. |
| Track | `Id`, `CreatedAt`, `Name`, `Description`, `Status: TrackStatus` | `TrackStatus`: Draft=0, Published=1, Archived=2. |
| Module | `Id`, `CreatedAt`, `Title`, `Summary`, `Order: int`, `TrackId: Guid` | Intended parent Track is identified by `TrackId`. |
| Lesson | `Id`, `CreatedAt`, `Title`, `Content`, `Order: int`, `ModuleId: Guid` | No `LessonType` property; `LessonType` enum exists separately. No source/resource collection. |
| Exam | `Id`, `CreatedAt`, `Title`, `PassThreshold: int`, `ModuleId: Guid` | `HasPassed(score)` returns `score >= PassThreshold`. No question model. |
| UserProgress | `Id`, `CreatedAt`, `UserId: Guid`, `ModuleId: Guid`, `IsUnlocked: bool`, `PassedExam: bool`, `Score: int` | Unique index exists on `(UserId, ModuleId)`. No explicit FK mapping/navigation properties. |

`Id` and `CreatedAt` are inherited from `BaseEntity`.

## 2. Relationships

### Intended learning relationships

- Track 1 → many Modules, ordered by `Module.Order`.
- Module 1 → many Lessons, ordered by `Lesson.Order`.
- Module 0/1 → Exam (product target: at most one exam per Module).
- User 1 → many UserProgress rows.
- Module 1 → many UserProgress rows.

### Current EF Core mapping status

- `UserProgressConfiguration` maps the primary key and unique `(UserId, ModuleId)` index.
- `TrackConfiguration`, `ModuleConfiguration`, and `ExamConfiguration` currently have empty `Configure` methods.
- The intended relationships above are not all enforced by explicit EF foreign keys in current configuration.
- Navigation properties are not declared on the current entities.

## 3. Proposed MVP Entities Not Yet in Code

### TrackEnrollment

Keep Track enrollment distinct from per-Module progress. Proposed fields: `Id`, `UserId`, `TrackId`, `EnrolledAt`; add unique `(UserId, TrackId)`. Enrollment creates the initial UserProgress row for the first ordered Module.

### LessonResource

Allow one Lesson to cite multiple learning sources. Proposed fields: `Id`, `LessonId`, `Title`, `Kind`, `Url?`, `Citation?`, and `Order`. Require at least one of Url or Citation. Resource kinds can include Book, Documentation, Video, Article, Repository, and Other.

### ExamQuestion and ExamOption

For the first exam format, use ordered multiple-choice questions. `ExamQuestion` belongs to Exam and stores Prompt, Order, and Points. `ExamOption` belongs to a question and stores learner-visible Text plus server-only `IsCorrect`. Never return `IsCorrect` in a learner response.

### Center and Room

These are optional operational features, not part of the self-paced learning flow. No fields or relationships are finalized. If approved later, decide whether a Center owns Tracks, Rooms, or both before adding FKs.

## 4. Target Progression Transaction

1. Create a TrackEnrollment for the authenticated User and selected Track.
2. Create UserProgress for the lowest-Order Module with `IsUnlocked = true`.
3. Compute the score on the server from the selected answers (the MVP does not store attempt history).
4. Update the current Module's progress with the latest score; set `PassedExam = true` on a pass and never reset it on a later failed retake.
5. On pass, unlock only the next Module in the same Track; on failure, leave later Modules locked.
6. Commit the result and progress changes in one UnitOfWork operation.
