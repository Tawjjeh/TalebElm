# Feature Specification: Track Enrollment & Student Progress

## 1. MVP Scope & Objective
A signed-in learner chooses a Track and enrolls once. Enrollment opens the first ordered Module; exam results then update per-Module progress and passing unlocks only the next Module.

## 2. Database & Domain Contract

### Current Entity: UserProgress

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|
| UserProgress | Id | Guid | No | Inherited primary key |
| UserProgress | CreatedAt | DateTimeOffset | No | Inherited UTC timestamp |
| UserProgress | UserId | Guid | No | Learner id; unique with ModuleId |
| UserProgress | ModuleId | Guid | No | Module id; unique with UserId |
| UserProgress | IsUnlocked | bool | No | Whether the learner may open this Module; sticky — never auto re-locked once `true` |
| UserProgress | PassedExam | bool | No | Whether this learner passed the Module exam; sticky — never reset by a later failed retake |
| UserProgress | Score | int | No | Latest attempt score on the `0–100` scale (latest-wins) |

The EF configuration enforces a unique index on `(UserId, ModuleId)` (exactly one row per learner + Module), `Restrict` delete behavior on both FKs, and plain-CLR navigation properties (`UserProgress.User`, `UserProgress.Module`).

### Current Entity: TrackEnrollment (Persistence Implemented)

`UserProgress` records Module state and is not an enrollment record. A separate `TrackEnrollment` entity stores the learner's Track choice. The entity, EF configuration (including the unique `(UserId, TrackId)` index and `Restrict` FKs), repository, `IUnitOfWork.TrackEnrollments`, and `AddTrackEnrollment` migration exist; the enrollment workflow and endpoint are still target behavior.

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|
| TrackEnrollment | Id | Guid | No | Inherited primary key |
| TrackEnrollment | UserId | Guid | No | Required FK to User |
| TrackEnrollment | TrackId | Guid | No | Required FK to Track |
| TrackEnrollment | EnrolledAt | DateTimeOffset | No | Set by the server in UTC |

The unique index on `(UserId, TrackId)` is implemented. Payment and attendance are separate features and are not fields on TrackEnrollment.

### Relationships & Rules (Target Workflow)

- One User can enroll in many Tracks; one Track can have many Users.
- One User has at most one progress row per Module (`UNIQUE(UserId, ModuleId)`).
- On enrollment, create progress for the lowest-Order Module and set `IsUnlocked = true`. No other rows are pre-created.
- On a passing Exam, mark the current Module passed and unlock only the next Module in the same Track.
- On failure, record the latest score and keep later Modules locked.
- A later failed retake never clears `PassedExam` or re-locks a Module that is already unlocked (both flags are sticky).
- An `Admin` may manually unlock any Module for a learner (creates the row if missing, sets `IsUnlocked = true`, leaves `Score` / `PassedExam` untouched).
- Reject enrollment in a Track with no Modules without creating partial records.
- Reject duplicate enrollment with `409 Conflict`; there is no un-enroll in the MVP.
- Modules without an Exam never set `PassedExam`; they progress only via the explicit unlock rule.
- New Modules added after enrollment start locked; inserting in the middle shifts sibling `Order` values in one transaction. Reordering a `Published` Track after learners start requires product approval.
- Deletes use `Restrict` (see `DATABASE_ERD.md` §5.4): blocked deletes return `409`; learner history is never cascaded.

## 3. Application Contracts

### Current DTO

`ProgressResponse(Guid ModuleId, bool IsUnlocked, bool PassedExam, int Score)`.

### Proposed DTOs and Services

- `EnrollInTrackRequest`: no body fields; Track id comes from the route and User id from authenticated identity.
- `EnrollmentResponse`: Enrollment id, Track id, and EnrolledAt.
- Add `ITrackEnrollmentService.EnrollAsync(Guid userId, Guid trackId)`.
- Change progress queries to accept `userId` explicitly. Application must not read `HttpContext` or JWT claims.

**Current status:** `TrackEnrollment` persistence (entity, repository, migration) exists, but no enrollment service/endpoint is wired. `IUserProgressService.GetMyProgressAsync()` still takes no user id and its implementation is a placeholder.

## 4. API Contract

| HTTP Method | Path | Request DTO | Response DTO | Current Status |
|---|---|---|---|---|
| POST | `/api/tracks/{trackId}/enrollment` | None; user comes from auth | EnrollmentResponse | Planned; no endpoint exists |
| GET | `/api/progress/me` | None | `ProgressResponse[]` | Exists but returns 501 Not Implemented |
| GET | `/api/progress/me/tracks/{trackId}` | None | `ProgressResponse[]` | Exists but returns 501 Not Implemented |

Authentication is not wired yet, so current-user routes cannot resolve a learner identity.

## 5. Exceptions & Tests

- `NotFoundException`: User or Track does not exist.
- `ValidationException`: Track has no Modules.
- `ConflictException`: duplicate enrollment violates the unique `(UserId, TrackId)` rule.
- Test first-Module unlock, duplicate enrollment, empty Track rejection, pass/fail/retry, final Module, and isolation between two Users.

## Decision 32 update
`UserProgress` stores sticky `CompletedRequiredProblems` (default false) as a Target field, with migration in Phase 3. `examGate = !hasExam || PassedExam`; `problemGate = !hasReq || CompletedRequiredProblems`. The next Module unlocks only when `(hasExam || hasReq)` and both gates and a next Module exist. A Module with neither uses Admin manual unlock. The unlock check runs after both events inside one UnitOfWork transaction. `ProgressResponse` also exposes `RequiredProblemsTotal` and `RequiredProblemsCompleted`.
