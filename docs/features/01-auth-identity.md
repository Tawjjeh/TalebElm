# Feature Specification: Auth & User Identity

## 1. MVP Scope & Objective

Create a learner account and identify the learner on later requests. The User model and service contract exist, but registration, password handling, login, tokens, and student roles are not implemented.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

| Entity | Field | Type | Nullable | Constraints |
|---|---|---|---|---|Rows 1–14 all needed changes; none were already correct.

What was built
Domain: reworked RefreshToken, new OtpRequest/OtpPurpose, Stage, AppLanguage, GradeLevel (D11 ranges), Student aggregate (CreateForAccount, CreateManually for the other developer, UpdateProfile, CompleteOnboarding, Anonymize — D1), UserSettings. All with stable error codes.
Application: Register (atomic: account + Student profile + default settings in one transaction), Login (distinct errors, lockout, PasswordNotSet for password-less accounts), Refresh (rotation + reuse detection), Logout (idempotent, caller's id from claims), ForgotPassword/VerifyOtp/ResetPassword (60 s OTP, 30 s cooldown, 5 attempts, single-use, HMAC-SHA256 + constant-time compare, 10-min single-use reset token, revokes all sessions, clears lockout, works for any role and password-less accounts), DeleteAccount (students only, anonymizes account+profile, removes settings, revokes sessions — D1/D23), GetCurrentUser; Students (profile GET/PUT with name/phone sync to account, onboarding) and Settings (every role, D17); internal idempotent EnsureProfile that self-heals Student profiles and creates settings for any role; shared PhoneNumbers (+20 normalization), PasswordRules, FullNames helpers.
Infrastructure: AppUser (Identity user store only, role as column), IdentityService, TokenProvider (JWT with sub/email/name/role/jti; 64-byte base64url refresh tokens stored as SHA-256), OtpService, LoggingEmailSender, validated JwtOptions/OtpOptions (ValidateOnStart), EF configurations (unique filtered index on Students.UserId, unique token-hash index, cascade/restrict FKs per spec) and migration AddIdentityAndStudent (9 tables, no role tables).
API: all 14 endpoints in section 6.1, central AuthorizationPolicies (Admin/Teacher/Assistant/Student/TeacherStaff), auth rate limiting 10 req/min per IP (configurable via RateLimiting:PermitLimit for tests), JWT bearer (30 s skew, role/name claim types, no inbound mapping), Swagger with Bearer scheme, ProblemDetails mapping via the existing ResultExtensions/ApiResponse mechanism.
Configuration & environment variables
Secrets are never committed — Jwt:SigningKey and Otp:HashKey are empty in appsettings.json and were stored via user secrets for development. Environment variables: Jwt__SigningKey, Jwt__Issuer, Jwt__Audience, Jwt__AccessTokenMinutes, Jwt__RefreshTokenDays, Otp__HashKey, Otp__ExpirySeconds, Otp__MaxAttempts, Otp__ResendCooldownSeconds, Otp__ResetTokenLifetimeMinutes, ConnectionStrings__DefaultConnection, RateLimiting__PermitLimit.

Deviations / notes
Tests: created (projects already existed). 75 unit tests pass; 30 integration tests compile but require Docker (SQL Server Testcontainer) to run — the only unverified behavior, including the "registration rolls back when profile creation fails" scenario.
IEmailService/GmailEmailService (pre-existing) kept intact; LoggingEmailSender handles OTP mails. A real email provider is a production blocker for the OTP flow.
AllowedUserNameCharacters was set to the email-safe character set (letters, digits, -._@+) rather than literally empty, because Identity's default validator rejects @ otherwise; empty string would require bypassing validation entirely.
Rate limiting made configurable (RateLimiting:PermitLimit, default 10) solely so the integration suite can lift it; production default is unchanged.
Teacher/Assistant/Admin work (profiles, admin seeder, teacher_id claim, CORS, account-created email, audit log) was not touched; the shared contracts it needs are in place (IIdentityService.CreateAsync with optional password, password reset for password-less accounts, transaction capability, UserSettings + EnsureProfile, Student.CreateManually without any endpoint).
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

### Role model and authorization policies (proposed)

Two roles exist in the target MVP: `Learner` (default after self-registration)
and `Admin` (content author). Roles should be stored on the user and surfaced
as role claims in the issued token. Authorization is enforced with named
policies, not ad-hoc checks in controllers:

| Policy | Roles allowed | Applies to |
|---|---|---|
| `LearnerOnly` | `Learner`, `Admin` | `GET /api/tracks`, `GET /api/tracks/{id}`, enrollment, lesson reads, exams, `/api/progress/me*` |
| `AdminOnly` | `Admin` | `POST/PUT/DELETE` on Tracks, Modules, Lessons, Exams, and `GET /api/users` |

Critical user-management operations (`PUT /api/users/{id}`,
`DELETE /api/users/{id}`) must be `AdminOnly`. A learner may read and update
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
- Enforcement uses the `LearnerOnly` and `AdminOnly` policies defined above. Attribute-level `[Authorize(Policy = "...")]` is preferred over manual role checks so the rule lives in one place.
- The current codebase has no roles model yet, so role names and authorization policies are still open design work.

### Input Validation

- No validation rules defined for user creation or login.
- Email format, uniqueness, and password handling are not implemented.
- Passwords must satisfy the password policy above; the raw password must never be logged or returned.
- The `POST /api/users` action currently accepts no request parameter, even though `CreateUserRequest` exists.
- Decide password hashing, token issuance, and the default learner role before implementing registration. Keep credentials out of response DTOs.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| `NotFoundException` | User lookup by ID returns `null` |
| `ValidationException` | Duplicate email or missing required fields (planned) |
| `NotImplementedException` | All current service methods and controller actions (placeholder) |
