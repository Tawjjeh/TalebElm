# TalebElm Business Workflows & State Transitions

> Companion to `BUSINESS_RULES.md` (the rules) — this file maps the **processes**.
> Rule IDs (U/T/L/E/A) refer to that file. Feature status per step is in
> `docs/FEATURE_INDEX.md`; `docs/API_ENDPOINTS.md` owns the HTTP surface.

## 1. User authentication lifecycle

```mermaid
stateDiagram-v2
    [*] --> Registered: Register (name, email, password)
    Registered --> Active: Login (JWT issued, Role=Student claim)
    Active --> Active: Refresh / Logout
    Active --> Locked: repeated failed logins
    Locked --> Active: cooldown / admin unlock / password reset
    Active --> Removed: DeleteAccount (history preserved per U-4)
```

| Step | Business behavior |
|---|---|
| Register | Validate (U-1); duplicate email → `409`; hash with salted slow algorithm; default `Role = Student`; `JoinedAt` = server UTC. |
| Login | Unknown email **or** wrong password → `401`, indistinguishable. Issue token with role claim; never return hash. |
| Authorization | `LearnerOnly` (Student, Instructor, Admin): browse, enroll, study, exams, own progress. `AdminOnly`: content writes, user management, manual unlock. |

## 2. Track lifecycle (content authoring)

```mermaid
stateDiagram-v2
    [*] --> Draft: Create (Admin)
    Draft --> Published: Publish (needs ≥1 Module, valid Orders)
    Published --> Archived: Archive (history preserved)
    Archived --> Published: Re-publish
    Published --> Published: Reorder (product approval required once learners started)
```

| Transition | Guard |
|---|---|
| Create | Admin; starts `Draft`, invisible to learners (T-1). |
| Publish | ≥ 1 Module; contiguous valid `Order`; Modules/Lessons within length bounds (T-2, T-3). |
| Archive | Always allowed; enrollments + progress untouched (T-5). |
| Delete | Blocked with `409` while Modules, enrollments, or progress reference the Track (T-5). There is no `Draft` regression after publishing. |

## 3. Enrollment flow

```mermaid
flowchart TD
    A[Learner chooses Published Track] --> B{Already enrolled?}
    B -- Yes --> C[409 Conflict, create nothing]
    B -- No --> D{Track has Modules?}
    D -- No --> E[400 Validation, create nothing]
    D -- Yes --> F[Create enrollment + progress row for lowest-Order Module, IsUnlocked=true]
    F --> G[Commit once via UnitOfWork]
```

Rules E-1…E-4 apply. No request body: Track id from route, User id from auth identity.

## 4. Study → exam → unlock loop (per Module)

```mermaid
stateDiagram-v2
    [*] --> Locked
    Locked --> Unlocked: enroll (first Module) / pass previous / admin unlock
    Unlocked --> Studied: read ordered Lessons + resources
    Studied --> Submitted: submit answers (server grades, A-1)
    Submitted --> Passed: score ≥ threshold → record Score, PassedExam=true, unlock ONLY next Module
    Submitted --> Failed: record Score, PassedExam=false → nothing unlocked, retry allowed
    Passed --> Passed: later failed retake updates Score only (sticky pass, A-3)
```

| Event | Effect |
|---|---|
| Submit (server-graded) | Overwrites `Score`; sets `PassedExam = true` on pass, never clears it later (A-3). |
| Pass | Unlock next Module in same Track (create its progress row if missing). Final Module: nothing further (A-4). |
| Fail | Nothing unlocked; learner may retry immediately (A-3). |
| Locked-module read/submit/view | `403`, not `404` (A-6). |

## 5. Track completion

A Track is complete for a learner when every Module's progress row has `PassedExam = true` (Modules without an Exam complete via explicit unlock per A-2). Completion is derived from progress rows — no separate completion record exists in the MVP.

## 6. Failure & conflict map

| Situation | Result | Rule |
|---|---|---|
| Duplicate email / enrollment / exam-per-Module | `409 Conflict`, nothing created | U-3, E-2, A-2 |
| Invalid input (blank, oversize, bad URL, bad Order) | `400` field-keyed errors | U-1, T-2, L-1/2 |
| Missing id | `404` | — |
| Locked content, cross-learner access, self-unlock attempt | `403` | A-5, A-6 |
| Missing/invalid token | `401` | §1 |
| Delete with dependents | `409`, delete nothing; archive instead | T-5 |
