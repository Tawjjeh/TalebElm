# Feature Specification: Automated GitHub Task Verification

> **Status discipline:** All entities, contracts, and endpoints in this specification are marked **Target** (decided for implementation in Phase 3). None are currently Implemented in code.
> Adheres strictly to the **Zero Manual Gating** principle: all verification is executed automatically by the server.

---

## 1. Scope & Objective

While algorithmic exercises in fundamentals modules are solved on competitive programming platforms like Codeforces, real-world framework engineering (ASP.NET Core, EF Core, Spring Boot) requires writing full-fledged software applications.

Key objectives:
- **CodeCrafters-Style Automated Verification:** Each practical task is defined by an official template repository (`Tawjjeh/task-<slug>-template`) containing a test suite and a GitHub Actions workflow.
- **Zero Manual Gating:** Learners generate a public repository from the template, implement their solution, and push to GitHub. The platform verifies completion automatically via the GitHub REST API. No human review or manual checklist is required to unlock progression.
- **Cryptographic Test Integrity:** The server compares the Git blob SHA of `.github/workflows/test.yml` and the Git tree SHA of `tests/` against the official `TaskVersion` to ensure test assertions were not altered or disabled.
- **Identity Security:** Learner links their GitHub account via OAuth with read-only identity scope. The platform stores the immutable numeric `GitHubUserId` (not just mutable username) to prevent account spoofing. Temporary OAuth access tokens are discarded immediately after identity resolution.

---

## 2. Database & Domain Contract

### 2.1 Entity: GitHubTask (Target)

Defines a practical engineering task attached to a Module or Capstone Milestone.

| Field | Type | Nullable | Constraints / Notes | Status |
|---|---|---|---|---|
| `Id` | `Guid` | No | PK, inherited from `BaseEntity` | Target |
| `CreatedAt` | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC | Target |
| `Title` | `string` | No | Display title (e.g., "Build an In-Memory Key-Value Store"). Max length **150**. | Target |
| `Description` | `string` | No | Task goals, instructions, and concepts exercised. Max length **4000**. | Target |
| `TemplateRepoFullName` | `string` | No | Official template repo (e.g., `Tawjjeh/task-key-value-store-template`). Max length **150**. | Target |
| `CurrentVersionId` | `Guid` | No | FK to active `TaskVersion.Id`. | Target |

### 2.2 Entity: TaskVersion (Target)

Stores cryptographic SHAs and metadata for a specific release tag of the template repository.

| Field | Type | Nullable | Constraints / Notes | Status |
|---|---|---|---|---|
| `Id` | `Guid` | No | PK, inherited from `BaseEntity` | Target |
| `CreatedAt` | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC | Target |
| `TaskId` | `Guid` | No | Required FK to `GitHubTask.Id`. Delete behavior: `Restrict`. | Target |
| `VersionNumber` | `int` | No | Incremental version number (1, 2, 3...). `UNIQUE(TaskId, VersionNumber)`. | Target |
| `OfficialCommitSha` | `string` | No | 40-character Git commit hash of the template release. | Target |
| `WorkflowBlobSha` | `string` | No | 40-character Git blob hash of `.github/workflows/test.yml`. | Target |
| `TestsTreeSha` | `string` | No | 40-character Git tree hash of the `tests/` directory. | Target |
| `IsActive` | `bool` | No | Indicates the recommended version for new submissions. | Target |

### 2.3 Entity: UserExternalAccount (GitHub Linkage) (Target)

Stores the verified connection to the learner's GitHub account.

| Field | Type | Nullable | Constraints / Notes | Status |
|---|---|---|---|---|
| `Id` | `Guid` | No | PK, inherited from `BaseEntity` | Target |
| `CreatedAt` | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC | Target |
| `UserId` | `Guid` | No | Required FK to `User.Id`. Delete behavior: `Restrict`. `UNIQUE(UserId, Platform)`. | Target |
| `Platform` | `ExternalPlatform` | No | Enum: `Codeforces = 1`, `GitHub = 2`. | Target |
| `Handle` | `string` | No | GitHub username/login (e.g., `octocat`). Max length **64**. | Target |
| `ExternalUserId` | `long?` | Yes | Immutable numeric GitHub user ID (e.g., `583231`). Required when `Platform == GitHub`. `UNIQUE(Platform, ExternalUserId)`. | Target |
| `VerificationStatus` | `AccountVerificationStatus` | No | Enum: `Verified = 2` (set automatically via OAuth callback). | Target |
| `VerifiedAt` | `DateTimeOffset?` | Yes | Timestamp of OAuth verification. | Target |

### 2.4 Entity: UserProblemCompletion (Unified Model) (Target)

To avoid schema duplication between Codeforces algorithmic problems and GitHub tasks, `UserProblemCompletion` serves as the unified completion entity across both platforms:

| Field | Type | Nullable | Constraints / Notes | Status |
|---|---|---|---|---|
| `Id` | `Guid` | No | PK, inherited from `BaseEntity` | Target |
| `CreatedAt` | `DateTimeOffset` | No | Inherited from `BaseEntity`, UTC | Target |
| `UserId` | `Guid` | No | Required FK to `User.Id`. Delete behavior: `Restrict`. | Target |
| `ModuleProblemId` | `Guid` | No | Required FK to `ModuleProblem.Id`. Delete behavior: `Restrict`. `UNIQUE(UserId, ModuleProblemId)`. | Target |
| `CompletedAt` | `DateTimeOffset` | No | Verification timestamp. | Target |
| `ExternalSubmissionId` | `string?` | Yes | Codeforces submission ID OR GitHub repository full name (`owner/repo`). Max length **150**. | Target |
| `VerifiedHeadCommitSha` | `string?` | Yes | Git commit SHA at which the test suite passed and SHAs matched. | Target |
| `TaskVersionId` | `Guid?` | Yes | FK to `TaskVersion.Id` evaluated during verification. | Target |

---

## 3. Step-by-Step Verification Algorithm

When a learner triggers `POST /api/tasks/{taskId}/verify` with their repository name:

1. **Verify Identity Linkage:**
   - Resolve authenticated `UserId`.
   - Retrieve `UserExternalAccount` where `Platform == GitHub` and `VerificationStatus == Verified`. If absent, return `400 Bad Request` ("Link your GitHub account first").
2. **Fetch Repository Metadata:**
   - Call GitHub REST API: `GET /repos/{owner}/{repo}` via `IGitHubClient`.
   - Ensure repository exists and is public (`private == false`).
   - Validate ownership: `response.owner.id` must strictly match the stored numeric `ExternalUserId`.
   - Validate provenance: `response.template_repository.full_name` must equal `TaskVersion.TemplateRepoFullName`.
3. **Verify CI Workflow Run:**
   - Call GitHub REST API: `GET /repos/{owner}/{repo}/actions/runs?branch={default_branch}&event=push`.
   - Locate the latest run for the workflow corresponding to `.github/workflows/test.yml`.
   - Assert `run.status == "completed"` and `run.conclusion == "success"`.
   - Extract `head_commit_sha = run.head_sha`.
4. **Assert Cryptographic File Integrity:**
   - Call GitHub REST API: `GET /repos/{owner}/{repo}/git/trees/{head_commit_sha}?recursive=1`.
   - Locate tree entry for `.github/workflows/test.yml`: ensure `entry.sha == TaskVersion.WorkflowBlobSha`.
   - Locate tree entry for `tests/`: ensure `entry.sha == TaskVersion.TestsTreeSha`.
   - If either SHA differs, reject with `400 Bad Request` ("Integrity check failed: tests or workflow files have been modified").
5. **Record Completion & Atomic Unlock:**
   - In a single `UnitOfWork` transaction:
     - Insert `UserProblemCompletion` record.
     - Evaluate the dual-gated progression invariant for the Module.
     - If both exam and required problems are now complete, set next module `IsUnlocked = true`.
     - Commit changes.

---

## 4. Failure Modes & Edge Cases

| Failure Mode | HTTP Status | Error Detail | Handling / Learner Action |
|---|---|---|---|
| **GitHub Account Not Linked** | `400 Bad Request` | `GitHubAccountNotLinked` | Learner must authenticate via GitHub OAuth first. |
| **Repository Not Found / Private** | `400 Bad Request` | `RepositoryNotFoundOrPrivate` | Learner must provide a valid public repository name under their account. |
| **Repo Owner Mismatch** | `400 Bad Request` | `RepositoryOwnerMismatch` | Repository belongs to a different GitHub account than the linked user. |
| **Not Created from Official Template** | `400 Bad Request` | `InvalidTemplateRepository` | Learner created a bare repository instead of clicking "Use this template". |
| **CI Workflow Not Run or Failed** | `400 Bad Request` | `WorkflowNotSuccessful` | Learner has failing tests or hasn't pushed commits to the default branch. |
| **Test Suite / Workflow Tampered** | `400 Bad Request` | `IntegrityShaMismatch` | Learner modified tests or CI workflow. Learner must restore official files. |
| **Template Version Outdated** | `400 Bad Request` | `TaskVersionDeprecated` | Task template received a breaking update; learner should rebase on latest template. |
| **GitHub API Rate Limit / Down** | `503 Service Unavailable` | `ExternalServiceUnavailable` | GitHub API throttled or down. No learner state is altered. Safe to retry later. |

---

## 5. Application Contracts & DTOs

### 5.1 DTOs (Target)

```csharp
public record GitHubTaskResponse(
    Guid Id,
    string Title,
    string Description,
    string TemplateRepoFullName,
    int VersionNumber,
    string TemplateUrl);

public record VerifyGitHubTaskRequest(
    string RepositoryName); // e.g. "my-key-value-store"

public record TaskVerificationResultResponse(
    bool Success,
    string Message,
    DateTimeOffset? CompletedAt,
    bool ModuleUnlocked);
```

### 5.2 External Client Interface (Application Layer)

```csharp
public interface IGitHubClient
{
    Task<GitHubUserResponse> GetAuthenticatedUserAsync(string accessToken);
    Task<GitHubRepositoryResponse?> GetRepositoryAsync(string owner, string repo);
    Task<GitHubWorkflowRunResponse?> GetLatestWorkflowRunAsync(string owner, string repo, string defaultBranch);
    Task<GitHubTreeResponse?> GetTreeAsync(string owner, string repo, string commitSha);
}
```

---

## 6. API Endpoints Contract

| HTTP Method | Route | Access | Request | Response | Status Codes |
|---|---|---|---|---|---|
| `GET` | `/api/tasks/{id}` | **Student** | None | `GitHubTaskResponse` | `200 OK`, `401 Unauthorized`, `403 Forbidden`, `404 Not Found` |
| `POST` | `/api/tasks/{id}/verify` | **Student** | `VerifyGitHubTaskRequest` | `TaskVerificationResultResponse` | `200 OK`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden`, `429 Too Many Requests`, `503 Service Unavailable` |
| `POST` | `/api/admin/tasks` | **AdminOnly** | `CreateGitHubTaskRequest` | `GitHubTaskResponse` | `201 Created`, `400 Bad Request`, `401 Unauthorized`, `403 Forbidden` |
| `POST` | `/api/admin/tasks/{id}/versions` | **AdminOnly** | `CreateTaskVersionRequest` | `TaskVersionResponse` | `201 Created`, `400 Bad Request`, `404 Not Found` |

---

## 7. Acceptance Criteria

- [ ] Verification fails with `400 Bad Request` if the repository was not created from the official template.
- [ ] Verification fails with `400 Bad Request` if the workflow blob SHA or tests tree SHA does not match the active `TaskVersion`.
- [ ] Verification fails with `400 Bad Request` if the latest GitHub Actions workflow run failed.
- [ ] Verification succeeds automatically when tests pass on the default branch and SHAs match, requiring zero manual human intervention.
- [ ] Successful task completion triggers module unlock evaluation inside the same `UnitOfWork` transaction.
