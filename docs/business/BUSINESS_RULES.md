# TalebElm Business Rules

> Single source of truth for product behavior. Terms match the Domain entities in
> `src/TalebElm.Domain/Entities/` exactly. Every rule states its **enforcement status**:
> **Implemented** (in code today) or **Target** (decided, not yet built — see
> `docs/FEATURE_INDEX.md`). A rule marked Target must not be assumed by new code
> until its enforcement lands. Open questions live in `docs/OPEN_DECISIONS.md`.

## 1. Core concepts

| Concept | Domain type | Definition |
|---|---|---|
| User | `User` | A registered person. `Role` is `UserRole`: `Student` (default; the MVP learner), `Instructor` (reserved), `Admin` (content author/operator). |
| Track | `Track` | An ordered learning path. The product word is **Track**, never Course. |
| Module | `Module` | One ordered section of a Track. Position is `Order` (≥ 1). |
| Lesson | `Lesson` | One ordered study unit inside a Module. Position is `Order` (≥ 1). |
| Exam | `Exam` | The assessment of a Module. At most one per Module; a Module may have none. |
| Progress | `UserProgress` | One learner's state for one Module. Exactly one row per (`UserId`, `ModuleId`). Tracks both exam pass and problem completion flags. |
| Enrollment | `TrackEnrollment` | A learner's explicit choice of a Track. Persisted separately from per-Module progress; the workflow is target behavior. |
| Resource | `LessonResource` / `Resource` | A cited source for a Lesson or a public Resource Hub topic. Persisted separately from lesson text; unifying under shared `Resource` is Decided (MVP) (Decision 25). |
| Technology | `Technology` | Hierarchical node in the public Resource Hub tree (Language, Framework, Topic, Tool). Target behavior. |
| External Account | `UserExternalAccount` | Learner profile on an external problem platform (Codeforces, GitHub) with verification challenge state. Target behavior. |
| Module Problem | `ModuleProblem` | An assigned practice problem (Codeforces) or practical task (GitHub) attached to a Module. Target behavior. |
| Problem Completion | `UserProblemCompletion` | Record of verified solve on an external platform or reviewed framework task. Target behavior. |
| Capstone Project | `CapstoneProject` | Comprehensive portfolio project or progressive track milestone deliverables. Target/Proposed behavior. |

All entities inherit `BaseEntity`: `Id` (auto-generated `Guid`, never reused, never reassigned) and `CreatedAt` (server UTC at insert, immutable afterwards).

### Role taxonomy (decision D-1)

| Role | Enum value | MVP status | Meaning |
|---|---|---|---|
| `Student` | 1 | Active (default) | A registered user who consumes a Track. This is the MVP learner. |
| `Instructor` | 2 | Reserved | Future teaching/authoring role. No MVP permission differs from `Student`; content authoring is `Admin`-only in the MVP. |
| `Admin` | 3 | Active | Content author/operator. May perform learner actions and all `Admin`-only operations. |

**`Learner` is not a role.** It is an aggregate product concept meaning "any authenticated user who consumes learning content", not a value stored in `UserRole`, a token claim, or the database. In the MVP the learner is the `Student`; an `Admin` may also act as a learner for support and testing. Authorization policies are `StudentOnly` (`Student`, with `Admin` override) and `AdminOnly` (`Admin`).

Decision rationale: the `UserRole` enum already ships with `Student`/`Instructor`/`Admin`; adding a fourth `Learner` value would duplicate `Student` and force a data migration, while removing `Instructor` would break the shipped enum and the future authoring split. Keeping `Instructor` reserved costs one unused value and is documented as non-MVP rather than left ambiguous.

## 2. User management & identity

| ID | Rule | Status |
|---|---|---|
| U-1 | `Name` is required, max 100, non-blank (whitespace-only rejected). Enforced by `CHECK (length(trim(Name)) > 0)` **and** the register validator. | Implemented (DB) / Target (register validator) |
| U-2 | `Email` is required, max 256, non-blank, and unique **case-insensitively** (`NOCASE` collation: `A@test.com` ≡ `a@test.com`). Duplicates return `409 Conflict`. | Unique index implemented / Target (409 mapping) |
| U-3 | `PasswordHash` is required and non-blank. A raw password is never stored, never logged, and never appears in any DTO — `UserResponse` carries `Id`, `Name`, `Email` only (guarded by reflection test). | Implemented |
| U-4 | New users default to `Role = Student`. Self-registration never assigns `Instructor` or `Admin`; the first `Admin` is provisioned out of band. | Implemented (default) / Target (provisioning) |
| U-5 | Registration sets `JoinedAt` to server UTC. `JoinedAt` (business fact) is independent of `CreatedAt` (technical fact); imports may backfill a different join date. | Target (registration flow) |
| U-6 | Login with unknown email **or** wrong password returns `401` without revealing which failed. No account enumeration. | Target (auth service) |
| U-7 | Learner history (`UserProgress`, enrollments) is never cascade-deleted with a User (`DeleteBehavior.Restrict`). | Implemented |

Boundary notes: empty string passes `NOT NULL` in SQLite — hence the `CK_Users_*_NotEmpty` constraints, not just `IsRequired()`. Email comparison must use the same normalization in register, login, and validators.

## 3. Tracks, Modules, Lessons hierarchy

| ID | Rule | Status |
|---|---|---|
| C-1 | A Track is created as `Draft` (`TrackStatus.Draft = 0`, the CLR default) and is invisible to learners until explicitly `Published`. Shipped Tracks are `Archived`, never deleted. | Implemented (entity + default) |
| C-2 | Only `Published` Tracks are browsable and enrollable. Learner reads filter by status; admin reads are unfiltered. | Target (services/API) |
| C-3 | Lengths: `Track.Name` ≤ 150, `Track.Description` ≤ 1000, `Module.Title` ≤ 150, `Module.Summary` ≤ 500, `Lesson.Title` ≤ 150. Oversize input is rejected at both the validator (`400`) and the database boundary. | Implemented (DB); validator coverage/wiring target |
| C-4 | Ordering is strict and contiguous: `UNIQUE(TrackId, Order)` for Modules, `UNIQUE(ModuleId, Order)` for Lessons; `Order ≥ 1`; surviving siblings are re-sequenced to `1, 2, 3, …` in the same `UnitOfWork` transaction on insert/move/delete. | Unique indexes implemented; re-sequencing target (Decision 12) |
| C-5 | Reordering a `Published` Track after learners started requires product approval. Progress rows are keyed by `ModuleId`, never by `Order`, so renames of positions never orphan history. | Target |
| C-6 | Deletes never cascade learner data: `Track` with Modules/enrollments/progress, `Module` with Lessons/Exam/progress, `Lesson` with resources are blocked with `409 Conflict`. FKs use `DeleteBehavior.Restrict` (configured for Module→Track, Lesson→Module, Exam→Module, UserProgress→User/Module, TrackEnrollment→User/Track, LessonResource→Lesson). | Implemented (Restrict FKs, 409 via `UnitOfWork`) |
| C-7 | `LessonType` (`Text`, `Video`, `Exercise`) exists as an enum but is **not** a `Lesson` property today. Until Decision 13 lands, content kind must not be inferred from anywhere else. | Implemented (as absence) |

## 4. Enrollment

| ID | Rule | Status |
|---|---|---|
| E-1 | Enrollment is explicit: `Published` Tracks are never auto-joined. No request body — Track id from route, User id from auth identity. | Target |
| E-2 | One enrollment per learner + Track (`UNIQUE(UserId, TrackId)`); duplicates return `409` and create nothing. No un-enroll in the MVP. | Index implemented; workflow target |
| E-3 | Enrolling in a Track with zero Modules is rejected with `400` and creates neither enrollment nor progress rows. | Target |
| E-4 | Enrollment creates exactly one progress row — for the lowest-`Order` Module — with `IsUnlocked = true`. Nothing else is pre-created. | Target |

## 5. Assessment & grading

| ID | Rule | Status |
|---|---|---|
| A-1 | `Score` is an integer `0–100`; `PassThreshold` is `1–100` in the same scale; pass is `score ≥ PassThreshold` (`Exam.HasPassed`). | Predicate, validator and DB check (`CK_Exams_PassThreshold_Range`, `1–100`) implemented |
| A-2 | Grading is server-side only, from server-side answer keys. `SubmitExamRequest(ExamId, Score)` accepts a client-supplied score and **must not** be used on the trusted path; it will be replaced by selected-answer submission. | Implemented (as prohibition) |
| A-3 | At most one Exam per Module, enforced by `UNIQUE(ModuleId)`. A Module may have no Exam; such Modules never set `PassedExam` and progress only by explicit unlock. | Implemented |
| A-4 | Retakes are unlimited with no cooldown; no per-attempt history is stored. Each submission overwrites `Score` (latest-wins) but `PassedExam`, once `true`, is never cleared by a later failure (sticky-wins). | Target (submit behavior) |
| A-5 | Each exam question (planned `ExamQuestion`/`ExamOption`) has ≥ 2 options with exactly one correct; `IsCorrect` is server-only and never serialized to learners. | Target |

## 6. Progress tracking & unlock

| ID | Rule | Status |
|---|---|---|
| P-1 | Exactly one `UserProgress` row per (`UserId`, `ModuleId`), enforced by unique index. | Implemented |
| P-2 | Unlock is forward-only, sticky, and gated by Decision 32. `UserProgress` uses sticky `PassedExam` and `CompletedRequiredProblems` (stored, default false). `examGate = !hasExam || PassedExam`; `problemGate = !hasReq || CompletedRequiredProblems`. The next Module unlocks only when `(hasExam || hasReq)` and both gates and a next Module exist. A Module with neither exam nor required problems is unlocked only by Admin manual unlock. Adding a required problem later never re-locks anyone. The check runs after both events inside one UnitOfWork transaction, which commits even when nothing unlocks. | Target (Phase 3) |
| P-3 | An `Admin` may manually unlock any Module for a learner (row created if missing, `Score`/`PassedExam`/`CompletedRequiredProblems` untouched) as an auditable support action. | Target |
| P-4 | Locked content returns `403 Forbidden` (exists but not allowed), never `404`. One learner never sees or changes another's progress (`403` on cross-access). | Target (API) |
| P-5 | Track completion is derived automatically, not stored: all Modules in the Track have `PassedExam = true` and `CompletedRequiredProblems = true`, plus all capstone `ProjectMilestone`s verified. No completion record exists in the MVP. | Target (Decision 27) |

## 7. Public Resource Hub

| ID | Rule | Status |
|---|---|---|
| R-1 | Read access to the Resource Hub is 100% public and anonymous; no authentication or bearer token is accepted or required for browsing and search. | Target |
| R-2 | The taxonomy follows a strict 3-click maximum hierarchy: Language → Framework/Technology → Topic → Resources. Nodes are modeled as self-referencing `Technology` trees. | Target |
| R-3 | Resources must have a non-blank `Title` (max 200), ISO language code, difficulty `Level`, `Kind`, and cost flag (`IsPaid`). At least one of `Url` (absolute `http`/`https`) or `Citation` (max 500) must be present. | Target |
| R-4 | Public anonymous queries only return resources with `Status = Approved`. Resources created by admin default to `Approved`; future community submissions start `PendingApproval`. | Target |
| R-5 | Lesson resources and Hub resources share one underlying `Resource` entity. `LessonResource` is a join entity (`LessonId`, `ResourceId`, `Order`, `Note`), migrated in a two-step non-destructive sequence. | Target (Decision 25/26) |
| R-6 | Scheduled background link checker probes URLs periodically; marks `Status = ResourceStatus.Broken` after 3 consecutive failures, hiding from public lists until repaired. | Target (Decision 25/26) |

## 8. External practice problems & automated verification

| ID | Rule | Status |
|---|---|---|
| X-1 | Practice problems support `ProblemKind`: `CodeforcesProblem` (algorithmic fundamentals) and `GitHubTask` (practical framework tasks), verified 100% automatically via `IProblemVerifier`. | Target (Decision 21) |
| X-2 | Codeforces handle ownership is verified via a 128-bit single-use token placed into profile field (`firstName` or `organization`) returned by public `user.info`. Verified handle is unique per user (`UNIQUE(Platform, Handle)`). | Target (Decision 22) |
| X-3 | Codeforces solve verification queries public `user.status` for `verdict == OK`, matching `contestId` and problem `index`. All-time accepted solves count. Solves are never self-reported. Cooldown is 60s per user. | Target (Decision 29) |
| X-4 | Automated GitHub tasks (CodeCrafters-style): learner links GitHub via OAuth (numeric ID stored), generates repo from official template, pushes to default branch. Platform verifies repo owner, template provenance, Actions workflow success, and blob/tree SHAs against `TaskVersion`. Zero manual gating. | Target (Decision 24/35) |
| X-5 | Problem difficulty calibration: `ModuleProblem` caches Codeforces rating. First Module problems must be rated ≤800, rising gradually. Content guideline and soft validator warning. | Target (Decision 28) |
| X-6 | External service outages (Codeforces or GitHub down/throttled) return a fail-safe 503 response and alter NO learner state. | Target (Decision 31) |

## 9. Cumulative capstone projects

| ID | Rule | Status |
|---|---|---|
| J-1 | A Track concludes with a single cumulative portfolio project built progressively across ordered `ProjectMilestone`s. | Target (Decision 27) |
| J-2 | Each milestone references a `GitHubTask` and is verified by the exact same automated GitHub task pipeline (CodeCrafters-style). Zero manual gating. | Target (Decision 27) |
| J-3 | Track completion is derived automatically when all Track Modules are passed and all Capstone Milestones are verified. Supplementary peer or mentor feedback is optional and non-blocking. | Target (Decision 27) |

## 10. Invariants enforcement map

| Invariant | Database | Application |
|---|---|---|
| One progress row per learner+Module | `UNIQUE(UserId, ModuleId)` ✅ | Upsert, never duplicate-insert |
| Dual-gated module unlock | — | Next module unlocked only if exam passed AND required problems completed |
| One exam per Module | `UNIQUE(ModuleId)` ✅ | Duplicate create → `409` (target) |
| Case-insensitive unique email | `UNIQUE(Email)` + `NOCASE` ✅ | Identical normalization in register/login (target) |
| Non-blank identity fields | `CK_Users_*_NotEmpty` ✅ | Register validator → `400` (target) |
| No silent data loss | `Restrict` FKs ✅ | Blocked delete → `409` (target) |
| Contiguous sibling order | Unique `(parent, Order)` ✅ | Shift/re-sequence in one transaction (target) |
| Length bounds | `HasMaxLength` ✅ | Module/Lesson validators; Track validator gaps → `400` (partial) |
| Server-side grading | — | Submit path ignores client score |
| Verified handle challenge | `UNIQUE(Platform, Handle)` | Token verification via Codeforces `user.info` profile check |
| Automated task integrity | — | GitHub Actions success AND blob/tree SHA integrity matches |
| Anonymous Hub access | — | Public read endpoints bypass authorization filter |

✅ Implemented · ⏳ Target (decided, scheduled)
