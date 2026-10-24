# Feature Specification: Public Resource Hub

> **Status discipline:** All entities, contracts, and endpoints in this specification are marked **Target** (decided for implementation in Phase 4). None are currently Implemented in code.

---

## 1. Scope & Objective

The **Resource Hub** is a public, community-curated directory of software engineering learning references:

- **Anonymous Public Access:** Anyone can browse and search the Hub without creating an account or logging in (`Target`).
- **Three-Click Discovery:** A visitor can reach any topic's resources in ≤ 3 clicks:
  1. Language / Ecosystem (e.g., C#, Python, Java, JavaScript)
  2. Framework / Technology (e.g., ASP.NET Core, EF Core, Spring Boot, React)
  3. Topic → Curated Resources (e.g., Dependency Injection, LINQ, Middleware)
- **Shared Resource Model:** One unified `Resource` entity serves both the Resource Hub and Lesson references. `LessonResource` becomes a join entity linking lessons to shared resources (`Target`).
- **Two-Step Non-Destructive Migration:** In Phase 4, `Resource` is added and populated from existing `LessonResource` records. In a subsequent migration, duplicated columns on `LessonResource` are retired.
- **Search & Pagination:** Public read endpoints support pagination, keyword title search, and filtering by language (Arabic, English), difficulty level, format kind, and free vs. paid.
- **Curated Quality & Link Health:**
  - MVP authoring is Admin-only.
  - Future community submissions execute automated safety checks first (HTTPS enforcement, URL normalization, domain blocklist, reachability probe) before entering an admin approval queue.
  - A scheduled background link-checker marks a Resource `Broken` after N consecutive connection failures, hiding it from public lists until repaired.

---

## 2. Database & Domain Contract

### 2.1 Entity: Technology (Target)

Represents a hierarchical node in the classification tree (Language → Framework → Topic). Modeled as a self-referencing tree structure.

| Field | Type | Nullable | Constraints / Notes | Status |
|---|---|---|---|---|
| `Id` | `Guid` | No | PK, inherited from `BaseEntity` | Target |
| `CreatedAt` | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC | Target |
| `ParentId` | `Guid?` | Yes | Self-referencing FK to parent `Technology.Id`. Null for top-level languages (e.g., C#, Java). | Target |
| `Name` | `string` | No | Required, max length **100**, non-blank check. | Target |
| `Slug` | `string` | No | Required, max length **100**, URL-safe lowercase identifier (e.g., `csharp`, `aspnet-core`, `dependency-injection`). `UNIQUE(ParentId, Slug)`. | Target |
| `Description` | `string` | Yes | Max length **500**. Context or summary of this technology/topic. | Target |
| `Kind` | `TechnologyKind` | No | Enum: `Language = 1`, `Framework = 2`, `Topic = 3`, `Tool = 4`. | Target |
| `Order` | `int` | No | Display sequence under the same `ParentId`. `UNIQUE(ParentId, Order)`. | Target |

### 2.2 Entity: Resource (Target)

Represents a specific curated learning source attached to a topic node or cited in lessons.

| Field | Type | Nullable | Constraints / Notes | Status |
|---|---|---|---|---|
| `Id` | `Guid` | No | PK, inherited from `BaseEntity` | Target |
| `CreatedAt` | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC | Target |
| `TechnologyId` | `Guid?` | Yes | Optional FK to `Technology.Id`. Delete behavior: `SetNull`. | Target |
| `Title` | `string` | No | Required, max length **200**, non-blank. | Target |
| `Kind` | `ResourceKind` | No | Enum: `Book = 1`, `Documentation = 2`, `Video = 3`, `Article = 4`, `Repository = 5`, `Course = 6`, `Other = 7`. | Target |
| `Url` | `string?` | Yes | External web link. Must be valid absolute `http`/`https` scheme. Null only when `Citation` is provided. | Target |
| `Citation` | `string?` | Yes | Bibliographic reference (e.g., book title, author, edition) for non-URL resources. Max length **500**. | Target |
| `Language` | `string` | No | Two-letter ISO language code (e.g., `en`, `ar`). Max length **10**. | Target |
| `Level` | `ResourceLevel` | No | Enum: `Beginner = 1`, `Intermediate = 2`, `Advanced = 3`. | Target |
| `IsPaid` | `bool` | No | `false` = Free, `true` = Paid. | Target |
| `Status` | `ResourceStatus` | No | Enum: `Approved = 1`, `PendingApproval = 2`, `Rejected = 3`, `Broken = 4`. | Target |
| `ConsecutiveFailureCount` | `int` | No | Count of failed automated health checks (marks `Broken` at threshold ≥3). Default 0. | Target |
| `LastCheckedAt` | `DateTimeOffset?` | Yes | Timestamp of last link-health check. | Target |
| `CreatedByUserId` | `Guid?` | Yes | Optional FK to `User.Id` (who authored/submitted it). Null for seeds. Delete behavior: `SetNull`. | Target |

### 2.3 Join Entity: LessonResource (Updated to Join Table) (Target)

The existing `LessonResource` entity (`src/TalebElm.Domain/Entities/LessonResource.cs`, `Implemented`) currently holds resource fields directly. In Phase 4, it transitions to a join entity connecting `Lesson` to `Resource`:

| Field | Type | Nullable | Constraints / Notes | Status |
|---|---|---|---|---|
| `Id` | `Guid` | No | PK, inherited from `BaseEntity` | Implemented |
| `CreatedAt` | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC | Implemented |
| `LessonId` | `Guid` | No | Required FK to `Lesson.Id`. Delete behavior: `Restrict`. | Implemented |
| `ResourceId` | `Guid` | No | Required FK to `Resource.Id`. Delete behavior: `Restrict`. | Target (Step 1) |
| `Order` | `int` | No | Sequence of this reference inside the lesson. `UNIQUE(LessonId, Order)`. | Implemented |
| `Note` | `string?` | Yes | Optional lesson-specific guidance (e.g., "Read Chapter 4 only"). Max length **300**. | Target |

#### Two-Step Non-Destructive Migration Plan
1. **Step 1 (Add Resource & Backfill):** Create `Resource` table. Add nullable `ResourceId` to `LessonResource`. Backfill a `Resource` row for every existing `LessonResource` row, setting `LessonResource.ResourceId = Resource.Id`.
2. **Step 2 (Drop Redundant Columns):** In a later release after verifying production data, make `ResourceId` required (`NOT NULL`) and drop `Title`, `Kind`, `Url`, `Citation` columns from `LessonResource`.

---

## 3. Automated Link Checker & Community Moderation

- **Automated Pre-Checks for Community Submissions:**
  - Must use `https://` scheme.
  - Domain must not match a blocklist of URL shorteners, spam, or ad networks.
  - Automated HTTP HEAD request probes reachability (returns 2xx or 3xx).
  - Passed submissions enter the moderation queue with `Status = PendingApproval`.
- **Background Link Checker:**
  - A hosted background service (`ResourceLinkHealthCheckService`) executes periodically (e.g., daily).
  - Probes URLs with an HTTP `HEAD` request.
  - If a resource returns 4xx/5xx or times out 3 consecutive times, update `Status = ResourceStatus.Broken`.
  - Public anonymous queries exclude resources where `Status != ResourceStatus.Approved`.

---

## 4. Application Contracts & DTOs

```csharp
public record TechnologySummaryResponse(
    Guid Id,
    Guid? ParentId,
    string Name,
    string Slug,
    int Kind,
    int Order,
    int ChildCount,
    int ResourceCount);

public record TechnologyDetailResponse(
    Guid Id,
    Guid? ParentId,
    string Name,
    string Slug,
    string? Description,
    int Kind,
    int Order,
    IReadOnlyList<TechnologySummaryResponse> Children,
    PagedResult<ResourceResponse> Resources);

public record ResourceResponse(
    Guid Id,
    Guid? TechnologyId,
    string Title,
    int Kind,
    string? Url,
    string? Citation,
    string Language,
    int Level,
    bool IsPaid,
    int Status);

public record ResourceFilterParameters(
    string? SearchQuery,
    string? Language,
    int? Level,
    bool? IsPaid,
    int? Kind,
    int PageNumber = 1,
    int PageSize = 20);

public record PagedResult<T>(
    IReadOnlyList<T> Items,
    int PageNumber,
    int PageSize,
    int TotalCount,
    int TotalPages);
```

---

## 5. API Endpoints Contract

| HTTP Method | Route | Access | Request | Response | Status Codes |
|---|---|---|---|---|---|
| `GET` | `/api/hub/technologies` | **Anonymous** | None | `TechnologySummaryResponse[]` | `200 OK` |
| `GET` | `/api/hub/technologies/{slug}` | **Anonymous** | Query filters + pagination | `TechnologyDetailResponse` | `200 OK`, `404 Not Found` |
| `GET` | `/api/hub/topics/{id}/resources` | **Anonymous** | Query filters + pagination | `PagedResult<ResourceResponse>` | `200 OK`, `404 Not Found` |
| `POST` | `/api/hub/resources` | **AdminOnly** | `CreateResourceRequest` | `ResourceResponse` | `201 Created`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden` |
| `PUT` | `/api/hub/resources/{id}/status` | **AdminOnly** | `UpdateResourceStatusRequest` | `ResourceResponse` | `200 OK`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found` |
| `DELETE` | `/api/hub/resources/{id}` | **AdminOnly** | None | None | `204 No Content`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found`, `409 Conflict` |

---

## 6. Acceptance Criteria

- [ ] Anonymous public access to all browse, search, and pagination endpoints.
- [ ] Topic navigation reachable in ≤ 3 clicks via self-referencing `Technology` hierarchy.
- [ ] Shared `Resource` model serves both Hub topics and lesson citations.
- [ ] Public reads strictly filter for `Status == ResourceStatus.Approved`.
- [ ] Background link checker flags broken resources and hides them from public views.

## Decision 36 update
Hub routing uses slug paths, for example `GET /api/hub/technologies/csharp/aspnet-core`. Admin routes use `/api/admin/hub/...`: `POST /api/admin/hub/resources`, `PUT /api/admin/hub/resources/{id}`, `PUT /api/admin/hub/resources/{id}/status`, `DELETE /api/admin/hub/resources/{id}`. Link checking uses HEAD, retries GET for 405/403/501, and applies SSRF, redirect, timeout, and per-host spacing rules. Broken resources return to Approved after a successful check.
