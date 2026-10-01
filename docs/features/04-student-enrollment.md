# Feature Specification: Student Enrollment & Progression

## 1. MVP Scope & Objective

Track each student's enrollment and progression through learning tracks. The `UserProgress` entity records which modules a student has unlocked and which exams they have passed — powering the "progression lock" where passing a module's exam automatically unlocks the next module.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|
| UserProgress | Id | `Guid` | No | PK, inherited from `BaseEntity` |
| UserProgress | CreatedAt | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC |
| UserProgress | UserId | `Guid` | No | FK → `User.Id` |
| UserProgress | ModuleId | `Guid` | No | FK → `Module.Id` |
| UserProgress | IsUnlocked | `bool` | No | Whether the student can access this module |
| UserProgress | PassedExam | `bool` | No | Whether the student passed this module's exam |
| UserProgress | Score | `int` | No | The exam score achieved; valid range not defined |

### Relationships

- `UserProgress.UserId` → `User.Id`: many-to-one (a user has many progress records).
- `UserProgress.ModuleId` → `Module.Id`: many-to-one (a module has many progress records).
- Intended unique constraint: one `UserProgress` per `(UserId, ModuleId)` pair — not yet configured.
- No navigation properties declared.

### Repository Interface

```csharp
public interface IUserProgressRepository : IRepository<UserProgress> { }
```

### EF Core Notes

- `UserProgressConfiguration` exists with an empty `Configure` body.
- No FK behavior, unique `(UserId, ModuleId)` index, or cascade rules configured.
- No seed data for initial module unlocks.

### Progression Lock Logic (Business Rule)

1. When a student enrolls in a track, Module 1 gets `IsUnlocked = true`.
2. The student reads lessons and takes Module 1's exam.
3. If `Score >= Exam.PassThreshold` → `PassedExam = true`.
4. On pass, the system creates/updates `UserProgress` for the **next** module with `IsUnlocked = true`.
5. Modules remain locked until the preceding module's exam is passed.

> This logic belongs in `ExamService.SubmitAsync` or a dedicated progression service.

## 3. Application Contracts (Shared / Joint Ownership)

### DTOs

| DTO | Type | Fields |
|---|---|---|
| `ProgressResponse` | Output | `Guid ModuleId`, `bool IsUnlocked`, `bool PassedExam`, `int Score` |

> No enrollment request DTO exists. Track enrollment could be implicit (first access) or explicit (a future `EnrollRequest` DTO).

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

**Current status:** Both actions return `501 Not Implemented` via `StatusCode(StatusCodes.Status501NotImplemented)`.

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
