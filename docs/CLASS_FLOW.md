# TalebElm: Student Flow and Layer Boundaries

## 1. Target Student Journey

```text
Register / Sign in
  → Browse Tracks
  → Choose a Track and Enroll
  → Open the first Module
  → Study ordered Lessons and their sources
  → Submit answers for the Module exam
  → Record score and pass state
  → Unlock only the next Module after a pass
  → Review progress
```

This is the target MVP flow, not a claim that every step currently works. Authentication, the Track enrollment workflow, lesson HTTP endpoints and resource validation, exam questions, trusted grading, and the progression-lock transaction are not implemented yet. User/password persistence, TrackEnrollment and LessonResource persistence, and Track/Module/Lesson read services are implemented.

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

## 4. Target Exam Submission Path

1. The API authenticates the learner and passes the learner ID to the Application service.
2. `ExamService` loads the Exam and its questions through repository contracts.
3. The learner submits selected option IDs. The server compares them with the stored answer key; it does not trust a client-provided score.
4. The service uses `Exam.HasPassed(score)` and updates the current UserProgress.
5. On a pass, it finds the next Module in the same Track by Order and unlocks that Module for this learner only.
6. `UnitOfWork.SaveChangesAsync()` commits the result and progress changes together.
7. The service returns an `ExamResultResponse`; the controller maps domain errors to HTTP responses through the registered middleware. Unique and foreign-key violations are translated into `ConflictException` (409) by `UnitOfWork.SaveChangesAsync`. Field-keyed validation responses are still target work (#321).

Question storage, answer submission, enrollment initialization, and unlock behavior are still target design; they must not be described as existing functionality until implemented and tested.
