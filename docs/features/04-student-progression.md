# Feature Specification: Student Progression

## 1. MVP Scope & Objective

Track each student's progress through modules. The `UserProgress` entity records which modules a student has unlocked and which exams they have passed — powering the progression lock where passing a module's exam unlocks the next module.

Enrollment is a separate concept and is not specified here. There is no `Enrollment` entity, enrollment service, or enrollment API in the current codebase, and the enrollment workflow has not been defined.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

This specification covers `UserProgress` only; it does not represent or define student enrollment.

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|
| UserProgress | Id | `Guid` | No | PK, inherited from `BaseEntity` |
| UserProgress | CreatedAt | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC |
| UserProgress | UserId | `Guid` | No | User identifier; no FK relationship currently mapped |
| UserProgress | ModuleId | `Guid` | No | Module identifier; no FK relationship currently mapped |
| UserProgress | IsUnlocked | `bool` | No | Whether the student can access this module |
| UserProgress | PassedExam | `bool` | No | Whether the student passed this module's exam |
| UserProgress | Score | `int` | No | The exam score achieved; valid range not defined |

### Relationships

- `UserProgress.UserId` and `UserProgress.ModuleId` are scalar FK-like fields; no navigation properties or explicit EF Core relationships are configured.
- A unique index on `(UserId, ModuleId)` is configured.

### Repository Interface

```csharp
public interface IUserProgressRepository : IRepository<UserProgress> { }
```

### EF Core Notes

- `UserProgressConfiguration` configures the primary key and unique `(UserId, ModuleId)` index.
- No FK behavior or cascade rules are configured.
- No seed data for initial module unlocks.

### Planned Progression Lock Logic

1. The trigger that creates a student's initial progress record and unlocks the first module is not defined; it belongs to a future enrollment workflow.
2. A student reads lessons and takes a module's exam.
3. If `Score >= Exam.PassThreshold`, the student's progress records the passing result.
4. On a pass, the system creates or updates `UserProgress` for the **next** module with `IsUnlocked = true`.
5. Modules remain locked until the preceding module's exam is passed.

> This progression logic is planned. It belongs in `ExamService.SubmitAsync` or a dedicated progression service; enrollment is not represented by `UserProgress`.

## 3. Application Contracts (Shared / Joint Ownership)

### DTOs

| DTO | Type | Fields |
|---|---|---|
| `ProgressResponse` | Output | `Guid ModuleId`, `bool IsUnlocked`, `bool PassedExam`, `int Score` |

> No enrollment request or response DTO is defined. The enrollment mechanism is TBD in a separate feature.

### Service Interface

```csharp
public interface IUserProgressService
{
    Task<IReadOnlyList<ProgressResponse>> GetMyProgressAsync();
}
```

**Implementation status:** `UserProgressService` in `Infrastructure/Services/` throws `NotImplementedException`.

### Planned extensions

```csharp
// Planned additions
Task<IReadOnlyList<ProgressResponse>> GetByTrackAsync(Guid trackId);
```

## 4. API Endpoints Contract (Owned by API Team)

### Endpoints Table

| HTTP Method | Route | Request DTO | Response DTO | Status Codes |
|---|---|---|---|---|
| `GET` | `/api/progress/me` | — | `ProgressResponse[]` | `200 OK`, `401 Unauthorized` |
| `GET` | `/api/progress/me/tracks/{trackId}` | — | `ProgressResponse[]` | `200 OK`, `401 Unauthorized`, `404 Not Found` |

**Current status:** `ProgressController` exposes both routes, and each currently returns `501 Not Implemented`.

### Input Validation

- `GET /api/progress/me`: requires authenticated user identity (not yet configured).
- `GET /api/progress/me/tracks/{trackId}`: `trackId` must reference an existing track.
- No validation classes exist for progress-related inputs.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| `NotFoundException` | User not found; Track not found when filtering by track; Module not found during unlock |
| `ValidationException` | Attempting to unlock a module when prerequisite exam is not passed (planned) |
| `NotImplementedException` | All current service methods and controller actions (placeholder) |
