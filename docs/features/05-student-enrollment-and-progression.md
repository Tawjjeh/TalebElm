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
| UserProgress | IsUnlocked | bool | No | Whether the learner may open this Module |
| UserProgress | PassedExam | bool | No | Whether this learner passed the Module exam |
| UserProgress | Score | int | No | Latest score; scale/range is not defined in current code |

The current EF configuration enforces a unique index on `(UserId, ModuleId)`. It does not configure foreign keys or navigation properties.

### Proposed Entity: TrackEnrollment

`UserProgress` records Module state and is not an enrollment record. Add a separate entity for the learner's Track choice:

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|
| TrackEnrollment | Id | Guid | No | Inherited primary key |
| TrackEnrollment | UserId | Guid | No | Required FK to User |
| TrackEnrollment | TrackId | Guid | No | Required FK to Track |
| TrackEnrollment | EnrolledAt | DateTimeOffset | No | Set by the server in UTC |

Add a unique index on `(UserId, TrackId)`. Center membership, payment, attendance, and room booking are separate features and are not fields on TrackEnrollment.

### Relationships & Rules

- One User can enroll in many Tracks; one Track can have many Users.
- One User has at most one progress row per Module.
- On enrollment, create progress for the lowest-Order Module and set `IsUnlocked = true`.
- On a passing Exam, mark the current Module passed and unlock only the next Module in the same Track.
- On failure, record the score and keep later Modules locked.
- A later failed retake never clears `PassedExam` or re-locks a Module that is already unlocked.
- Reject enrollment in a Track with no Modules without creating partial records.
- The last Module has no next Module to unlock.

## 3. Application Contracts

### Current DTO

`ProgressResponse(Guid ModuleId, bool IsUnlocked, bool PassedExam, int Score)`.

### Proposed DTOs and Services

- `EnrollInTrackRequest`: no body fields; Track id comes from the route and User id from authenticated identity.
- `EnrollmentResponse`: Enrollment id, Track id, and EnrolledAt.
- Add `ITrackEnrollmentService.EnrollAsync(Guid userId, Guid trackId)`.
- Change progress queries to accept `userId` explicitly. Application must not read `HttpContext` or JWT claims.

**Current status:** `IUserProgressService.GetMyProgressAsync()` takes no user id and its implementation is a placeholder. The current progress model has no TrackEnrollment entity.

## 4. API Contract

| HTTP Method | Path | Request DTO | Response DTO | Current Status |
|---|---|---|---|---|
| POST | `/api/tracks/{trackId}/enrollment` | None; user comes from auth | EnrollmentResponse | Planned; no endpoint exists |
| GET | `/api/progress/me` | None | `ProgressResponse[]` | Exists but returns 501 Not Implemented |
| GET | `/api/progress/me/tracks/{trackId}` | None | `ProgressResponse[]` | Exists but returns 501 Not Implemented |

Authentication is not wired yet, so current-user routes cannot resolve a learner identity.

## 5. Exceptions & Tests

- `NotFoundException`: User or Track does not exist.
- `ValidationException`: Track has no Modules or duplicate enrollment violates the chosen behavior.
- Test first-Module unlock, duplicate enrollment, empty Track rejection, pass/fail/retry, final Module, and isolation between two Users.
