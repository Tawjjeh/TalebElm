# MVP Learning Journey

## Product Goal
TalebElm helps a learner study software development through an ordered Track. The learner chooses a Track, studies each Module's Lessons and linked learning sources, passes its Exam, and unlocks the next Module while the platform records progress.

## Learner Flow

1. **Create an account** — the learner provides a name and email. The current `User` model stores these fields, but registration, password storage, and login are not implemented.
2. **Browse and choose a Track** — show available Tracks and their Modules in `Order`. The current model has `Track`, `Module`, and `TrackStatus`; Track read API behavior is incomplete.
3. **Enroll in the Track** — create one enrollment for the learner and Track. There is no Enrollment entity or workflow yet. Do not use `UserProgress` as a substitute: it is per user and Module.
4. **Study a Module** — display its ordered Lessons. A Lesson currently has a text `Content` field; books, official documentation, videos, and other sources need a separate resource contract.
5. **Take the Module Exam** — load the Exam and its questions, accept answers, and calculate the score on the server. The current model stores only Exam title and pass threshold; it has no question/answer model. The current submit DTO accepts a client-provided score and is not suitable for trusted grading.
6. **Record the result** — store the score and pass state in the learner's `UserProgress` row for that Module.
7. **Unlock the next Module** — on a pass, unlock only the next Module in the same Track. A failed attempt records the score but leaves later Modules locked.
8. **Show progress** — list the learner's Module states in Track order. Authentication and current-user resolution are not implemented yet.

## Core Rules

- The first Module becomes available when enrollment is created.
- A passing score is `score >= Exam.PassThreshold`.
- Passing unlocks only the next Module; it does not unlock the whole Track.
- A failed attempt must not unlock another Module.
- Progress belongs to one learner and one Module. Enforce uniqueness on `(UserId, ModuleId)`.
- The final Module has no next Module to unlock.
- Only `Published` Tracks can be browsed and enrolled in (proposed).
- A learner may retake a failed Exam. A later attempt never reverts `PassedExam` or re-locks a Module that was already unlocked (proposed).
- The score is calculated by the server from the sum of `Points` of correctly answered questions (proposed; see [OPEN_DECISIONS.md](OPEN_DECISIONS.md)).

## Current Code vs. MVP Target

| Area | Current code | MVP target |
|---|---|---|
| User identity | User name/email fields and user service contract | Registration and authentication before protected learner actions |
| Track structure | Track, Module, Lesson entities; Track service list/create implementations | Browse a Track and its ordered content |
| Lesson sources | Lesson has a text Content field | Store content plus explicit book/documentation/video/resource links |
| Enrollment | No Enrollment entity or enrollment service | One enrollment per learner and Track; initialize the first Module's progress |
| Exams | Exam title and threshold; service supports loading an Exam | Questions, answer submission, server-side scoring, and result recording |
| Progression | UserProgress entity and read/service contracts | Per-learner unlock rules and progress queries |
| API | Several placeholder controllers; no ExamsController | Endpoints wired to Application service interfaces |

## Suggested Delivery Order

1. Finish database setup, mappings, repositories, and UnitOfWork.
2. Add Track enrollment and lesson resource contracts.
3. Add Exam questions and server-side answer scoring.
4. Implement UserProgress updates and the pass/fail unlock rule.
5. Add behavior tests for each rule.
6. Wire authentication and API endpoints after the Application flows are testable.

## Explicitly Out of This Learning-Flow MVP

Center scheduling, room booking, attendance, payments, and instructor operations are separate product features. Keep them out of Track enrollment and `UserProgress` until product requirements define how they connect.
