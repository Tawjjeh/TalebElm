# Feature Specification: Capstone Projects & Track Completion

> **Status discipline:** All entities, contracts, and endpoints in this specification are marked **Target** (decided for implementation) or **Proposed** (design pending team decision). None are currently Implemented in code.

---

## 1. Scope & Objective

At the conclusion of a Track, learners transition from guided modular learning to synthesis: building and delivering a substantial software project.

Key objectives:
- **Real-World Portfolio Piece:** The capstone project requires the learner to architect, implement, test, and document a complete software system demonstrating mastery of the Track's concepts.
- **Architectural Flexibility (Open Decision #27):** Supports two delivery formats:
  1. *Terminal Capstone:* A standalone comprehensive project unlocked after all track modules are completed.
  2. *Cumulative Milestone Project:* A single cohesive system developed incrementally across the Track, where each module completes a discrete milestone.
- **Submission & Mentorship:** Learners submit a public Git repository URL (e.g., GitHub) accompanied by deployment links and architecture notes.
- **Review Lifecycle:** In the MVP, submissions undergo review against a structured checklist (`Submitted`, `UnderReview`, `Approved`, `NeedsRevision`) by an Admin or mentor. Automated testing via CI/CD runners is a future enhancement (`Proposed`).

---

## 2. Database & Domain Contract

### 2.1 Entity: CapstoneProject (Target)

Represents the capstone project definition attached to a Track.

| Field | Type | Nullable | Constraints / Notes | Status |
|---|---|---|---|---|
| `Id` | `Guid` | No | PK, inherited from `BaseEntity` | Target |
| `CreatedAt` | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC | Target |
| `TrackId` | `Guid` | No | Required FK to `Track.Id`. Delete behavior: `Restrict`. `UNIQUE(TrackId)` (at most one capstone per track). | Target |
| `Title` | `string` | No | Required, max length **150**, non-blank. | Target |
| `Description` | `string` | No | Detailed project requirements, user stories, and architectural expectations. Max length **4000**. | Target |
| `RubricUrl` | `string?` | Yes | External link to evaluation rubric or checklist. Max length **500**. | Target |
| `IsCumulative` | `bool` | No | `false` = Terminal project after all modules; `true` = Progressive milestones mapped to modules. Default `false`. | Proposed |

### 2.2 Entity: ProjectMilestone (Target)

Defines an ordered checkpoint or milestone within the capstone project.

| Field | Type | Nullable | Constraints / Notes | Status |
|---|---|---|---|---|
| `Id` | `Guid` | No | PK, inherited from `BaseEntity` | Target |
| `CreatedAt` | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC | Target |
| `ProjectId` | `Guid` | No | Required FK to `CapstoneProject.Id`. Delete behavior: `Restrict`. | Target |
| `ModuleId` | `Guid?` | Yes | Optional link to a specific `Module.Id` if the milestone corresponds to that module's completion. | Proposed |
| `Title` | `string` | No | Required, max length **150**. | Target |
| `Instructions` | `string` | No | Deliverable requirements for this milestone. Max length **2000**. | Target |
| `Order` | `int` | No | Display sequence (1, 2, 3...). `UNIQUE(ProjectId, Order)`. | Target |

### 2.3 Entity: ProjectSubmission (Target)

Stores a learner's project submission and tracks its review lifecycle.

| Field | Type | Nullable | Constraints / Notes | Status |
|---|---|---|---|---|
| `Id` | `Guid` | No | PK, inherited from `BaseEntity` | Target |
| `CreatedAt` | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC | Target |
| `UserId` | `Guid` | No | Required FK to `User.Id`. Delete behavior: `Restrict`. | Target |
| `ProjectId` | `Guid` | No | Required FK to `CapstoneProject.Id`. Delete behavior: `Restrict`. | Target |
| `MilestoneId` | `Guid?` | Yes | Optional FK to specific `ProjectMilestone.Id` if submitted per milestone. | Target |
| `RepositoryUrl` | `string` | No | Public Git repository URL (e.g., `https://github.com/user/repo`). Max length **500**. | Target |
| `DemoUrl` | `string?` | Yes | Optional live deployment URL. Max length **500**. | Target |
| `Notes` | `string?` | Yes | Learner submission notes or architectural highlights. Max length **1000**. | Target |
| `Status` | `SubmissionStatus` | No | Enum: `Submitted = 1`, `UnderReview = 2`, `Approved = 3`, `NeedsRevision = 4`. | Target |
| `ReviewNotes` | `string?` | Yes | Feedback from the reviewer. Max length **2000**. | Target |
| `ReviewedAt` | `DateTimeOffset?` | Yes | Timestamp of review completion. | Target |
| `ReviewedByUserId` | `Guid?` | Yes | Admin/Mentor reviewer FK to `User.Id`. Delete behavior: `SetNull`. | Target |

---

## 3. Application Contracts

### 3.1 DTOs (Target)

```csharp
public record CapstoneProjectResponse(
    Guid Id,
    Guid TrackId,
    string Title,
    string Description,
    string? RubricUrl,
    bool IsCumulative,
    IReadOnlyList<ProjectMilestoneResponse> Milestones);

public record ProjectMilestoneResponse(
    Guid Id,
    Guid ProjectId,
    Guid? ModuleId,
    string Title,
    string Instructions,
    int Order);

public record SubmitProjectRequest(
    Guid ProjectId,
    Guid? MilestoneId,
    string RepositoryUrl,
    string? DemoUrl,
    string? Notes);

public record ProjectSubmissionResponse(
    Guid Id,
    Guid ProjectId,
    Guid? MilestoneId,
    string RepositoryUrl,
    string? DemoUrl,
    string? Notes,
    int Status,
    string? ReviewNotes,
    DateTimeOffset SubmittedAt,
    DateTimeOffset? ReviewedAt);

public record ReviewSubmissionRequest(
    int Status, // Approved or NeedsRevision
    string? ReviewNotes);
```

### 3.2 Service Interface (Target)

```csharp
public interface ICapstoneProjectService
{
    Task<CapstoneProjectResponse> GetProjectByTrackIdAsync(Guid trackId, Guid userId);
    Task<ProjectSubmissionResponse> SubmitProjectAsync(Guid userId, SubmitProjectRequest request);
    Task<IReadOnlyList<ProjectSubmissionResponse>> GetMySubmissionsAsync(Guid trackId, Guid userId);

    // Admin / Mentor review
    Task<IReadOnlyList<ProjectSubmissionResponse>> GetPendingReviewsAsync(Guid adminUserId);
    Task<ProjectSubmissionResponse> ReviewSubmissionAsync(Guid submissionId, ReviewSubmissionRequest request, Guid adminUserId);
}
```

---

## 4. API Endpoints Contract

| HTTP Method | Route | Access | Request | Response | Status Codes |
|---|---|---|---|---|---|
| `GET` | `/api/tracks/{trackId}/project` | **Student** | None | `CapstoneProjectResponse` | `200 OK`, `401 Unauthorized`, `403 Forbidden` (if track modules incomplete), `404 Not Found` |
| `POST` | `/api/tracks/{trackId}/project/submissions` | **Student** | `SubmitProjectRequest` | `ProjectSubmissionResponse` | `201 Created`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden` |
| `GET` | `/api/tracks/{trackId}/project/submissions/me` | **Student** | None | `ProjectSubmissionResponse[]` | `200 OK`, `401 Unauthorized` |
| `GET` | `/api/admin/projects/submissions/pending` | **AdminOnly** | None | `ProjectSubmissionResponse[]` | `200 OK`, `401 Unauthorized`, `403 Forbidden` |
| `PUT` | `/api/admin/projects/submissions/{id}/review` | **AdminOnly** | `ReviewSubmissionRequest` | `ProjectSubmissionResponse` | `200 OK`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found` |

---

## 5. Domain Exceptions

| Exception | Scenario | HTTP Status |
|---|---|---|
| `ValidationException` | Invalid Git repository URL scheme, missing required submission fields. | `400 Bad Request` |
| `DomainException` | Attempting to access or submit a capstone before completing prerequisite track modules (`403 Forbidden`). | `403 Forbidden` |
| `ConflictException` | A submission is already currently `UnderReview` for this milestone. | `409 Conflict` |
| `NotFoundException` | Track, project, milestone, or submission not found. | `404 Not Found` |

---

## 6. Acceptance Criteria

- [ ] A learner cannot access or submit a terminal capstone project until all Track modules are marked both passed and completed (`403 Forbidden`).
- [ ] Submitting a project requires a valid absolute HTTP/HTTPS URL pointing to a Git repository.
- [ ] Once submitted, the submission state moves to `Submitted`.
- [ ] An admin/mentor can approve or request revisions with notes; learner sees updated status.
- [ ] Track completion requires an approved capstone project submission (`Target`).
