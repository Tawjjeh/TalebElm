# Open Decisions

Items to settle before the related code is written. "Proposed" is what the docs currently assume; change the docs together with the decision.

| # | Decision | Proposed default | Affects |
|---|---|---|---|
| 1 | Authentication method | Email + password with hashing, JWT bearer tokens | [01](features/01-auth-identity.md), [API](API_ENDPOINTS.md) |
| 2 | Roles | `Learner` (default) and `Admin`; stored on the user | [01](features/01-auth-identity.md), [API](API_ENDPOINTS.md) |
| 3 | Score scale | Points: sum of `Points` of correct answers; `PassThreshold` in points | [04](features/04-exams-management.md) |
| 4 | Exam retakes | Unlimited attempts, no cooldown in MVP; latest score kept; a pass is never reverted | [04](features/04-exams-management.md), [05](features/05-student-enrollment-and-progression.md) |
| 5 | Attempt history | Not in MVP; only the latest result in `UserProgress` | [DATABASE_ERD](DATABASE_ERD.md) |
| 6 | Exams per Module | At most one, enforced with a unique `ModuleId` index | [04](features/04-exams-management.md) |
| 7 | Question format | Multiple choice with exactly one correct option | [04](features/04-exams-management.md) |
| 8 | Track visibility | Only `Published` Tracks are browsable and enrollable | [02](features/02-tracks-content.md) |
| 9 | Re-enrollment | Duplicate enrollment returns `409`; no un-enroll in MVP | [05](features/05-student-enrollment-and-progression.md) |
| 10 | Content added after enrollment | A new Module is locked until the previous Module is passed; confirm the rule for Modules inserted in the middle | [05](features/05-student-enrollment-and-progression.md) |
| 11 | Delete rules | Block deleting a Track/Module/Lesson that has dependent data, or define cascade explicitly | [DATABASE_ERD](DATABASE_ERD.md) |
| 12 | Unique `Order` | Enforce unique `(TrackId, Order)` for Modules and `(ModuleId, Order)` for Lessons | [02](features/02-tracks-content.md) |
| 13 | `LessonType` | Decide whether to store it on `Lesson` or drop the enum | [02](features/02-tracks-content.md), [03](features/03-lessons-and-learning-resources.md) |
| 14 | Centers and Rooms | Not part of the MVP; decide whether they are needed at all | [06](features/06-center-management.md), [07](features/07-room-management.md) |
| 15 | Password policy | Min 8 chars, at least one letter and one digit; hashed with a salted slow algorithm; never stored or returned raw | [01](features/01-auth-identity.md) |
| 16 | Field max lengths | Track.Name 150, Track.Description 1000, Module.Title 150, Module.Summary 500, Lesson.Title 150, LessonResource.Title 200 | [02](features/02-tracks-content.md), [03](features/03-lessons-and-learning-resources.md) |
| 17 | Lesson resource URL | Must be absolute `http`/`https` when present; reject relative paths and unsupported schemes with `400` | [03](features/03-lessons-and-learning-resources.md) |
| 18 | Authorization policies | Named `LearnerOnly` and `AdminOnly` policies; critical user management is `AdminOnly` | [01](features/01-auth-identity.md) |
