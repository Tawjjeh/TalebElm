# Feature Specification: Auth & User Identity

## 1. MVP Scope & Objective

Create a student account and identify the student on later requests. The `User` model, `UserRole`, password hashing (PBKDF2 via ASP.NET Core Identity), repository, and unique email index exist. Registration, login, token issuance, and authorization policies are not implemented.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|
| User | Id | `Guid` | No | PK, inherited from `BaseEntity`, auto-generated |
| User | CreatedAt | `DateTimeOffset` | No | Inherited from `BaseEntity`, initialized to UTC |
| User | Name | `string` | No | Initialized to `string.Empty`; required, max 100, non-blank check |
| User | Email | `string` | No | Initialized to `string.Empty`; required, max 256, unique `NOCASE` index, non-blank check |
| User | PasswordHash | `string` | No | Required, non-blank check; PBKDF2 hash produced by `IPasswordHasher`, never a raw password |
| User | Role | `UserRole` | No | Defaults to `Student` (1); values `Student`/`Instructor`/`Admin` |
| User | JoinedAt | `DateTime` | No | No database default configured; registration sets server UTC (target) |

`CreatedAt` is the technical row-creation timestamp inherited from `BaseEntity`. `JoinedAt` is the business timestamp for when the student joined the platform; it may match `CreatedAt` for self-registration, but it remains a separate field so imports or backfilled accounts can preserve the actual join date.

`PasswordHash` is required and stores a PBKDF2 hash; a raw password must never be stored, logged, or returned. `Role` is the `UserRole` enum (`Student = 1`, `Instructor = 2`, `Admin = 3`).

### Relationships

- `User` has a `TrackEnrollments` collection; each `TrackEnrollment` has a required `User` FK with `Restrict` delete behavior.
- `UserProgress.UserId` has a required FK to `User.Id` with `Restrict` delete behavior, plus a `UserProgress.User` navigation.

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

- Add `RegisterRequest` with student profile fields plus password/credential input.
- Add `LoginRequest` with email/username plus password.
- Add `AuthTokenResponse` with access token, optional refresh token, expiry, and basic user identity metadata.
- Add `IAuthService` to validate credentials, hash passwords, and issue/revoke tokens.

### Password policy (proposed)

Passwords are never stored or returned in plain text. Registration enforces the
following complexity rules before hashing (see [OPEN_DECISIONS.md](../OPEN_DECISIONS.md)):

| Rule | Requirement |
|---|---|
| Minimum length | 8 characters |
| Maximum length | 128 characters (protects hashing cost) |
| Character mix | At least one letter and at least one digit |
| Buffer | Reject leading/trailing whitespace-only passwords |
| Storage | Hash with a salted, slow algorithm (e.g. ASP.NET Core `PasswordHasher<T>`); never return the hash |

These values are a starting proposal; confirm them before implementation so the
validator and the hash parameters stay in sync.

### Role model and authorization policies

Three roles are defined in `UserRole`: `Student` (default after self-registration), `Instructor` (reserved, no distinct MVP permissions) and `Admin` (content author/operator). `Student` and `Admin` are active in the MVP. "Learner" is an aggregate product term, not a stored role — see [`business/BUSINESS_RULES.md`](../business/BUSINESS_RULES.md#role-taxonomy-decision-d-1). Roles are surfaced
as role claims in the issued token. Authorization is enforced with named
policies, not ad-hoc checks in controllers:

| Policy | Roles allowed | Applies to |
|---|---|---|
| `StudentOnly` | `Student`, `Admin` | `GET /api/tracks`, `GET /api/tracks/{id}`, enrollment, lesson reads, exams, `/api/progress/me*` |
| `AdminOnly` | `Admin` | `POST/PUT/DELETE` on Tracks, Modules, Lessons, Exams, and `GET /api/users` |

#### Admin Account Provisioning (MVP)
Self-registration (`POST /api/auth/register`) only creates accounts with the default `Student` role. The first `Admin` account cannot be self-registered via public endpoints; it is provisioned through database seeding or initial startup scripts (`DbInitializer`/manual database insert with a pre-hashed password).

Critical user-management operations (`PUT /api/users/{id}`,
`DELETE /api/users/{id}`) must be `AdminOnly`. A student may read and update
only their own profile; any attempt to modify another user must return `403`.

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
| `GET` | `/api/users` | — | `UserResponse[]` (planned) | Throws domain `NotImplementedException` |
| `GET` | `/api/users/{id}` | — | `UserResponse` (planned) | Planned: `200`, `404` |
| `POST` | `/api/users` | `CreateUserRequest` (exists) | `UserResponse` (planned) | Throws domain `NotImplementedException`; the action currently takes no body |
| `POST` | `/api/auth/register` | `RegisterRequest` (planned) | `AuthTokenResponse` or `UserResponse` (planned) | Planned: `201`, `400`, `409` |
| `PUT` | `/api/users/{id}` | `UpdateUserRequest` (planned) | `UserResponse` (planned) | Planned: `200`, `400`, `403`, `404` |
| `DELETE` | `/api/users/{id}` | — | — | Planned: `204`, `403`, `404` |

### Authentication and authorization rules

- Password hashing is implemented through `IPasswordHasher`/`PasswordHasher`. Token handling is planned, not implemented; the MVP needs credential validation, access-token issuance, and token lifetime/refresh behavior.
- `GET /api/users/{id}` is a separate read contract from list/create and should throw `NotFoundException` when the user does not exist.
- Critical user-management operations such as `PUT /api/users/{id}` and `DELETE /api/users/{id}` must be role-protected. A student should not update or delete arbitrary users.
- Enforcement uses the `StudentOnly` and `AdminOnly` policies defined above. Attribute-level `[Authorize(Policy = "...")]` is preferred over manual role checks so the rule lives in one place.
- The role model is implemented (`UserRole`, `User.Role`); role claims and authorization policies are still design work.

### Input Validation

- No registration/login endpoint validation exists yet.
- Email uniqueness and non-blank checks are implemented at the database boundary; email format validation and registration/login handling are not implemented.
- Passwords must satisfy the password policy above; the raw password must never be logged or returned.
- The `POST /api/users` action currently accepts no request parameter, even though `CreateUserRequest` exists.
- Password hashing and the default `Student` role are decided; token issuance remains. Keep credentials out of response DTOs.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| `NotFoundException` | User lookup by ID returns `null` |
| `ValidationException` | Missing/invalid name, email, or password fields (planned) |
| `ConflictException` | Duplicate email on registration (planned; unique index already rejects it) |
| `NotImplementedException` | Remaining placeholder auth/user controller actions and service methods |

## Related external accounts and problems
External account lifecycle is specified under Decision 33 and Feature 07.
