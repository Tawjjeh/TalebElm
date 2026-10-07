# Feature Specification: Auth & User Identity

## 1. MVP Scope & Objective

Create a learner account and identify the learner on later requests. The User model and service contract exist, but registration, password handling, login, tokens, and student roles are not implemented.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|
| User | Id | `Guid` | No | PK, inherited from `BaseEntity`, auto-generated |
| User | CreatedAt | `DateTimeOffset` | No | Inherited from `BaseEntity`, initialized to UTC |
| User | Name | `string` | No | Initialized to `string.Empty` |
| User | Email | `string` | No | Initialized to `string.Empty`; no uniqueness or format constraint configured |
| User | JoinedAt | `DateTime` | No | No database default configured |

`CreatedAt` is the technical row-creation timestamp inherited from `BaseEntity`. `JoinedAt` is the business timestamp for when the learner joined the platform; it may match `CreatedAt` for self-registration, but it remains a separate field so imports or backfilled accounts can preserve the actual join date.

There is no `PasswordHash`, `Role`, or `UserRole` property/type in the current model. Never store a raw password on `User`.

### Relationships

- `User` has no navigation properties in the current model.
- `UserProgress.UserId` is intended to reference `User.Id`, but the current EF configuration does not define this foreign key or a navigation property.

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

> The current `CreateUserRequest` contains only Name and Email. Login/register request and token-response DTOs do not exist.

### Planned authentication contracts

- Add `RegisterRequest` with learner profile fields plus password/credential input.
- Add `LoginRequest` with email/username plus password.
- Add `AuthTokenResponse` with access token, optional refresh token, expiry, and basic user identity metadata.
- Add `IAuthService` to validate credentials, hash passwords, and issue/revoke tokens.

### Service Interface

```csharp
public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync();
    Task<UserResponse> CreateAsync(CreateUserRequest request);
}
```

**Implementation status:** `UserService` methods are placeholders. No authentication service interface or identity-provider integration exists.

## 4. API Endpoints Contract (Owned by API Team)

### Endpoints Table

| HTTP Method | Route | Request DTO | Response DTO | Status Codes |
|---|---|---|---|---|
| `POST` | `/api/auth/login` | — (none declared) | — (none declared) | Current action throws `NotImplementedException`; target `200`, `400`, `401` |
| `GET` | `/api/users` | — | `UserResponse[]` (planned) | Throws `NotImplementedException` |
| `GET` | `/api/users/{id}` | — | `UserResponse` (planned) | Planned: `200`, `404` |
| `POST` | `/api/users` | `CreateUserRequest` (planned) | `UserResponse` (planned) | Throws `NotImplementedException` |
| `POST` | `/api/auth/register` | `RegisterRequest` (planned) | `AuthTokenResponse` or `UserResponse` (planned) | Planned: `201`, `400`, `409` |
| `PUT` | `/api/users/{id}` | `UpdateUserRequest` (planned) | `UserResponse` (planned) | Planned: `200`, `400`, `403`, `404` |
| `DELETE` | `/api/users/{id}` | — | — | Planned: `204`, `403`, `404` |

### Authentication and authorization rules

- Token handling is planned, not implemented. The MVP needs a single place to hash passwords, validate credentials, issue access tokens, and define token lifetime/refresh behavior.
- `GET /api/users/{id}` is a separate read contract from list/create and should throw `NotFoundException` when the user does not exist.
- Critical user-management operations such as `PUT /api/users/{id}` and `DELETE /api/users/{id}` must be role-protected. A learner should not update or delete arbitrary users.
- The current codebase has no roles model yet, so role names and authorization policies are still open design work.

### Input Validation

- No validation rules defined for user creation or login.
- Email format, uniqueness, and password handling are not implemented.
- The `POST /api/users` action currently accepts no request parameter, even though `CreateUserRequest` exists.
- Decide password hashing, token issuance, and the default learner role before implementing registration. Keep credentials out of response DTOs.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| `NotFoundException` | User lookup by ID returns `null` |
| `ValidationException` | Duplicate email or missing required fields (planned) |
| `NotImplementedException` | All current service methods and controller actions (placeholder) |
