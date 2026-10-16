# TalebElm Data Model: Current and Proposed

This document distinguishes Domain entities that exist in code from the relational rules the MVP still needs. `AppDbContext` exposes the current entities and applies configuration classes, but most relationship configurations are empty.

## 1. Current Domain Entities

| Entity | Properties | Current constraints/status |
|---|---|---|
| User | `Id: Guid`, `CreatedAt: DateTimeOffset`, `Name: string`, `Email: string`, `JoinedAt: DateTime` | No password, role, or enrollment property. `Email` must gain a unique index at implementation time. |
| Track | `Id`, `CreatedAt`, `Name`, `Description`, `Status: TrackStatus` | `TrackStatus`: Draft=0, Published=1, Archived=2. New Tracks default to `Draft`. |
| Module | `Id`, `CreatedAt`, `Title`, `Summary`, `Order: int`, `TrackId: Guid` | Intended parent Track is identified by `TrackId`. MVP requires `UNIQUE(TrackId, Order)` (see §5). |
| Lesson | `Id`, `CreatedAt`, `Title`, `Content`, `Order: int`, `ModuleId: Guid` | No `LessonType` property; `LessonType` enum exists separately. No source/resource collection. MVP requires `UNIQUE(ModuleId, Order)` (see §5). |
| Exam | `Id`, `CreatedAt`, `Title`, `PassThreshold: int`, `ModuleId: Guid` | `HasPassed(score)` returns `score >= PassThreshold`. No question model. MVP requires `UNIQUE(ModuleId)` — at most one exam per Module (see §5). |
| UserProgress | `Id`, `CreatedAt`, `UserId: Guid`, `ModuleId: Guid`, `IsUnlocked: bool`, `PassedExam: bool`, `Score: int` | Unique index exists on `(UserId, ModuleId)`. `Score` stores the **latest** attempt score (0–100 scale); `PassedExam` is sticky (see §4). |

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
- `ModuleConfiguration` maps the `Module.TrackId → Track.Id` foreign key with `DeleteBehavior.Restrict` (merged via #262); remaining configurations are being completed with navigation-based mappings per §5.
- Every relationship in §2 must be enforced by an explicit EF foreign key with `DeleteBehavior.Restrict` plus the unique indexes in §5 — see the target configurations in [ARCHITECTURE.md](ARCHITECTURE.md) §6.
- Navigation properties (`Track.Modules`, `Module.Track`/`Lessons`/`Exam`, `Lesson.Module`, `Exam.Module`, `UserProgress.User`/`Module`) are the MVP target; treat any entity still missing them as pending implementation, not as the design.

## 3. Proposed MVP Entities Not Yet in Code

### TrackEnrollment

Keep Track enrollment distinct from per-Module progress. Proposed fields: `Id`, `UserId`, `TrackId`, `EnrolledAt`; add unique `(UserId, TrackId)`. Enrollment creates the initial UserProgress row for the first ordered Module.

### LessonResource

Allow one Lesson to cite multiple learning sources. Proposed fields: `Id`, `LessonId`, `Title`, `Kind`, `Url?`, `Citation?`, and `Order`. Require at least one of Url or Citation. Resource kinds can include Book, Documentation, Video, Article, Repository, and Other.

### ExamQuestion and ExamOption

For the first exam format, use ordered multiple-choice questions. `ExamQuestion` belongs to Exam and stores Prompt, Order, and Points. `ExamOption` belongs to a question and stores learner-visible Text plus server-only `IsCorrect`. Never return `IsCorrect` in a learner response.

## 4. Target Progression Transaction

1. Create a TrackEnrollment for the authenticated User and selected Track.
2. Create UserProgress for the lowest-Order Module with `IsUnlocked = true`.
3. Compute the score on the server from the selected answers (the MVP does not store attempt history).
4. Update the current Module's progress with the latest score; set `PassedExam = true` on a pass and never reset it on a later failed retake.
5. On pass, unlock only the next Module in the same Track; on failure, leave later Modules locked.
6. Commit the result and progress changes in one UnitOfWork operation.

## 5. MVP Integrity Rules (binding)

These rules close design-review questions Q2–Q3, Q5, Q7–Q12. They are binding for implementation and tests.

### 5.1 Exam scoring, retries, and pass validation

- **Score scale:** `Score` is an integer `0–100`. `Exam.PassThreshold` is an integer `1–100` in the same scale. `Exam.HasPassed(score)` is `score >= PassThreshold`.
- **Server-side grading only:** the score is computed by the Application service from server-side answer keys. A client-supplied score is never trusted.
- **Retries:** unlimited attempts, no cooldown in the MVP.
- **Latest-wins for `Score`, sticky-wins for `PassedExam`:** each submission overwrites `UserProgress.Score` with the latest attempt score, but `PassedExam`, once `true`, is never reset to `false` by a later failed retake. A pass is permanent.
- **No attempt history in MVP:** only the aggregate outcome lives in `UserProgress`. Do not add `ExamAttempt` / `StudentAnswer` tables until history or analytics is an approved requirement.
- **Modules without exams:** a Module MAY exist without an Exam in the MVP. A Module with no Exam never sets `PassedExam`; progression past it is defined by the enrollment/progression spec (explicit unlock rule), not by an implicit pass.

### 5.2 Unique ordering constraints

Enforced at the database boundary (unique indexes) **and** in Application validators:

| Table | Constraint | Meaning |
|---|---|---|
| `Module` | `UNIQUE(TrackId, Order)` | No two Modules share an `Order` within one Track. |
| `Lesson` | `UNIQUE(ModuleId, Order)` | No two Lessons share an `Order` within one Module. |
| `UserProgress` | `UNIQUE(UserId, ModuleId)` | Exactly one progress row per learner + Module. |
| `Exam` | `UNIQUE(ModuleId)` | At most one Exam per Module. |
| `TrackEnrollment` (planned) | `UNIQUE(UserId, TrackId)` | One enrollment per learner + Track. |

Reordering or inserting in the middle must shift sibling `Order` values inside the same `UnitOfWork` transaction so the constraint never observes a duplicate. Reordering a `Published` Track after learners have started is discouraged: existing `UserProgress` rows stay keyed to Modules (by `ModuleId`, never by `Order`), but product must confirm the unlock impact before approving the change.

### 5.3 Module lifecycle: unlock, re-lock, enrollment

- **Initial progress:** enrollment creates the `TrackEnrollment` row and exactly one `UserProgress` row — for the lowest-`Order` Module — with `IsUnlocked = true`. No other rows are pre-created.
- **Forward-only unlock:** a pass unlocks only the next Module in the same Track (creates its `UserProgress` row with `IsUnlocked = true` if absent). A fail records the score and unlocks nothing. The final Module has no next Module.
- **No silent re-lock:** a Module that is unlocked is never automatically re-locked (not by a later failed retake, not by reordering). `IsUnlocked`, like `PassedExam`, is sticky.
- **Manual unlock:** an `Admin` may explicitly unlock any Module for a learner (support/admin operation). The operation creates the `UserProgress` row if missing and sets `IsUnlocked = true` without altering `Score` / `PassedExam`. Every manual unlock is an auditable admin action.
- **Enrollment:** learners must explicitly enroll; `Published` Tracks are **not** auto-available. Duplicate enrollment returns `409 Conflict`. Enrolling in a Track with zero Modules is rejected with no partial rows. There is no un-enroll in the MVP.

### 5.4 Delete policy: `Restrict`, never silent cascade

All learning-path foreign keys use `DeleteBehavior.Restrict`. There is no soft delete in the MVP and no cascading delete of learner data.

| Target | Rule |
|---|---|
| `Track` | Blocked (`409`) while any `Module`, `TrackEnrollment`, or `UserProgress` (via its Modules) references it. Archive (`Status = Archived`) instead of deleting a shipped Track. |
| `Module` | Blocked (`409`) while any `Lesson`, `Exam`, or `UserProgress` references it. Deleting a Module requires first removing its Lessons/Exam and resolving learner rows by product decision; then re-sequence surviving siblings to contiguous `Order` (`1, 2, 3, …`) in the same transaction. |
| `Lesson` | Blocked (`409`) while learner-facing linkage depends on it (resources, progress views). After deletion, re-sequence surviving Lessons in the Module contiguously in the same transaction. |
| `Exam` | Deleting an Exam is allowed only when no grading depends on it; it never deletes the parent Module or learner `UserProgress` rows. |
| `User` | Learner history is preserved: never cascade-delete `TrackEnrollment` / `UserProgress` rows when a user is removed. Account removal follows the auth/identity retention policy. |
