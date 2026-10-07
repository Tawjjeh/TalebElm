# Feature Specification: Health Check & Error Handling

## 1. MVP Scope & Objective

Expose a lightweight health endpoint so callers can confirm the API process responds. A global exception middleware is planned, but exception-to-HTTP mapping is not active yet.

## 2. Database & Domain Contract (Owned by DB/Domain Team)

### Entities & Fields

- No entities belong to this feature.

### Relationships

- None.

### Domain Exception Hierarchy

This feature *consumes* the domain exception hierarchy rather than defining entities:

```
DomainException (base)
├── NotFoundException
├── ValidationException
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

### Health check scope

- The current `/api/health` endpoint checks only that the API process can receive a request and return a response.
- It does not currently verify database connectivity, cache availability, message brokers, SMTP providers, object storage, or any third-party identity/auth service.
- That narrow scope matches the current codebase, which has no dependency-specific health probes registered.
- If TalebElm adds infrastructure readiness checks later, start with the primary database and any configured auth/token provider, then expose richer dependency status separately from this lightweight liveness endpoint.

### Exception Handling Middleware

The intended `ExceptionHandlingMiddleware` mapping is:

| Exception Type | HTTP Status | Response Body |
|---|---|---|
| `NotFoundException` | `404` | `{ "error": "<message>" }` |
| `ValidationException` | `400` | `{ "error": "<message>" }` |
| `DomainException` | `400` | `{ "error": "<message>" }` |
| `NotImplementedException` (Domain) | `501` | `{ "error": "<message>" }` |
| Unhandled `Exception` | `500` | `{ "error": "An unexpected error occurred." }` |

**Current status:** `ExceptionHandlingMiddleware.InvokeAsync` is empty and `Program.cs` does not register it. The table above is a target contract, not current runtime behavior.

### Registration (planned)

```csharp
// In Program.cs, before app.MapControllers():
app.UseMiddleware<ExceptionHandlingMiddleware>();
```

### Input Validation

- No validation required for the health endpoint.

## 5. Domain Exceptions

| Exception | When Thrown |
|---|---|
| — | The health endpoint declares no domain exceptions. |

This feature's middleware *handles* all domain exceptions thrown by other features. See the mapping table above.
