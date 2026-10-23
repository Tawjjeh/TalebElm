# TalebElm API: Current Routes and MVP Target

This page separates routes currently declared in `TalebElm.Api` from the routes needed to complete the student learning journey. A controller class is not proof that its use case is implemented.

## 1. Routes Declared in the Current Code

| HTTP | Route | Current behavior |
|---|---|---|
| GET | `/api/health` | Returns `200 OK` with `healthy`. |
| POST | `/api/auth/login` | Declared, no request/response DTO, throws domain `NotImplementedException`. |
| GET | `/api/users` | Declared, action has no parameters and throws domain `NotImplementedException`. |
| POST | `/api/users` | Declared, action has no request parameter and throws domain `NotImplementedException`. |
| GET | `/api/tracks` | `TrackService` list exists, but is not wired to the controller; throws domain `NotImplementedException`. |
| POST | `/api/tracks` | `TrackService` create exists, but the action has no request parameter and is not wired; throws domain `NotImplementedException`. |
| GET | `/api/modules` | `ModuleService` read exists; controller is still a parameterless placeholder. |
| GET | `/api/lessons` | `LessonService` reads exist; controller is still a parameterless placeholder. |
| GET | `/api/exams/{id}` | Declared as a placeholder; throws domain `NotImplementedException`. |
| POST | `/api/exams` | Declared as a parameterless placeholder; throws domain `NotImplementedException`. |
| POST | `/api/exams/{id}/submit` | Declared as a placeholder; throws domain `NotImplementedException`. |
| GET | `/api/progress/me` | Returns `501 Not Implemented`. |
| GET | `/api/progress/me/tracks/{trackId}` | Returns `501 Not Implemented`. |

There is no registration endpoint, enrollment endpoint, nested lesson route, or student-facing exam grading in the current API project. Authentication and authorization are not configured in `Program.cs`.

## 2. Target MVP Student Journey

### Identity and Track selection

| HTTP | Route | Request | Response | Target status |
|---|---|---|---|---|
| POST | `/api/auth/register` | Registration request with name, email, password | User/profile response and auth result | 201, 400, 409 |
| POST | `/api/auth/login` | Login request | Token response | 200, 400, 401 |
| GET | `/api/tracks` | None | Published Track summaries | 200 |
| GET | `/api/tracks/{trackId}` | None | Track detail with ordered Modules | 200, 404 |
| POST | `/api/tracks/{trackId}/enrollment` | None; Student comes from auth | Enrollment response | 201, 401, 404, 409 |

`User` now has `PasswordHash`, `Role` (`UserRole`: `Student`/`Instructor`/`Admin`), and a `TrackEnrollments` collection; password hashing is implemented. Registration, login, token issuance, and authorization policies are still not implemented. `TrackEnrollment` persistence exists separately from `UserProgress`; the enrollment endpoint is not wired.

### Lessons and learning sources

| HTTP | Route | Request | Response | Target status |
|---|---|---|---|---|
| GET | `/api/modules/{moduleId}/lessons` | None | Ordered Lesson summaries | 200, 403, 404 |
| GET | `/api/lessons/{lessonId}` | None | Lesson detail and ordered source links | 200, 403, 404 |

`Lesson` stores `Content`, `Order`, and `ModuleId`. `LessonResource` (title, kind, URL/citation, order) now exists in Domain/Infrastructure with a repository and migration; the lesson detail service loads them. Lesson HTTP endpoints remain placeholders.

### Exams and progression

| HTTP | Route | Request | Response | Target status |
|---|---|---|---|---|
| GET | `/api/exams/{examId}` | None | Exam with Student-safe question/option DTOs | 200, 403, 404 |
| POST | `/api/exams/{examId}/submit` | Selected question/option IDs; Student from auth | Exam result and pass state | 200, 400, 401, 403, 404 |
| GET | `/api/progress/me` | None | Progress summaries for authenticated Student | 200, 401 |
| GET | `/api/progress/me/tracks/{trackId}` | None | Ordered progress for one Track | 200, 401, 404 |

The current `SubmitExamRequest` accepts a client-supplied score. Do not use that value for trusted grading. The target request must submit answers so the server can compare them with a server-side answer key, calculate the score, record `UserProgress`, and unlock only the next Module on a pass.

### Practice problems & external verification

| HTTP | Route | Request | Response | Target status |
|---|---|---|---|---|
| GET | `/api/modules/{moduleId}/problems` | None | Ordered module problems list | 200, 401, 403, 404 |
| POST | `/api/users/me/external-accounts/codeforces` | Link request (handle) | Account details with challenge instructions | 201, 400, 401, 409 |
| POST | `/api/users/me/external-accounts/codeforces/verify` | None | Verified account details | 200, 400, 401, 404, 503 |
| GET | `/api/auth/github` | None | Redirect to GitHub OAuth authorize URL | 302 |
| GET | `/api/auth/github/callback` | OAuth code & state | Identity linked confirmation | 200, 400, 401, 503 |
| POST | `/api/modules/{moduleId}/problems/verify` | None | Solves summary & module completion state | 200, 400, 401, 403, 429, 503 |
| POST | `/api/tasks/{taskId}/verify` | Verify task request (repoName) | Task completion response & unlock state | 200, 400, 401, 403, 429, 503 |
| POST | `/api/admin/modules/{moduleId}/problems` | Problem creation DTO | Created problem DTO | 201, 400, 401, 403 |
| POST | `/api/admin/tasks` | Task creation DTO | Created task DTO | 201, 400, 401, 403 |

### Public Resource Hub

| HTTP | Route | Request | Response | Target status |
|---|---|---|---|---|
| GET | `/api/hub/technologies` | None | Top-level languages & categories | 200 |
| GET | `/api/hub/technologies/{slug}` | Query filters (language, level, cost) | Technology detail with children & resources | 200, 404 |
| GET | `/api/hub/resources` | Query filters (search, language, level, cost, kind, page, size) | Paginated curated resources | 200 |
| POST | `/api/admin/hub/resources` | Resource creation DTO | Created resource DTO | 201, 400, 401, 403 |
| PUT | `/api/admin/hub/resources/{id}` | Resource update DTO | Updated resource DTO | 200, 400, 401, 403, 404 |
| DELETE | `/api/admin/hub/resources/{id}` | None | None | 204, 401, 403, 404, 409 |

### Capstone projects

| HTTP | Route | Request | Response | Target status |
|---|---|---|---|---|
| GET | `/api/tracks/{trackId}/project` | None | Capstone project & milestones | 200, 401, 403, 404 |
| GET | `/api/tracks/{trackId}/project/milestones` | None | Ordered milestones with completion status | 200, 401, 403, 404 |
| POST | `/api/tracks/{trackId}/project/milestones/{id}/verify` | Verify milestone request (repoName) | Milestone completion & track unlock state | 200, 400, 401, 403, 429, 503 |
| POST | `/api/admin/tracks/{trackId}/project` | Project creation DTO | Created capstone project DTO | 201, 400, 401, 403 |
| POST | `/api/admin/projects/{id}/milestones` | Milestone creation DTO | Created milestone DTO | 201, 400, 401, 403 |

### Who may call (proposed)

The role model uses `UserRole` (`Student = 1`, `Instructor = 2`, `Admin = 3`). `Student` (the learner, default after registration) and `Admin` (content author/operator) are active in the MVP; `Instructor` is defined but reserved. "Learner" is an aggregate product term, not a stored role — see [`business/BUSINESS_RULES.md`](business/BUSINESS_RULES.md#role-taxonomy-decision-d-1).

| Access | Routes |
|---|---|
| Anonymous | `GET /api/health`, `POST /api/auth/register`, `POST /api/auth/login`, `GET /api/hub/technologies*`, `GET /api/hub/resources` |
| Student (authenticated) | `GET /api/tracks`, `GET /api/tracks/{trackId}`, `POST /api/tracks/{trackId}/enrollment`, lesson reads, `GET /api/exams/{examId}`, `POST /api/exams/{examId}/submit`, `/api/progress/me*`, `GET /api/modules/{moduleId}/problems`, `/api/users/me/external-accounts*`, `POST /api/modules/{moduleId}/problems/verify`, `POST /api/tasks/{taskId}/verify`, `/api/tracks/{trackId}/project*` |
| Admin | `POST/PUT/DELETE` on Tracks, Modules, Lessons, Exams, Hub resources, module problems, tasks, capstone projects/milestones, and `GET /api/users` |

Student-facing reads should also return a Track's Modules and Lessons only to an enrolled Student once the unlock rule is implemented: a locked Module's Lessons, Exam, and Problems must not be served. When an authenticated Student requests a Lesson, Exam, or Problem set that belongs to a Module the Student has not unlocked, the API must return `403 Forbidden` — not `404` — because the resource exists but the Student is not allowed to access it yet.

## 3. Validation and Response Rules

- Use GUID route identifiers and JSON DTOs.
- Return 400 for invalid input, 401 for missing/invalid authentication, 403 for forbidden actions, 404 for unknown resources, and 409 for duplicate enrollment or exam-per-module conflicts.
- Keep controllers thin: bind HTTP input, call an Application service interface, and translate the result to HTTP. Do not put repository or progression logic in controllers.
- `ExceptionHandlingMiddleware` is registered and maps `NotFoundException` to 404 and FluentValidation failures to 400 as a problem+json `detail` string. Unique and foreign-key violations from the database (a duplicate that raced past a pre-check, or a blocked dependent delete) become `ConflictException` in `UnitOfWork.SaveChangesAsync` and so return 409. Other database failures stay 500. A field-keyed `errors` object is target work (#321). See [08](features/08-health-and-error-handling.md).
- Do not document a planned route as implemented until its controller, service wiring, and behavior tests are present.

## 4. Out of Scope for the Core Learning Flow

Scheduling, attendance, and payments are not part of the current Track → Module → Lesson → Exam → Progress flow.
