# Feature Specification: Auth & User Identity

## 1. MVP Scope & Objective

Provide basic user registration and profile storage so the platform can identify who is learning. Authentication (login/token) is a placeholder in the MVP; the structural contracts are defined but not yet implemented.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|
| User | Id | `Guid` | No | PK, inherited from `BaseEntity`, auto-generated |
| User | CreatedAt | `DateTimeOffset` | No | Inherited from `BaseEntity`, initialized to UTC |
| User | Name | `string` | No | Initialized to `string.Empty` |
| User | Email | `string` | No | Initialized to `string.Empty`; no uniqueness or format constraint configured |
| User | JoinedAt | `DateTime` | No | No database default configured |

### Relationships

- `User` has no navigation properties in the current model.
- `User` → `UserProgress`: one-to-many (a user has many progress records). FK defined on `UserProgress.UserId`.

### Repository Interface

```csharp
public interface IUserRepository : IRepository<User> { }
```

## 3. Application Contracts (Shared / Joint Ownership)

### DTOs

| DTO | Type | Fields |
|---|---|---|
| `CreateUserRequest` | Input | `string Name`, `string Email` |
| `UserResponse` | Output | `Guid Id`, `string Name`, `string Email` |

> Login request/response DTOs do not exist yet.

### Service Interface

```csharp
public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync();
    Task<UserResponse> CreateAsync(CreateUserRequest request);
}
```

**Implementation status:** `UserService` in `Infrastructure/Services/` throws `NotImplementedException` from both methods. No authentication service interface exists.

## 4. API Endpoints Contract (Owned by API Team)

### Endpoints Table

| HTTP Method | Route | Request DTO | Response DTO | Status Codes |
|---|---|---|---|---|
| `POST` | `/api/auth/login` | — (none declared) | — (none declared) | Throws `NotImplementedException` |
| `GET` | `/api/users` | — | `UserResponse[]` (planned) | Throws `NotImplementedException` |
| `POST` | `/api/users` | `CreateUserRequest` (planned) | `UserResponse` (planned) | Throws `NotImplementedException` |

### Input Validation

- No validation rules defined for user creation or login.
- Email format, uniqueness, and password handling are not implemented.
- The `POST /api/users` action currently accepts no request parameter, even though `CreateUserRequest` exists.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| `NotFoundException` | User lookup by ID returns `null` |
| `ValidationException` | Duplicate email or missing required fields (planned) |
| `NotImplementedException` | All current service methods and controller actions (placeholder) |
