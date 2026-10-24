# TalebElm Workflows & State Transitions

> Companion to `BUSINESS_RULES.md` — the rules are normative; this file maps the **processes**.
> Rule IDs (U/C/E/A/P) refer to that file. Implementation maturity per step is in
> `docs/FEATURE_INDEX.md`; the HTTP surface is owned by `docs/API_ENDPOINTS.md`.

## 1. Authentication lifecycle

```mermaid
stateDiagram-v2
    [*] --> Registered: Register(name, email, password)
    Registered --> Active: Login (token + Role claim)
    Active --> Active: Authorized calls
    Active --> Locked: repeated failures
    Locked --> Active: cooldown / admin unlock / reset
    Active --> Removed: DeleteAccount (history preserved, U-7)
```

| Step | Behavior | Rules |
|---|---|---|
| Register | Validate U-1…U-3; duplicate email → `409`; hash with salted slow algorithm; `Role = Student`; `JoinedAt` = server UTC. No token for duplicate, no partial user on failure. | U-1–U-5 |
| Login | Unknown email **or** wrong password → indistinguishable `401`. Success returns token; hash never leaves the server. | U-3, U-6 |
| Authorize | `StudentOnly` (Student; Admin override): browse, enroll, study, exams, own progress. `AdminOnly`: content writes, user management, manual unlock. `Instructor` is defined but has no distinct MVP permissions. | D-1, §5 |

## 2. Track lifecycle (content authoring)

```mermaid
stateDiagram-v2
    [*] --> Draft: Create (Admin)
    Draft --> Published: Publish
    Published --> Archived: Archive
    Archived --> Published: Re-publish
    note right of Published: Reorder needs product approval once learners started
```

| Transition | Guard (all-or-nothing) |
|---|---|
| Create | Admin only. Starts `Draft`; invisible to learners. |
| Publish | Track has ≥ 1 Module; all `Order` values valid and contiguous; lengths within bounds. |
| Archive | Always allowed. Enrollments and progress untouched. |
| Delete | Blocked (`409`) while Modules, enrollments, or progress reference the Track. No `Draft` regression after publishing. |

## 3. Enrollment flow

```mermaid
flowchart TD
    A["Learner chooses Published Track"] --> B{"Already enrolled?"}
    B -- Yes --> C["409 Conflict (create nothing)"]
    B -- No --> D{"Track has Modules?"}
    D -- No --> E["400 Bad Request (create nothing)"]
    D -- Yes --> F["Create enrollment + ONE progress row (lowest-Order Module, IsUnlocked=true)"]
    F --> G["Single UnitOfWork commit"]
```

Rules E-1…E-4. Identity comes from auth, never from the request body.

## 4. Dual-Gated Unlock Check (Atomic UnitOfWork Evaluation)

The unlock check runs after **either** event occurs (submitting an exam OR verifying practice problems) inside a single `UnitOfWork` transaction:

```mermaid
flowchart TD
    E1["Event A: Submit Exam Answers"] --> Grade["Server Grades Exam"]
    Grade --> CheckScore{"Score >= PassThreshold?"}
    CheckScore -- Yes --> SetExam["Set PassedExam = true (Sticky)"]
    CheckScore -- No --> RetainExam["Retain Previous PassedExam Value"]
    
    E2["Event B: Verify Problem Solves"] --> Match["Verify External Solves (CF / GitHub)"]
    Match --> CheckAllProblems{"All IsRequired Problems Complete?"}
    CheckAllProblems -- Yes --> SetProb["Set CompletedRequiredProblems = true (Sticky)"]
    CheckAllProblems -- No --> RetainProb["Retain Previous Value"]
    
    SetExam --> Eval{"Evaluate Dual-Gate Invariant"}
    RetainExam --> Eval
    SetProb --> Eval
    RetainProb --> Eval
    
    Eval --> GateCheck{"PassedExam == true AND CompletedRequiredProblems == true?"}
    GateCheck -- Yes --> Unlock["Set Next Module IsUnlocked = true in Same Transaction"]
    GateCheck -- No --> NoUnlock["Next Module Remains Locked"]
    Unlock --> Commit["Commit UnitOfWork Transaction"]
    NoUnlock --> Commit
```

| Invariant Trigger | Condition | Transactional Action |
|---|---|---|
| Exam Submission | Score ≥ threshold | Sets `PassedExam = true`. If all required problems were already completed, unlocks next Module. |
| Problem Verification | All required problems verified | Sets `CompletedRequiredProblems = true`. If exam was already passed, unlocks next Module. |
| Single Transaction | Atomic commit | Row creation/update of current module and unlock of next module committed in one `UnitOfWork` commit. |

---

## 5. Codeforces Verification Workflows

### 5.1 Handle Ownership Verification via Profile Token

```mermaid
sequenceDiagram
    autonumber
    actor Learner
    participant Platform as TalebElm Platform
    participant CF as Codeforces Public API

    Learner->>Platform: POST /api/users/me/external-accounts/codeforces (Handle)
    Platform->>Platform: Generate 128-bit single-use token (15m expiry)
    Platform-->>Learner: Return Token & Instructions: Paste into Codeforces profile First Name
    Learner->>CF: Save Profile (firstName = Token)
    Learner->>Platform: POST /api/users/me/external-accounts/{id}/verify
    Platform->>CF: GET /api/user.info?handles={handle}
    CF-->>Platform: User JSON (firstName, organization)
    Platform->>Platform: Assert firstName or organization contains Token
    Platform->>Platform: Set VerificationStatus = Verified, clear Token
    Platform-->>Learner: 200 OK (Verified! You may remove the token from your profile)
```

### 5.2 Problem Solve Verification

```mermaid
sequenceDiagram
    autonumber
    actor Learner
    participant Platform as TalebElm Platform
    participant CF as Codeforces Public API

    Learner->>CF: Solve and submit code on Codeforces (verdict == OK)
    Learner->>Platform: POST /api/modules/{moduleId}/problems/verify (60s cooldown)
    Platform->>Platform: Assert cooldown has elapsed (429 if < 60s)
    Platform->>CF: GET /api/user.status?handle={handle}&count=50
    alt CF API Down or Throttled
        CF-->>Platform: Error / 5xx / 429
        Platform-->>Learner: 503 Service Unavailable (External service temporarily unavailable, 0 state changed)
    else CF API OK
        CF-->>Platform: Submissions JSON (verdict, contestId, problem index)
        Platform->>Platform: Match problems with verdict == OK (All-time solves count)
        Platform->>Platform: Insert UserProblemCompletion records
        Platform->>Platform: Evaluate Dual-Gated Unlock (Section 4)
        Platform-->>Learner: 200 OK (Solves verified, ModuleUnlocked: true/false)
    end
```

---

## 6. Automated GitHub Task Verification Workflow (CodeCrafters-Style)

```mermaid
sequenceDiagram
    autonumber
    actor Learner
    participant Platform as TalebElm Platform
    participant GH as GitHub REST API

    Note over Learner,Platform: Prerequisite: OAuth Account Link (Numeric ExternalUserId Stored)
    Learner->>GH: Push commit to default branch of repository created from official template
    GH->>GH: Execute .github/workflows/test.yml (Status: Completed, Conclusion: Success)
    Learner->>Platform: POST /api/tasks/{taskId}/verify (repoName)
    Platform->>GH: GET /repos/{owner}/{repo}
    Platform->>Platform: Assert owner.id == ExternalUserId AND template_repository == TaskVersion.TemplateRepoFullName
    Platform->>GH: GET /repos/{owner}/{repo}/actions/runs?branch=default&event=push
    Platform->>Platform: Assert latest run conclusion == "success" (head_sha)
    Platform->>GH: GET /repos/{owner}/{repo}/git/trees/{head_sha}?recursive=1
    Platform->>Platform: Assert blob SHA of workflow == TaskVersion.WorkflowBlobSha
    Platform->>Platform: Assert tree SHA of tests/ == TaskVersion.TestsTreeSha
    Platform->>Platform: Record UserProblemCompletion & Evaluate Dual-Gated Unlock
    Platform-->>Learner: 200 OK (Task Verified! Dual-gate progression evaluated)
```

---

## 7. Resource Hub 3-Click Discovery & Curation Flow

```mermaid
flowchart TD
    subgraph Discovery["Public Anonymous Discovery (3 Clicks + Search/Pagination)"]
        V1["GET /api/hub/technologies"] -->|"Click 1: Select Language (C#)"| V2["GET /api/hub/technologies/csharp"]
        V2 -->|"Click 2: Select Framework (ASP.NET Core)"| V3["GET /api/hub/technologies/aspnet-core"]
        V3 -->|"Click 3: Select Topic (Dependency Injection)"| V4["GET /api/hub/topics/{id}/resources"]
        V4 --> V5["View Paged Results with Filters (Language, Level, Cost, Title Search)"]
    end

    subgraph HealthChecker["Automated Link Health Background Service"]
        H1["Scheduled Timer Fires Daily"] --> H2["Fetch Active Resources"]
        H2 --> H3["Send HTTP HEAD Probe to Resource URL"]
        H3 -- 2xx/3xx Success --> H4["Reset Failure Count to 0"]
        H3 -- 4xx/5xx/Timeout --> H5["Increment Failure Count"]
        H5 --> H6{"Failure Count >= 3?"}
        H6 -- Yes --> H7["Set Status = Broken (Hidden from Public Lists)"]
        H6 -- No --> H8["Keep Current Status"]
    end
```

---

## 8. Learner Journey End-to-End

```mermaid
flowchart LR
    R["Register / Login"] --> B["Browse Published Tracks"]
    B --> E["Enroll (Module 1 Unlocks)"]
    E --> S["Study Lessons & Resources"]
    S --> X["Pass Exam"]
    S --> P["Solve Automated Problems / Tasks"]
    X --> N["Next Module Unlocks"]
    P --> N
    N --> V["Repeat Across All Modules"]
    V --> CAP["Complete Automated Capstone Milestones"]
    CAP --> DONE["Track Complete"]
```

---

## 9. Failure → Status Map

| Situation | Status | Rule |
|---|---|---|
| Duplicate email / enrollment / exam-per-Module | `409 Conflict`, nothing created | U-2, E-2, A-3 |
| Blank / oversize / bad URL / bad `Order` | `400 Bad Request` field-keyed errors | U-1, C-3, R-3 |
| Unknown id | `404 Not Found` | — |
| Locked content, incomplete prerequisites, cross-learner access | `403 Forbidden` | P-2, P-4, J-2 |
| Missing / invalid / expired token | `401 Unauthorized` | U-6 |
| Delete with dependents | `409 Conflict`, nothing deleted; archive instead | C-6 |
| Codeforces / GitHub API rate limit | `429 Too Many Requests` (per-user throttle) | X-3, Decision 31 |
| External provider downtime / network failure | `503 Service Unavailable` (fail-safe; 0 state changed) | X-6, Decision 31 |
| Tampered GitHub task test suite / workflow SHA mismatch | `400 Bad Request` (`IntegrityShaMismatch`) | Decision 35 |
