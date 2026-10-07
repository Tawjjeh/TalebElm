# TalebElm API: Current Routes and MVP Target

This page separates routes currently declared in `TalebElm.Api` from the routes needed to complete the student learning journey. A controller class is not proof that its use case is implemented.

## 1. Routes Declared in the Current Code

| HTTP | Route | Current behavior |
|---|---|---|
| GET | `/api/health` | Returns `200 OK` with `healthy`. |
| POST | `/api/auth/login` | Declared, no request/response DTO, throws `System.NotImplementedException`. |
| GET | `/api/users` | Declared, action has no parameters and throws `System.NotImplementedException`. |
| POST | `/api/users` | Declared, action has no request parameter and throws `System.NotImplementedException`. |
| GET | `/api/tracks` | Declared, no service is wired to the controller; throws `System.NotImplementedException`. |
| POST | `/api/tracks` | Declared, action has no request parameter; throws `System.NotImplementedException`. |
| GET | `/api/modules` | Declared as a parameterless placeholder; throws `System.NotImplementedException`. |
| GET | `/api/lessons` | Declared as a parameterless placeholder; throws `System.NotImplementedException`. |
| GET | `/api/progress/me` | Returns `501 Not Implemented`. |
| GET | `/api/progress/me/tracks/{trackId}` | Returns `501 Not Implemented`. |

There is no `ExamsController`, registration endpoint, enrollment endpoint, nested lesson route, or student-facing exam route in the current API project. Authentication and authorization are not configured in `Program.cs`.

## 2. Target MVP Student Journey

### Identity and Track selection

| HTTP | Route | Request | Response | Target status |
|---|---|---|---|---|
| POST | `/api/auth/register` | Registration request with name, email, password | User/profile response and auth result | 201, 400, 409 |
| POST | `/api/auth/login` | Login request | Token response | 200, 400, 401 |
| GET | `/api/tracks` | None | Published Track summaries | 200 |
| GET | `/api/tracks/{trackId}` | None | Track detail with ordered Modules | 200, 404 |
| POST | `/api/tracks/{trackId}/enrollment` | None; learner comes from auth | Enrollment response | 201, 401, 404, 409 |

`User` currently has no password, role, or enrollment relationship. The registration/auth contract must be decided before these routes are implemented. `TrackEnrollment` is proposed separately from `UserProgress`.

### Lessons and learning sources

| HTTP | Route | Request | Response | Target status |
|---|---|---|---|---|
| GET | `/api/modules/{moduleId}/lessons` | None | Ordered Lesson summaries | 200, 404 |
| GET | `/api/lessons/{lessonId}` | None | Lesson detail and ordered source links | 200, 404 |

`Lesson` currently stores `Content`, `Order`, and `ModuleId`. Books, official documentation, videos, and other source links need a proposed `LessonResource` contract; there is no resource entity or service today.

### Exams and progression

| HTTP | Route | Request | Response | Target status |
|---|---|---|---|---|
| GET | `/api/exams/{examId}` | None | Exam with learner-safe question/option DTOs | 200, 404 |
| POST | `/api/exams/{examId}/submit` | Selected question/option IDs; learner from auth | Exam result and pass state | 200, 400, 401, 404 |
| GET | `/api/progress/me` | None | Progress summaries for authenticated learner | 200, 401 |
| GET | `/api/progress/me/tracks/{trackId}` | None | Ordered progress for one Track | 200, 401, 404 |

The current `SubmitExamRequest` accepts a client-supplied score. Do not use that value for trusted grading. The target request must submit answers so the server can compare them with a server-side answer key, calculate the score, record `UserProgress`, and unlock only the next Module on a pass.

### Who may call (proposed)

No roles exist in code yet. The MVP needs two: **Learner** (default after registration) and **Admin** (content author).

| Access | Routes |
|---|---|
| Anonymous | `GET /api/health`, `POST /api/auth/register`, `POST /api/auth/login` |
| Learner (authenticated) | `GET /api/tracks`, `GET /api/tracks/{trackId}`, `POST /api/tracks/{trackId}/enrollment`, lesson reads, `GET /api/exams/{examId}`, `POST /api/exams/{examId}/submit`, `/api/progress/me*` |
| Admin | `POST/PUT/DELETE` on Tracks, Modules, Lessons, and Exams, and `GET /api/users` |

Learner-facing reads should also return a Track's Modules and Lessons only to an enrolled learner once the unlock rule is implemented: a locked Module's Lessons and Exam must not be served.

## 3. Validation and Response Rules

- Use GUID route identifiers and JSON DTOs.
- Return 400 for invalid input, 401 for missing/invalid authentication, 403 for forbidden actions, 404 for unknown resources, and 409 for duplicate enrollment or exam-per-module conflicts.
- Keep controllers thin: bind HTTP input, call an Application service interface, and translate the result to HTTP. Do not put repository or progression logic in controllers.
- Map `NotFoundException` to 404 and `ValidationException` to 400 after exception middleware is implemented. The current middleware is empty and is not registered.
- Do not document a planned route as implemented until its controller, service wiring, and behavior tests are present.

## 4. Out of Scope for the Core Learning Flow

Center administration, room management, room booking, attendance, and payments are not part of the current Track → Module → Lesson → Exam → Progress flow. Their API paths should be defined only after the product decides how Centers and Rooms relate to learners and Tracks.
