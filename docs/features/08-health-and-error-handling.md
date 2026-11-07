# Feature Specification: Health Check & Error Handling

## 1. MVP Scope & Objective

Expose a lightweight health endpoint so callers can confirm the API process responds.

Global exception middleware is registered and maps domain and FluentValidation exceptions to `application/problem+json`.

FluentValidation exceptions return structured field-level validation errors, allowing API clients to associate validation messages with their corresponding request properties.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

- No entities belong to this feature.

### Relationships

- None.

### Domain Exception Hierarchy

This feature consumes the domain exception hierarchy rather than defining entities:

```text
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

| HTTP Method | Route | Request DTO | Response DTO | Status Codes |
|---|---|---|---|---|
| `GET` | `/api/health` | — | `"healthy"` (string) | `200 OK` |

### Health Check Scope

- The current `/api/health` endpoint checks only that the API process can receive a request and return a response.
- It does not currently verify database connectivity, cache availability, message brokers, SMTP providers, object storage, or any third-party identity/auth service.
- That narrow scope matches the current codebase, which has no dependency-specific health probes registered.
- If TalebElm adds infrastructure readiness checks later, start with the primary database and any configured auth/token provider, then expose richer dependency status separately from this lightweight liveness endpoint.

### Exception Handling Middleware

`ExceptionHandlingMiddleware` returns `application/problem+json` with `status`, `title`, and `detail`.

For unhandled HTTP 500 responses, internal exception details are not exposed to clients.

| Exception Type | HTTP Status | Response Title |
|---|---|---|
| `NotFoundException` | `404` | `The requested resource was not found.` |
| Domain `ValidationException` | `400` | `A business rule was violated.` |
| FluentValidation `ValidationException` | `400` | `One or more validation errors occurred.` |
| `ConflictException` | `409` | `A conflicting state or duplicate resource was detected.` (also raised by `UnitOfWork.SaveChangesAsync` for unique and foreign-key violations) |
| `DomainException` | `400` | `The request could not be processed.` |
| `NotImplementedException` (Domain) | `501` | `The requested capability is not implemented yet.` |
| Unhandled `Exception` | `500` | `An unexpected error occurred.` (database failures other than unique/foreign-key violations still fall here) |

### Validation Error Payload

The middleware returns field-specific validation errors for FluentValidation exceptions while preserving the existing Problem Details response envelope.

Example response:

```json
{
  "status": 400,
  "title": "One or more validation errors occurred.",
  "detail": "One or more validation errors occurred.",
  "errors": {
    "name": [
      "'Name' must not be empty."
    ],
    "capacity": [
      "'Capacity' must be greater than '0'."
    ]
  }
}
```

Validation response rules:

- The response uses the `application/problem+json` content type.
- The `status`, `title`, and `detail` properties preserve the existing Problem Details envelope.
- Keys in `errors` use camelCase property names.
- Each field maps to an array of validation messages, allowing multiple failures for the same field.
- Multiple failures for the same property are grouped together, with duplicate messages removed.
- Failures with an empty or whitespace-only `PropertyName` are reported through the top-level `detail` instead of creating an invalid field key.
- If there are no unassigned messages, `detail` uses a generic validation message.
- Domain `TalebElm.Domain.Exceptions.ValidationException` responses retain their existing single-detail behavior and do not include the field-level `errors` object.
- Unhandled HTTP 500 responses must not expose internal exception details.

### Registration (Implemented)

```csharp
// In Program.cs, before app.MapControllers():
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

### Input Validation

- No validation required for the health endpoint.
- FluentValidation handles request-level validation in other API features.
- The global exception middleware converts FluentValidation failures into structured field-level responses.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| `NotFoundException` | User lookup by ID returns null |
| `ConflictException` | Duplicate entity, unique constraint violation, or conflicting state (e.g. duplicate enrollment or exam) |
| `ValidationException` | Invalid request fields, invalid credentials shape, or other business-rule validation |
| `NotImplementedException` | Remaining placeholder controller actions and unimplemented capabilities |

This feature's middleware handles all domain exceptions thrown by other features. See the mapping table above.

Domain validation exceptions remain separate from FluentValidation exceptions:

- Domain `ValidationException` represents business-rule violations and returns a single `detail` message.
- FluentValidation `ValidationException` represents request validation failures and returns an `errors` object grouped by camelCase property names.