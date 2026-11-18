# TalebElm — GitHub Labels Catalog

TalebElm uses a streamlined 15-label catalog organized into types, architectural layers, triage states, resolutions, and specialized boundaries.

## 1. Type labels

| Label | Description | Color |
|---|---|---|
| `bug` | Something is broken, throwing an unexpected error, or failing a test. | `d73a4a` |
| `enhancement` | A new capability, feature, or improvement to existing functionality. | `a2eeef` |
| `documentation` | Additions, corrections, or structure improvements in documentation. | `0075ca` |
| `question` | Inquiries about architecture, design decisions, or implementation clarification. | `d876e3` |

## 2. Layer labels

| Label | Description | Color |
|---|---|---|
| `layer:domain` | Core business models: entities, enums, exceptions, and repository interfaces. | `1D76DB` |
| `layer:application` | Application services, DTOs, FluentValidation validators, and client contracts. | `5319E7` |
| `layer:infrastructure` | EF Core `AppDbContext`, migrations, configurations, repositories, and HTTP clients. | `0E8A16` |
| `layer:api` | Web layer: ASP.NET Core controllers, middleware, Program.cs, and Swagger. | `FB8C00` |
| `layer:tests` | Unit tests, integration tests, DI resolution tests, and test helpers. | `008672` |

## 3. State labels

| Label | Description | Color |
|---|---|---|
| `good first issue` | Well-scoped, beginner-friendly task with clear boundaries and existing examples. | `7057ff` |
| `help wanted` | Open for community contribution; waiting for a contributor to claim. | `008672` |

## 4. Resolution labels

| Label | Description | Color |
|---|---|---|
| `duplicate` | Work or report that is already tracked in an existing issue or pull request. | `cfd3d7` |
| `wontfix` | Proposed work that is out of MVP scope or contradicted by project decisions. | `ffffff` |

## 5. Specialized labels

| Label | Description | Color |
|---|---|---|
| `security` | Authentication, password hashing, token validation, and vulnerability reports. | `b60205` |
| `database` | Schema changes, EF Core configurations, migrations, and database indexing. | `116329` |

---

## Rules for contributors

Every issue or pull request carries at most one type label and one layer label.
State labels apply when open for contributors; `security` and `database` apply only when the change directly touches those boundaries.
