# TalebElm: Student Flow and Layer Boundaries

## 1. Target Student Journey

```text
Register / Sign in
  → Browse Tracks
  → Choose a Track and Enroll
  → Open the first Module
  → Study ordered Lessons and their sources
  → Submit answers for the Module exam (server grades)
  → Solve practice problems (Codeforces API verification / GitHub tasks)
  → Record score, exam pass, and problem completion states
  → Unlock only the next Module when BOTH exam and required problems are passed
  → Review progress
  → Deliver Track Capstone project
```

This is the target MVP flow, not a claim that every step currently works. Authentication, the Track enrollment workflow, lesson HTTP endpoints and resource validation, exam questions, trusted grading, practice problem verification, and the dual-gated progression-lock transaction are not implemented yet. User/password persistence, TrackEnrollment and LessonResource persistence, and Track/Module/Lesson read services are implemented.

## 2. Clean Architecture Request Path

```text
HTTP request
  → API Controller
  → Application service interface (Abstraction)
  → Application service implementation (Concrete, via IUnitOfWork)
  → Domain repository interface / UnitOfWork
  → Infrastructure repository
  → AppDbContext
  → SQLite database
```

- **API** binds HTTP input and returns HTTP responses. It should not contain business rules or access repositories directly.
- **Application** defines service contracts, implements all business logic / use cases, and owns DTOs. It receives explicit user IDs, not `HttpContext` or JWT types, and accesses persistence only through `IUnitOfWork` (never a DbContext abstraction).
- **Domain** owns entities, business rules, repository contracts, and domain exceptions.
- **Infrastructure** implements repositories and owns EF Core persistence (`DbContext/`, `Configurations/`, `Repositories/`). It contains no business logic.

## 3. Current Code Paths

- `GET /api/health` returns `healthy` directly from `HealthController`.
- Track list/create behavior exists in `TrackService`, but `TracksController` actions still throw `NotImplementedException`.
- `ExamService.GetByIdAsync` loads an Exam through `IUnitOfWork` and throws `NotFoundException` when missing. Create and Submit are placeholders.
- `UserService` methods and `UserProgressService.GetMyProgressAsync` are placeholders.
- User, Module, Lesson, Exams, and Progress controllers are placeholders; Exams actions throw `NotImplementedException` and Progress currently returns 501.
- `Program.cs` registers persistence, Track/Exam/User/UserProgress services, `AddApplication`, and `ExceptionHandlingMiddleware`. Authentication and authorization are not registered.

## 4. Target Exam & Problem Progression Path

1. The API authenticates the learner and passes the learner ID to the Application service.
2. `ExamService` loads the Exam and its questions through repository contracts.
3. The learner submits selected option IDs. The server compares them with the stored answer key; it does not trust a client-provided score.
4. The service uses `Exam.HasPassed(score)` and updates `UserProgress.Score` and `PassedExam = true`.
5. `ModuleProblemService` verifies external problem solves via Codeforces public API (`user.status`) or `TaskVerificationService` verifies GitHub tasks via GitHub REST API (workflow run success and test/workflow SHA integrity check), setting `UserProgress.CompletedRequiredProblems = true`.
6. Progression check runs inside the same `UnitOfWork` transaction after **either** event (exam submit or problem completion): when **both** `PassedExam` and `CompletedRequiredProblems` are satisfied, the service finds the next Module in the same Track by Order and unlocks that Module for this learner only.
7. `UnitOfWork.SaveChangesAsync()` commits the result and progress changes together atomically.
8. The service returns results; the controller maps domain errors to HTTP responses through the registered middleware. Unique and foreign-key violations are translated into `ConflictException` (409) by `UnitOfWork.SaveChangesAsync`. Field-keyed validation responses are still target work (#321).

Question storage, answer submission, problem verification, enrollment initialization, and unlock behavior are still target design; they must not be described as existing functionality until implemented and tested.
