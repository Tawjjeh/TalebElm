# Feature Specification: Health Check & Error Handling

## 1. MVP Scope & Objective

Expose a lightweight health endpoint so callers can confirm the API is running. Provide a global exception-handling middleware that catches domain exceptions and maps them to consistent HTTP error responses.

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

### Exception Handling Middleware

`ExceptionHandlingMiddleware` wraps the entire request pipeline and maps exceptions to HTTP responses:

| Exception Type | HTTP Status | Response Body |
|---|---|---|
| `NotFoundException` | `404` | `{ "error": "<message>" }` |
| `ValidationException` | `400` | `{ "error": "<message>" }` |
| `DomainException` | `400` | `{ "error": "<message>" }` |
| `NotImplementedException` (Domain) | `501` | `{ "error": "<message>" }` |
| Unhandled `Exception` | `500` | `{ "error": "An unexpected error occurred." }` |

**Current status:** `ExceptionHandlingMiddleware` exists with an empty `InvokeAsync` body and is not registered in `Program.cs`.

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
