# Feature Specification: Exams Management

## 1. MVP Scope & Objective

Attach an exam to a Module and grade the learner's answers against its pass threshold. A passing result is intended to update that learner's progress and unlock the next Module; question storage and answer grading are not implemented yet.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|
| Exam | Id | `Guid` | No | PK, inherited from `BaseEntity` |
| Exam | CreatedAt | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC |
| Exam | Title | `string` | No | Initialized `string.Empty`; no max length configured |
| Exam | PassThreshold | `int` | No | Minimum score to pass; valid range not defined |
| Exam | ModuleId | `Guid` | No | FK → `Module.Id`; no unique constraint (one exam per module is intended but not enforced) |

### Relationships

- `Exam.ModuleId` → `Module.Id`: many-to-one (intended as one-to-one per module).
- No navigation properties declared.

### Repository Interface

```csharp
public interface IExamRepository : IRepository<Exam> { }
```

### EF Core Notes

- `ExamConfiguration` exists with an empty `Configure` body.
- No FK behavior, unique `ModuleId` constraint, or field-length rules configured.

### Questions and answer keys

No question, option, or answer entity exists. For the MVP, use multiple choice:

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|
| ExamQuestion | Id | Guid | No | Inherited primary key |
| ExamQuestion | ExamId | Guid | No | FK to Exam |
| ExamQuestion | Prompt | string | No | Required question text |
| ExamQuestion | Order | int | No | Display order within Exam |
| ExamQuestion | Points | int | No | Positive points for a correct answer |
| ExamOption | Id | Guid | No | Inherited primary key |
| ExamOption | ExamQuestionId | Guid | No | FK to ExamQuestion |
| ExamOption | Text | string | No | Learner-visible option text |
| ExamOption | IsCorrect | bool | No | Server-only grading data; never return in learner responses |

Proposed rules: each question has at least two options and exactly one correct option. Keep these as Application validation until a database constraint is selected.

### Scoring rule (proposed)

- `Score` = sum of `Points` of the questions the learner answered correctly. An unanswered or wrong question earns 0.
- `PassThreshold` is expressed in the same points and must be between 1 and the exam's total points.
- `Exam.HasPassed(score)` stays `score >= PassThreshold`.
- Retakes are allowed. Only the latest result is stored in `UserProgress`; a later failed retake must not clear `PassedExam`. Attempt history is not part of the MVP.

## 3. Application Contracts (Shared / Joint Ownership)

### DTOs

| DTO | Type | Fields |
|---|---|---|
| `CreateExamRequest` | Input | `Guid ModuleId`, `string Title`, `int PassThreshold`, `CreateExamQuestionRequest[] Questions` |
| `SubmitExamRequest` | Current input | `Guid ExamId`, `int Score` |
| `ExamResponse` | Output | `Guid Id`, `string Title`, `int PassThreshold`, `Guid ModuleId`, `ExamQuestionResponse[] Questions` |
| `ExamResultResponse` | Output | `Guid ExamId`, `bool Passed`, `int Score` |

Add nested collections so POST/GET can describe the exam body:

- `CreateExamQuestionRequest`: prompt, order, points, and `CreateExamOptionRequest[] Options`.
- `CreateExamOptionRequest`: option text and correctness flag.
- `ExamQuestionResponse`: id, prompt, order, points, and learner-safe `ExamOptionResponse[] Options`.
- `ExamOptionResponse`: id and option text only; never return `IsCorrect` to the learner.

### Service Interface

```csharp
public interface IExamService
{
    Task<ExamResponse> CreateAsync(CreateExamRequest request);
    Task<ExamResponse> GetByIdAsync(Guid id);
    Task<ExamResultResponse> SubmitAsync(SubmitExamRequest request);
}
```

**Implementation status:** `ExamService.GetByIdAsync` loads an Exam and throws `NotFoundException` when missing. `CreateAsync` and `SubmitAsync` still throw `System.NotImplementedException`. `Exam.HasPassed(int score)` implements `score >= PassThreshold`.

The current Submit DTO accepts a client-supplied score and cannot provide trusted grading. Before implementing submission, replace it with selected question/option IDs and add an explicit learner ID argument to the service call.

## 4. API Endpoints Contract (Owned by API Team)

### Endpoints Table

| HTTP Method | Route | Request DTO | Response DTO | Status Codes |
|---|---|---|---|---|
| `POST` | `/api/exams` | `CreateExamRequest` | `ExamResponse` | `201 Created`, `400 Bad Request` |
| `GET` | `/api/exams/{id}` | — | `ExamResponse` | `200 OK`, `404 Not Found` |
| `POST` | `/api/exams/{id}/submit` | `SubmitExamRequest` | `ExamResultResponse` | `200 OK`, `400 Bad Request`, `404 Not Found` |

> No `ExamsController` exists in the current source tree. These endpoints are planned.

The target submit request contains selected question/option IDs, not a score. The API takes the learner ID from the authenticated identity and passes it explicitly to Application; it must not come from request JSON. Calculate the score from server-side `IsCorrect` values, store the current Module result, and unlock only the next Module after a pass.

### Input Validation

- `CreateExamRequest`: `Title` must not be empty; `PassThreshold` must be > 0.
- `CreateExamRequest.Questions` must not be empty. Each question needs at least two options and exactly one correct option.
- The referenced Exam must exist. Define a score scale before validating score bounds; the current code does not define one.
- No validation classes exist for exam DTOs.

### Submission persistence decision (planned)

- Do not persist per-question learner answers in the MVP.
- Grade the submission in memory from selected option ids and server-side answer keys.
- Persist only the aggregate module outcome already represented by `UserProgress`: latest `Score`, `PassedExam`, and unlocked-next-module effect.
- If review history or analytics become requirements later, add dedicated `ExamAttempt` and `StudentAnswer` entities instead of overloading `UserProgress`.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| `NotFoundException` | Exam lookup by ID returns `null`; Module referenced by `ModuleId` not found |
| `ValidationException` | `PassThreshold` out of range; `Title` empty; duplicate exam per module (planned) |
| `DomainException` | General exam business rule violations |
