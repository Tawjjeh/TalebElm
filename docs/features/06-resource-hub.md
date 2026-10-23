# Feature Specification: Public Resource Hub

> **Status discipline:** All entities, contracts, and endpoints in this specification are marked **Target** (decided for implementation) or **Proposed** (design pending team decision). None are currently Implemented in code.

---

## 1. Scope & Objective

The **Resource Hub** provides a public, high-quality, community-curated index of software engineering learning references. 

Key objectives:
- **Anonymous Public Access:** Anyone can browse and search the Hub without creating an account or logging in (`Target`).
- **Three-Click Rule:** A visitor can reach any topic's resources in $\le$ 3 clicks:
  1. Language / Ecosystem (e.g., C#, Python, Java, JavaScript)
  2. Framework / Technology (e.g., ASP.NET Core, EF Core, Spring Boot, React)
  3. Topic $\to$ Curated Resources (e.g., Dependency Injection, LINQ, Middleware)
- **Multi-Source Support:** Every topic can link to multiple high-quality references with rich metadata (format, language, level, cost).
- **Shared Resource Model:** Hub resources and Lesson references share a single underlying `Resource` entity to avoid duplicating external references across the platform (`Proposed`, see [Open Decision #25](../OPEN_DECISIONS.md)).
- **Curated Quality:** Resources have an approval lifecycle (`Approved`, `PendingApproval`, `Rejected`). In the initial release, only administrators curate resources; community submissions with moderation arrive in a later phase (`Target`).

---

## 2. Database & Domain Contract

### 2.1 Entity: Technology (Target)

Represents a hierarchical node in the classification tree (Language $\to$ Framework $\to$ Topic). Modeled as a self-referencing tree structure.

| Field | Type | Nullable | Constraints / Notes | Status |
|---|---|---|---|---|
| `Id` | `Guid` | No | PK, inherited from `BaseEntity` | Target |
| `CreatedAt` | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC | Target |
| `ParentId` | `Guid?` | Yes | Self-referencing FK to parent `Technology.Id`. Null for top-level languages (e.g., C#, Java). | Target |
| `Name` | `string` | No | Required, max length **100**, non-blank check. | Target |
| `Slug` | `string` | No | Required, max length **100**, URL-safe lowercase identifier (e.g., `csharp`, `aspnet-core`, `dependency-injection`). Unique index on `Slug`. | Target |
| `Description` | `string` | Yes | Max length **500**. Context or summary of this technology/topic. | Target |
| `Kind` | `TechnologyKind` | No | Enum: `Language = 1`, `Framework = 2`, `Topic = 3`, `Tool = 4`. | Target |
| `Order` | `int` | No | Display order among siblings under the same `ParentId`. `UNIQUE(ParentId, Order)`. | Target |

### 2.2 Entity: Resource (Target)

Represents a specific curated learning source attached to a topic node.

| Field | Type | Nullable | Constraints / Notes | Status |
|---|---|---|---|---|
| `Id` | `Guid` | No | PK, inherited from `BaseEntity` | Target |
| `CreatedAt` | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC | Target |
| `TechnologyId` | `Guid` | No | Required FK to `Technology.Id`. Delete behavior: `Restrict`. | Target |
| `Title` | `string` | No | Required, max length **200**, non-blank. | Target |
| `Kind` | `ResourceKind` | No | Enum: `Book = 1`, `Documentation = 2`, `Video = 3`, `Article = 4`, `Repository = 5`, `Course = 6`, `Other = 7`. (Aligns with `LessonResourceKind`). | Target |
| `Url` | `string?` | Yes | External web link. Must be valid absolute `http`/`https` scheme when present. Null only when `Citation` is provided. | Target |
| `Citation` | `string?` | Yes | Bibliographic reference (e.g., book title, edition, author) for non-URL resources. Max length **500**. | Target |
| `Language` | `string` | No | Two-letter ISO language code (e.g., `en`, `ar`). Max length **10**. | Target |
| `Level` | `ResourceLevel` | No | Enum: `Beginner = 1`, `Intermediate = 2`, `Advanced = 3`. | Target |
| `IsPaid` | `bool` | No | `false` = Free, `true` = Paid. | Target |
| `Status` | `ResourceStatus` | No | Enum: `Approved = 1`, `PendingApproval = 2`, `Rejected = 3`. Default for admin creation is `Approved`. | Target |
| `CreatedByUserId` | `Guid?` | Yes | Optional FK to `User.Id` (who submitted it). Null for system seeds. Delete behavior: `SetNull`. | Target |

### 2.3 Shared Use with Lessons & Migration (Proposed)

Today, lessons cite sources via the `LessonResource` entity (`src/TalebElm.Domain/Entities/LessonResource.cs`, `Implemented`), which is directly tied to `LessonId`. 

To prevent data duplication and allow resources curated in the Hub to be linked to lessons (and vice-versa):
- **Proposed Target Design:** `Resource` becomes the single source-of-truth table. A join table `LessonResourceLink(LessonId, ResourceId, Order)` connects lessons to shared resources.
- **Migration Strategy:** Documented in [Open Decision #25](../OPEN_DECISIONS.md). In Phase 4A, the Hub introduces `Technology` and `Resource`. Phase 4B provides a migration script to transition `LessonResource` records into `Resource` entries.

---

## 3. Application Contracts

### 3.1 DTOs (Target)

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
    IReadOnlyList<ResourceResponse> Resources);

public record ResourceResponse(
    Guid Id,
    Guid TechnologyId,
    string Title,
    int Kind,
    string? Url,
    string? Citation,
    string Language,
    int Level,
    bool IsPaid,
    int Status);

public record CreateResourceRequest(
    Guid TechnologyId,
    string Title,
    int Kind,
    string? Url,
    string? Citation,
    string Language,
    int Level,
    bool IsPaid);

public record ResourceFilterParameters(
    string? Language,
    int? Level,
    bool? IsPaid,
    int? Kind);
```

### 3.2 Service Interface (Target)

```csharp
public interface IResourceHubService
{
    // Public anonymous reads
    Task<IReadOnlyList<TechnologySummaryResponse>> GetTopLevelLanguagesAsync();
    Task<IReadOnlyList<TechnologySummaryResponse>> GetChildrenAsync(Guid parentId);
    Task<TechnologyDetailResponse> GetBySlugAsync(string slug, ResourceFilterParameters filters);
    Task<IReadOnlyList<ResourceResponse>> GetResourcesByTopicAsync(Guid topicId, ResourceFilterParameters filters);

    // Admin management
    Task<ResourceResponse> CreateResourceAsync(CreateResourceRequest request, Guid adminUserId);
    Task<ResourceResponse> UpdateResourceStatusAsync(Guid resourceId, int newStatus, Guid adminUserId);
    Task DeleteResourceAsync(Guid resourceId, Guid adminUserId);
}
```

---

## 4. API Endpoints Contract

### 4.1 Endpoints Matrix (Target)

| HTTP Method | Route | Access | Request DTO | Response DTO | Status Codes |
|---|---|---|---|---|---|
| `GET` | `/api/hub/technologies` | **Anonymous** | None | `TechnologySummaryResponse[]` | `200 OK` |
| `GET` | `/api/hub/technologies/{slug}` | **Anonymous** | Query filters | `TechnologyDetailResponse` | `200 OK`, `404 Not Found` |
| `GET` | `/api/hub/topics/{id}/resources` | **Anonymous** | Query filters | `ResourceResponse[]` | `200 OK`, `404 Not Found` |
| `POST` | `/api/hub/resources` | **AdminOnly** | `CreateResourceRequest` | `ResourceResponse` | `201 Created`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden` |
| `PUT` | `/api/hub/resources/{id}/status` | **AdminOnly** | `UpdateResourceStatusRequest` | `ResourceResponse` | `200 OK`, `400 Bad Request`, `404 Not Found` |
| `DELETE` | `/api/hub/resources/{id}` | **AdminOnly** | None | None | `204 No Content`, `404 Not Found`, `409 Conflict` |

### 4.2 Validation Rules

- `Technology.Slug` must be lowercase alphanumeric with hyphens (e.g., `^[a-z0-9-]+$`).
- `Resource.Title` is required, max 200 characters.
- At least one of `Url` or `Citation` must be provided.
- When `Url` is present, it must be an absolute URI with `http` or `https` scheme. Relative URLs and other protocols are rejected with `400 Bad Request`.
- `Language` must match a valid 2-letter ISO language code (e.g., `en`, `ar`).
- Only `Approved` resources are returned on public anonymous endpoints.

---

## 5. Domain Exceptions

| Exception | Scenario | HTTP Status |
|---|---|---|
| `NotFoundException` | Technology slug or topic ID not found; Resource ID not found. | `404 Not Found` |
| `ValidationException` | Invalid URL format, missing both URL and Citation, invalid language code. | `400 Bad Request` |
| `ConflictException` | Duplicate technology slug or duplicate sibling `Order`. Blocked deletion of technology with child nodes. | `409 Conflict` |

---

## 6. Acceptance Criteria

- [ ] Anonymous visitors can list top-level technologies and drill down to resources in $\le$ 3 requests without providing an Authorization header.
- [ ] Filtering by language (`ar`, `en`), level (`Beginner`), and cost (`IsPaid=false`) returns accurate subsets.
- [ ] Admin can create a resource with auto-approved status; unapproved resources are excluded from public reads.
- [ ] Invalid URL formats (e.g. `ftp://`, `javascript:`, relative paths) are rejected with `400 Bad Request`.
- [ ] Deleting a technology with attached resources or child technologies is prevented with `409 Conflict`.
