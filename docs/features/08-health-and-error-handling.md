# Feature Specification: Health Check & Error Handling

## 1. MVP Scope & Objective

Expose a lightweight health endpoint so callers can confirm the API process responds. Global exception middleware is registered and maps domain/FluentValidation exceptions to `application/problem+json`; structured field-level validation errors remain target behavior.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

- No entities belong to this feature.

### Relationships

- None.

### Domain Exception Hierarchy

This feature _consumes_ the domain exception hierarchy rather than defining entities:

```
DomainException (base)
├── NotFoundException
├── ValidationException
├── ConflictException
└── NotImplementedException
```

## 3. Application Contracts (Shared / Joint Ownership)

### DTOs

- No DTOs. The health endpoint returns a plain string `"healthy"`.

### Service Interface

- No service interface. The health check is handled directly by the controller.

## 4. API Endpoints Contract (Owned by API Team)

### Endpoints Table

| HTTP Method | Route         | Request DTO | Response DTO         | Status Codes |
| ----------- | ------------- | ----------- | -------------------- | ------------ |
| `GET`       | `/api/health` | —           | `"healthy"` (string) | `200 OK`     |

### Health check scope

- The current `/api/health` endpoint checks only that the API process can receive a request and return a response.
- It does not currently verify database connectivity, cache availability, message brokers, SMTP providers, object storage, or any third-party identity/auth service.
- That narrow scope matches the current codebase, which has no dependency-specific health probes registered.
- If TalebElm adds infrastructure readiness checks later, start with the primary database and any configured auth/token provider, then expose richer dependency status separately from this lightweight liveness endpoint.

### Exception Handling Middleware

`ExceptionHandlingMiddleware` currently returns `application/problem+json` with `status`, `title`, and `detail` (the detail is suppressed for `500`):

| Exception Type                     | HTTP Status | Current Title                                                        |
| ---------------------------------- | ----------- | -------------------------------------------------------------------- |
| `NotFoundException`                | `404`       | `The requested resource was not found.`                              |
| Domain `ValidationException`       | `400`       | `A business rule was violated.`                                      |
| FluentValidation `ValidationException` | `400`   | `One or more validation errors occurred.`                            |
| `ConflictException`                | `409`       | `A conflicting state or duplicate resource was detected.` (also raised by `UnitOfWork.SaveChangesAsync` for unique and foreign-key violations) |
| `DomainException`                  | `400`       | `The request could not be processed.`                                |
| `NotImplementedException` (Domain) | `501`       | `The requested capability is not implemented yet.`                   |
| Unhandled `Exception`              | `500`       | `An unexpected error occurred.` (database failures other than unique/foreign-key violations still fall here) |

### Validation error payload (target contract)

The current middleware returns a single `detail` string. The target contract adds field-specific errors so clients can attach each message to its input field:

```json
{
  "error": "Validation failed.",
  "errors": {
    "name": ["'Name' must not be empty."],
    "capacity": ["'Capacity' must be greater than '0'."]
  }
}
```

- Keys in `errors` are the failing property names in camelCase; each value is an array of messages, because one field can fail more than one rule.
- The top-level `error` string stays present for logging and generic display; failures that are not tied to a specific field are reported there only.
- Services currently throw FluentValidation's `ValidationException` (for example `TrackService.CreateAsync`), whose `ValidationFailure` items already carry `PropertyName` and `ErrorMessage`. The middleware must map those into the `errors` object above. `TalebElm.Domain.Exceptions.ValidationException` carries a single message and maps to `error` only, until it is extended to carry per-field errors.

### Registration (implemented)

```csharp
// In Program.cs, before app.MapControllers():
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

### Input Validation

- No validation required for the health endpoint.

## 5. Domain Exceptions

| Exception                 | When Thrown                                                                                             |
| ------------------------- | ------------------------------------------------------------------------------------------------------- |
| `NotFoundException`       | User lookup by ID returns null                                                                          |
| `ConflictException`       | Duplicate entity, unique constraint violation, or conflicting state (e.g. duplicate enrollment or exam) |
| `ValidationException`     | Invalid request fields, invalid credentials shape, or other business-rule validation                    |
| `NotImplementedException` | Remaining placeholder controller actions and unimplemented capabilities                                |

This feature's middleware _handles_ all domain exceptions thrown by other features. See the mapping table above.
