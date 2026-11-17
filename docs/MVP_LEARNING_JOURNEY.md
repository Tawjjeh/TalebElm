# MVP Learning Journey

## Product Goal
TalebElm helps a learner study software development through an ordered Track. The learner chooses a Track, studies each Module's Lessons and linked learning sources, passes its Exam, and unlocks the next Module while the platform records progress.

## Learner Flow

1. **Create an account** — the learner provides a name and email. The `User` model stores these fields plus a PBKDF2 `PasswordHash` and `Role` (`Student` default). Registration, login, and token issuance are not implemented; `AuthController.Login` returns `501`.
2. **Browse and choose a Track** — show available Tracks and their Modules in `Order`. `GET /api/tracks` returns a page of Tracks, and `TrackService.CreateAsync` exists behind a `POST /api/tracks` placeholder. `ModuleService` and `LessonService` return the ordered content; their controllers still return `501`.
3. **Enroll in the Track** — create one enrollment for the learner and Track. `TrackEnrollmentService.EnrollAsync` does this today: it rejects a duplicate enrollment with `409`, rejects a Track with no Modules with `400`, and creates the first Module's `UserProgress` row as unlocked. No HTTP endpoint exposes it yet. Do not use `UserProgress` as a substitute for enrollment: it is per user and Module.
4. **Study a Module** — display its ordered Lessons. `Lesson.Content` holds the explanation, `Lesson.Type` marks text, video, or exercise, and `LessonResource` stores books, official documentation, videos, and other sources. `LessonService` returns this data; `LessonsController` still returns `501`.
5. **Take the Module Exam** — load the Exam and its questions, accept answers, and calculate the score on the server. `ExamQuestion` and `ExamOption` hold the questions and the answer key, and `ExamService.SubmitAsync` validates every answer, grades from `Points` and `IsCorrect`, then writes `Score` and the sticky `PassedExam` flag to `UserProgress`. `ExamsController` endpoints still return `501`.
6. **Solve Practice Problems** — for fundamentals modules, solve assigned Codeforces problems verified via the public API against the learner's linked, profile-verified handle; for framework modules, solve practical GitHub tasks verified automatically by the server. The `ModuleProblem`, `GitHubTask`, `UserExternalAccount`, and `UserProblemCompletion` entities exist, and `ICodeforcesClient` / `IGitHubClient` define the calls; neither client is implemented and no verification service writes completions yet.
7. **Record the result** — store the latest score, sticky `PassedExam = true`, and sticky `CompletedRequiredProblems = true` in the learner's `UserProgress` row for that Module. The exam half happens today inside `ExamService.SubmitAsync`; `CompletedRequiredProblems` is never set because problem verification is not implemented.
8. **Unlock the next Module** — on passing **both** the exam and completing required practice problems, unlock only the next Module in the same Track. Failing either gate leaves later Modules locked.
9. **Show progress** — list the learner's Module states in Track order. `UserProgressService.GetMyProgressAsync(userId)` and `GetByTrackAsync(userId, trackId)` read the rows, but `ProgressController` returns `501` and authentication is not implemented, so there is no current-user resolution yet.
10. **Deliver Track Capstone** — build a single cumulative portfolio project across the entire Track, split into ordered milestones verified automatically via GitHub tasks. `CapstoneProject` and `ProjectMilestone` are persisted; per-learner milestone progress, verification, and the derived track completion are not implemented.

## Core Rules

- The first Module becomes available when enrollment is created.
- A passing score is `score >= Exam.PassThreshold` on a `0–100` scale (`PassThreshold` is `1–100`, enforced by validation and by the database check).
- The score is always computed on the server from server-side answer keys; a client-supplied score is ignored.
- Dual-gated unlock: unlocking the next Module requires **both** `PassedExam = true` and `CompletedRequiredProblems = true`. Both flags are sticky once set.
- Problem verification is 100% automated via public APIs (Codeforces `user.status`) or GitHub REST API (workflow run & SHA integrity check); self-reporting or manual reviews are rejected.
- Each submission overwrites `UserProgress.Score` with the latest score (latest-wins), but `PassedExam`, once `true`, is never cleared by a later failed retake (sticky pass). Retries are unlimited with no cooldown in the MVP, and no per-attempt history is stored.
- A Module MAY have no Exam; such a Module never sets `PassedExam` and is progressed only by the explicit unlock rule.
- Passing unlocks only the next Module; it does not unlock the whole Track.
- Unlocks are forward-only and sticky: an unlocked Module is never auto re-locked. An `Admin` may manually unlock any Module for a learner without touching `Score` / `PassedExam` / `CompletedRequiredProblems`.
- Enrollment is explicit: `Published` Tracks are enrollable but never auto-joined. One enrollment per learner + Track (`409` on duplicate); a Track with no Modules cannot be enrolled in.
- A failed attempt or missing required problems must not unlock another Module.
- Progress belongs to one learner and one Module. Enforce uniqueness on `(UserId, ModuleId)`, plus `UNIQUE(TrackId, Order)` for Modules, `UNIQUE(ModuleId, Order)` for Lessons, and `UNIQUE(ModuleId)` for Exams (at most one Exam per Module).
- The final Module has no next Module to unlock; it unlocks the Track Capstone project.
- Only `Published` Tracks can be browsed and enrolled in.
- Deletes never cascade learner data: `Restrict` on all learning-path FKs; blocked deletes return `409` (archive a shipped Track instead of deleting it).
- The score is calculated by the server from the sum of `Points` of correctly answered questions (proposed; see [OPEN_DECISIONS.md](OPEN_DECISIONS.md)).

## Current Code vs. MVP Target

| Area | Current code | MVP target |
|---|---|---|
| User identity | User name/email, `PasswordHash`, `Role`, and a user service | Registration and authentication before protected learner actions |
| Track structure | Track, Module, Lesson entities; `TrackService` paging and creation; Module/Lesson read services; `GET /api/tracks` live | The remaining browse endpoints, wired to the existing services |
| Lesson sources | Lesson text with `Type`, `LessonResource` entity/configuration/repository/migration, Lesson read services | Authoring and URL validation, lesson endpoints, shared Resource Hub |
| Enrollment | `TrackEnrollment` persistence and `TrackEnrollmentService.EnrollAsync`, which unlocks the first Module; no endpoint | An enroll endpoint and the progress endpoints |
| Exams | `Exam`, `ExamQuestion`, `ExamOption`; `ExamService.SubmitAsync` grades on the server and records `Score` / `PassedExam` | Admin authoring, controller wiring, and the exam gate unlock |
| Practice problems | `ModuleProblem`, `GitHubTask`, `UserExternalAccount`, `UserProblemCompletion` persistence; `ICodeforcesClient` / `IGitHubClient` contracts without implementations | Handle verification, solve verification, and automated GitHub task verification |
| Progression | `UserProgress` with `PassedExam` and `CompletedRequiredProblems`, plus read services | The dual-gated unlock transaction and progress queries |
| Resource Hub | `Technology` / `Resource` persistence and their repositories | 3-click anonymous public directory of curated resources across languages and topics |
| Capstone | `CapstoneProject` / `ProjectMilestone` persistence | Milestone verification and derived track completion |
| API | `GET /api/tracks` and `/api/health` are live; the other controllers return `501`; DI, validators, and exception middleware are registered; auth is not | Endpoints wired to Application service interfaces |

## Suggested Delivery Order

1. Wire authentication and authorization policies so protected routes can resolve the current learner (Phase 1).
2. Expose the existing Track, Module, Lesson, enrollment, exam, and progress services over HTTP (Phase 2).
3. Implement practice-problem verification, handle verification, and the dual-gated unlock (Phase 3).
4. Deploy the public read-only Resource Hub (Phase 4, running in parallel).
5. Implement capstone milestone verification and derived track completion (Phase 5).

## Explicitly Out of This Learning-Flow MVP

Scheduling, attendance, payments, in-browser IDE compiler, and contest hosting are separate product concerns. Keep them out of Track enrollment and `UserProgress` until product requirements define how they connect.

## Decision 32 updates
Module progression uses sticky `PassedExam` and `CompletedRequiredProblems`; the next Module unlocks only when both gates pass (or only when the existing gate exists).
