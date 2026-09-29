# Feature Specification: Exams Management

## 1. MVP Scope & Objective

Allow instructors to attach one exam per Module and let students submit their scores against a pass threshold. Exam management is the prerequisite for the progression-lock system — a student cannot unlock the next module until passing the current module's exam.

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

## 3. Application Contracts (Shared / Joint Ownership)

### DTOs

| DTO | Type | Fields |
|---|---|---|
| `CreateExamRequest` | Input | `Guid ModuleId`, `string Title`, `int PassThreshold` |
| `SubmitExamRequest` | Input | `Guid ExamId`, `int Score` |
| `ExamResponse` | Output | `Guid Id`, `string Title`, `int PassThreshold`, `Guid ModuleId` |
| `ExamResultResponse` | Output | `Guid ExamId`, `bool Passed`, `int Score` |

### Service Interface

```csharp
public interface IExamService
{
    Task<ExamResponse> CreateAsync(CreateExamRequest request);
    Task<ExamResponse> GetByIdAsync(Guid id);
    Task<ExamResultResponse> SubmitAsync(SubmitExamRequest request);
}
```

**Implementation status:** No `ExamService` class exists in `Infrastructure/Services/`. The interface is defined but has no implementation.

## 4. API Endpoints Contract (Owned by API Team)

### Endpoints Table

| HTTP Method | Route | Request DTO | Response DTO | Status Codes |
|---|---|---|---|---|
| `POST` | `/api/exams` | `CreateExamRequest` | `ExamResponse` | `201 Created`, `400 Bad Request` |
| `GET` | `/api/exams/{id}` | — | `ExamResponse` | `200 OK`, `404 Not Found` |
| `POST` | `/api/exams/{id}/submit` | `SubmitExamRequest` | `ExamResultResponse` | `200 OK`, `400 Bad Request`, `404 Not Found` |

> No `ExamsController` exists in the current source tree. These endpoints are planned.

### Input Validation

- `CreateExamRequest`: `Title` must not be empty; `PassThreshold` must be > 0.
- `SubmitExamRequest`: `Score` must be >= 0; `ExamId` must reference an existing exam.
- No validation classes exist for exam DTOs.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| `NotFoundException` | Exam lookup by ID returns `null`; Module referenced by `ModuleId` not found |
| `ValidationException` | `PassThreshold` out of range; `Title` empty; duplicate exam per module (planned) |
| `DomainException` | General exam business rule violations |
