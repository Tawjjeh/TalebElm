# TalebElm Business Rules (Single Source of Truth)

> **Documentation as Code.** This file is the binding product contract for maintainers and developers.
> Terms match the Domain entities in `src/TalebElm.Domain/Entities/` exactly.
> Technical enforcement (configurations, validators, endpoints) is specified in
> `docs/FEATURE_INDEX.md` and the `docs/features/` specs — anything that contradicts
> this file is a bug in the other document, not here.
> Open product questions live in `docs/OPEN_DECISIONS.md`.

## 1. Ubiquitous language

| Term | Domain type | Meaning |
|---|---|---|
| User / Learner | `User` | A registered person. `Role` is `UserRole`: `Student` (default learner), `Instructor`, `Admin`. |
| Track | `Track` | An ordered learning path (e.g. ".NET"). Lifecycle: `Draft → Published → Archived`. |
| Module | `Module` | One ordered section of a Track (`Order ≥ 1`, contiguous per Track). |
| Lesson | `Lesson` | One ordered study unit inside a Module (`Order ≥ 1`, contiguous per Module). |
| Exam | `Exam` | At most one assessment per Module. `PassThreshold` 1–100. |
| Progress | `UserProgress` | One learner's state for one Module. Exactly one row per (`UserId`, `ModuleId`). |
| Enrollment | *(planned `TrackEnrollment`)* | A learner's explicit choice of a Track. Distinct from per-Module progress. |

`Track` is the product word for what curricula call a "course". Code and docs say **Track**, never Course.

## 2. Users & identity

| # | Rule |
|---|---|
| U-1 | `Name` required, max 100, non-blank. `Email` required, max 256, unique **case-insensitive** (`NOCASE`), non-blank. `PasswordHash` required, non-blank — a raw password is never stored, logged, or returned in any DTO. |
| U-2 | New users default to `Role = Student`. The first `Admin` is seeded/provisioned, never self-registered. |
| U-3 | Duplicate email (any casing) is rejected with `409 Conflict`. Unknown email **or** wrong password on login returns `401` without revealing which failed. |
| U-4 | Learner history is never cascade-deleted with a User. Account removal follows the retention policy in `features/01-auth-identity.md`. |

## 3. Tracks & Modules

| # | Rule |
|---|---|
| T-1 | Every new Track starts as `Draft` and is invisible to learners. Only `Published` Tracks are browsable and enrollable. Shipped Tracks are `Archived`, never deleted. |
| T-2 | `Track.Name` ≤ 150, `Track.Description` ≤ 1000. `Module.Title` ≤ 150, `Module.Summary` ≤ 500. |
| T-3 | Module ordering is strict: `UNIQUE(TrackId, Order)`; `Order ≥ 1`; surviving siblings stay contiguous (`1, 2, 3, …`) after any insert, move, or delete — shifted in a single transaction. |
| T-4 | Reordering a `Published` Track after learners started requires product approval; progress rows are keyed by `ModuleId`, never by `Order`. |
| T-5 | Deletes never cascade: `Track` with Modules/enrollments/progress, or `Module` with Lessons/Exam/progress, is blocked with `409 Conflict` (`DeleteBehavior.Restrict` on all learning-path FKs). |

## 4. Lessons & resources

| # | Rule |
|---|---|
| L-1 | `Lesson.Title` ≤ 150; `Lesson.Order` follows the same strict-contiguous rule within its Module (`UNIQUE(ModuleId, Order)`). |
| L-2 | A Lesson's authored text lives in `Content`; external sources (books, docs, videos) live in a separate resource list, each with a title and a URL and/or citation. Resource URLs must be absolute `http(s)`. |
| L-3 | Lessons of a locked Module are never served (see §6). |

## 5. Enrollment

| # | Rule |
|---|---|
| E-1 | Enrollment is **explicit**: `Published` Tracks are never auto-joined. |
| E-2 | One enrollment per learner + Track (`UNIQUE(UserId, TrackId)`); duplicates return `409`; nothing is created on failure. No un-enroll in the MVP. |
| E-3 | Enrolling in a Track with zero Modules is rejected with no partial rows. |
| E-4 | Enrollment creates exactly one progress row — for the lowest-`Order` Module — with `IsUnlocked = true`. Nothing else is pre-created. |

## 6. Assessment & progression

| # | Rule |
|---|---|
| A-1 | `Score` is `0–100`, computed **server-side** from server-side answer keys. Client-supplied scores are never trusted. Pass is `score ≥ PassThreshold` (`Exam.HasPassed`). |
| A-2 | At most one Exam per Module (`UNIQUE(ModuleId)`); a Module MAY have no Exam, in which case it never sets `PassedExam` and progresses only via explicit unlock. |
| A-3 | Retries are unlimited with no cooldown; no per-attempt history is stored. Each submission overwrites `Score` (latest-wins) but `PassedExam`, once `true`, is **never** cleared by a later failure (sticky-wins). |
| A-4 | Unlock is forward-only and sticky: a pass unlocks **only** the next Module in the same Track; a fail unlocks nothing; an unlocked Module is never auto re-locked; the final Module unlocks nothing further. |
| A-5 | An `Admin` may manually unlock any Module for a learner (row created if missing, `Score`/`PassedExam` untouched) as an auditable support action. |
| A-6 | Progress reads and exam views/submits for a locked Module return `403 Forbidden` (exists but not yet allowed), never `404`. Cross-learner access returns `403`. |

## 7. Invariants index (enforcement map)

| Invariant | DB (unique index / CHECK / Restrict) | Application (validator / service) |
|---|---|---|
| One progress row per learner+Module | `UNIQUE(UserId, ModuleId)` | Service upserts, never duplicate-inserts |
| Contiguous module order | `UNIQUE(TrackId, Order)` | Shift/re-sequence in one `UnitOfWork` transaction |
| Contiguous lesson order | `UNIQUE(ModuleId, Order)` | Same as above, scoped to Module |
| One exam per Module | `UNIQUE(ModuleId)` | Duplicate create → `409` |
| One enrollment per learner+Track | `UNIQUE(UserId, TrackId)` | Duplicate enroll → `409`, create nothing |
| Case-insensitive unique email | `UNIQUE(Email)` + `NOCASE` | Normalize identically in register/login |
| Non-blank identity fields | `CK_Users_*_NotEmpty` | Register validator rejects blanks (`400`) |
| No silent data loss | `Restrict` on all learning-path FKs | Blocked delete → `409`, archive instead |
