# MVP Learning Journey

## Product Goal
TalebElm helps a learner study software development through an ordered Track. The learner chooses a Track, studies each Module's Lessons and linked learning sources, passes its Exam, and unlocks the next Module while the platform records progress.

## Learner Flow

1. **Create an account** — the learner provides a name and email. The `User` model stores these fields plus a PBKDF2 `PasswordHash` and `Role` (`Student` default); registration, login, and token issuance are not implemented.
2. **Browse and choose a Track** — show available Tracks and their Modules in `Order`. `Track`, `Module`, and `TrackStatus` exist; Track list/create services exist, while the HTTP API remains a placeholder.
3. **Enroll in the Track** — create one enrollment for the learner and Track. The `TrackEnrollment` entity, repository, and migration exist; the enrollment workflow is not implemented. Do not use `UserProgress` as a substitute: it is per user and Module.
4. **Study a Module** — display its ordered Lessons. `Lesson.Content` holds the explanation, and `LessonResource` stores books, official documentation, videos, and other sources; lesson HTTP endpoints remain placeholders.
5. **Take the Module Exam** — load the Exam and its questions, accept answers, and calculate the score on the server. The current model stores only Exam title and pass threshold; it has no question/answer model. The current submit DTO accepts a client-provided score and is not suitable for trusted grading.
6. **Solve Practice Problems** — for fundamentals modules, solve assigned Codeforces problems verified via public API against the learner's linked profile-verified handle; for framework modules, solve practical GitHub tasks verified automatically by the server.
7. **Record the result** — store the latest score, sticky `PassedExam = true`, and sticky `CompletedRequiredProblems = true` in the learner's `UserProgress` row for that Module.
8. **Unlock the next Module** — on passing **both** the exam and completing required practice problems, unlock only the next Module in the same Track. Failing either gate leaves later Modules locked.
9. **Show progress** — list the learner's Module states in Track order. Authentication and current-user resolution are not implemented yet.
10. **Deliver Track Capstone** — build a single cumulative portfolio project across the entire Track, split into ordered milestones verified automatically via GitHub tasks. Track completion is derived: all Modules passed AND all capstone Milestones verified.

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
| User identity | User name/email, `PasswordHash`, `Role`, and user service contract | Registration and authentication before protected learner actions |
| Track structure | Track, Module, Lesson entities; Track/Module/Lesson read services | Browse a Track and its ordered content |
| Lesson sources | Lesson text plus `LessonResource` entity/repository | Resource authoring, validation, and lesson endpoints; shared Resource Hub |
| Enrollment | `TrackEnrollment` entity/repository exists; no enrollment service | One enrollment per learner and Track; initialize the first Module's progress |
| Exams | Exam title and threshold; service supports loading an Exam | Questions, answer submission, server-side scoring, and result recording |
| Practice problems | None | Codeforces handle token verification, solve verification API, automated GitHub tasks |
| Progression | UserProgress entity and read/service contracts | Dual-gated unlock rules (exam AND problems) and progress queries |
| Resource Hub | None | 3-click anonymous public directory of curated resources across languages and topics |
| Capstone | None | Cumulative track portfolio project and milestones verified automatically via GitHub tasks |
| API | Placeholder controllers; DI, Application services, validators, and exception middleware are registered; auth is not | Endpoints wired to Application service interfaces |

## Suggested Delivery Order

1. Finish database setup, mappings, repositories, and UnitOfWork (#274; #326 resolved in PR #328).
2. Wire authentication and authorization policies (Phase 1).
3. Implement Track enrollment, Exam questions, and server-side answer scoring (Phase 2).
4. Integrate practice problems, handle verification, and dual-gated progression (Phase 3).
5. Deploy the public read-only Resource Hub (Phase 4, running in parallel).
6. Implement Capstone project definitions and automated milestone verification (Phase 5).

## Explicitly Out of This Learning-Flow MVP

Scheduling, attendance, payments, in-browser IDE compiler, and contest hosting are separate product concerns. Keep them out of Track enrollment and `UserProgress` until product requirements define how they connect.
