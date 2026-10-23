# TalebElm — GitHub Issues Implementation Plan

This document defines the structured, dependency-ordered issue rollout plan for the **TalebElm** platform across all six development roadmap phases.

---

## 1. Methodology & Governance Rules

1. **Strict Zero Manual Gating:**
   - Every learner progression step (exams, practice problems, capstone milestones, track completion) is verified **100% automatically** by the server.
   - Admins author content and moderate the public Resource Hub; humans never review or checklist learner progress.
2. **Layer Discipline & Sizing:**
   - Every issue is scoped to **one architectural layer** (`layer:domain`, `layer:application`, `layer:infrastructure`, `layer:api`, or `layer:tests`).
   - Sized as **small PRs** (1–4 hours of focused work, typically $\le 200$ lines of code plus tests).
3. **No Duplicate Issues:**
   - Pre-existing issues (#326, #274, #324, #322, #321, #325, #323, #248, #278, #279, #280, #289, #244, #246, #273, #250, #254, #255, #256, #208, #268, #266, etc.) are foundational and linked directly.
   - This plan specifies new work for automated problem verification, the public 3-click Resource Hub, cumulative capstone projects, and missing domain/security foundations.
4. **Label Usage:**
   - Issues use **only approved labels** from [`docs/GITHUB_LABELS.md`](GITHUB_LABELS.md).
   - Proposed labels (`feature:resources`, `feature:problems`, `feature:capstone`, `feature:enrollment`) are noted where applicable and await maintainer approval at Gate 2.
5. **Two-Wave Staging:**
   - **First Wave (36 issues):** Foundational tracking issues, domain contracts, test fakes, EF Core mappings/migrations, core verification algorithms, and concurrency tests.
   - **Backlog (20 issues):** Controller wiring, background hosted services, Step 2 migration, and end-to-end integration flows.

---

## 2. Gate 2 Summary Plan Table

| Order | Issue Title | Layer | Labels | Difficulty | Blocked By |
|---|---|---|---|---|---|
| **I-01** | [Tracking] Phase 0: Core Architectural Foundations & Hardening | `layer:infrastructure` | `setup`, `layer:infrastructure`, `enhancement` | Easy | None |
| **I-02** | [Tracking] Phase 1: Identity, Authentication & Role Access | `layer:api` | `setup`, `layer:api`, `feature:auth` | Easy | None |
| **I-03** | [Tracking] Phase 2: Core Learning Flow & Dual-Gated Progression | `layer:application` | `setup`, `layer:application`, `feature:tracks`, `feature:exams` | Easy | None |
| **I-04** | [Tracking] Phase 3: Automated Practice Problems & External Verification | `layer:application` | `setup`, `layer:application`, `feature:exams` | Easy | None |
| **I-05** | Implement OAuth state generation and validation helper for external account linking | `layer:application` | `layer:application`, `interface`, `feature:auth`, `good first issue` | Easy | None |
| **I-06** | Add external identity configuration options with environment secret binding | `layer:infrastructure` | `layer:infrastructure`, `setup`, `feature:auth`, `good first issue` | Easy | None |
| **I-07** | Add ProblemKind enum and ModuleProblem domain entity | `layer:domain` | `layer:domain`, `entity`, `enum`, `feature:exams`, `good first issue` | Very Easy | None |
| **I-08** | Add ExternalPlatform and AccountVerificationStatus enums and UserExternalAccount entity | `layer:domain` | `layer:domain`, `entity`, `enum`, `feature:auth`, `good first issue` | Very Easy | None |
| **I-09** | Add GitHubTask domain entity | `layer:domain` | `layer:domain`, `entity`, `feature:tracks`, `good first issue` | Very Easy | None |
| **I-10** | Add TaskVersion domain entity with Git commit and tree SHAs | `layer:domain` | `layer:domain`, `entity`, `feature:tracks`, `good first issue` | Easy | `I-09` |
| **I-11** | Add UserProblemCompletion unified completion domain entity | `layer:domain` | `layer:domain`, `entity`, `feature:exams`, `good first issue` | Easy | `I-07`, `I-10` |
| **I-12** | Define ICodeforcesClient and IGitHubClient interfaces in Application | `layer:application` | `layer:application`, `interface`, `feature:exams`, `good first issue` | Very Easy | None |
| **I-13** | Create FakeCodeforcesClient and FakeGitHubClient in Tests for offline testing | `layer:tests` | `layer:tests`, `testing`, `feature:exams`, `good first issue` | Easy | `I-12` |
| **I-14** | Configure EF Core mapping and add migration for ModuleProblem | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:exams` | Medium | `I-07` |
| **I-15** | Configure EF Core mapping and add migration for UserExternalAccount | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:auth` | Medium | `I-08` |
| **I-16** | Configure EF Core mapping and add migration for GitHubTask and TaskVersion | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:tracks` | Medium | `I-09`, `I-10` |
| **I-17** | Configure EF Core mapping and add migration for UserProblemCompletion | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:exams` | Medium | `I-11`, `I-14`, `I-16` |
| **I-18** | Implement Codeforces challenge token generation service | `layer:application` | `layer:application`, `feature:exams`, `good first issue` | Easy | `I-08`, `I-12` |
| **I-19** | Implement Codeforces handle profile verification service via user.info | `layer:application` | `layer:application`, `feature:exams` | Medium | `I-12`, `I-15`, `I-18` |
| **I-20** | Implement Codeforces solve matching service via user.status with 60s cooldown | `layer:application` | `layer:application`, `feature:exams` | Medium | `I-12`, `I-14`, `I-17` |
| **I-21** | Implement GitHub repository provenance and CI workflow run verifier | `layer:application` | `layer:application`, `feature:tracks` | Medium | `I-12`, `I-15`, `I-16` |
| **I-22** | Implement Git workflow and test suite SHA integrity-check service | `layer:application` | `layer:application`, `feature:tracks` | Medium | `I-10`, `I-12` |
| **I-23** | Unit test GitHub SHA integrity-checker with recorded Git tree API fixtures | `layer:tests` | `layer:tests`, `testing`, `feature:tracks` | Medium | `I-22` |
| **I-24** | Implement atomic dual-gated progression unlock service across exams and problems | `layer:application` | `layer:application`, `feature:exams` | Hard | `I-17`, `#208` |
| **I-25** | Integration test: Complete last requirement in either order unlocks module exactly once | `layer:tests` | `layer:tests`, `testing`, `feature:exams` | Medium | `I-24` |
| **I-26** | Add per-user verification rate-limiting middleware for external verification endpoints | `layer:api` | `layer:api`, `middleware`, `feature:exams` | Medium | None |
| **I-27** | Tooling: Create example starter template repository task per TASK_TEMPLATE_SPEC | `layer:infrastructure` | `setup`, `documentation`, `good first issue` | Easy | None |
| **I-28** | Add TechnologyKind enum and Technology self-referencing tree domain entity | `layer:domain` | `layer:domain`, `entity`, `enum`, `feature:tracks`, `good first issue` | Very Easy | None |
| **I-29** | Add ResourceKind, ResourceLevel, ResourceStatus enums and Resource domain entity | `layer:domain` | `layer:domain`, `entity`, `enum`, `feature:tracks`, `good first issue` | Very Easy | `I-28` |
| **I-30** | Configure EF Core mapping and add migration for Technology and Resource | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:tracks` | Medium | `I-28`, `I-29` |
| **I-31** | Migration Step 1: Add ResourceId FK to LessonResource and backfill existing rows | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:tracks` | Medium | `I-30` |
| **I-32** | Add CapstoneProject domain entity | `layer:domain` | `layer:domain`, `entity`, `feature:tracks`, `good first issue` | Very Easy | None |
| **I-33** | Add ProjectMilestone domain entity referencing GitHubTask | `layer:domain` | `layer:domain`, `entity`, `feature:tracks`, `good first issue` | Easy | `I-09`, `I-32` |
| **I-34** | Add UserMilestoneProgress domain entity | `layer:domain` | `layer:domain`, `entity`, `feature:tracks`, `good first issue` | Very Easy | `I-33` |
| **I-35** | Configure EF Core mapping and add migration for Capstone entities | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:tracks` | Medium | `I-32`, `I-33`, `I-34` |
| **I-36** | Unit tests for Capstone and Milestone progression invariants | `layer:tests` | `layer:tests`, `testing`, `feature:tracks`, `good first issue` | Easy | `I-34` |

---

## 3. Backlog Implementation Plan (Second Wave)

These issues are scheduled after the first-wave domain and service foundations are merged.

| Order | Issue Title | Layer | Labels | Blocked By | Rationale for Deferral |
|---|---|---|---|---|---|
| **B-01** | [Tracking] Phase 4: Public 3-Click Resource Hub | `layer:application` | `setup`, `layer:application`, `feature:tracks` | `I-31` | Slices Phase 4 delivery after Step 1 migration. |
| **B-02** | [Tracking] Phase 5: Cumulative Capstone Projects & Track Completion | `layer:application` | `setup`, `layer:application`, `feature:tracks` | `I-35` | Slices Phase 5 delivery after Capstone persistence. |
| **B-03** | Implement CodeforcesClient typed HttpClient in Infrastructure with resilience handler | `layer:infrastructure` | `layer:infrastructure`, `setup`, `feature:exams` | `I-12` | Production HTTP client; offline development uses `FakeCodeforcesClient` (`I-13`). |
| **B-04** | Implement GitHubClient typed HttpClient in Infrastructure with OAuth bearer support | `layer:infrastructure` | `layer:infrastructure`, `setup`, `feature:tracks` | `I-12` | Production HTTP client; offline development uses `FakeGitHubClient` (`I-13`). |
| **B-05** | Wire Codeforces handle linking and verification API endpoints in Api | `layer:api` | `layer:api`, `controller`, `feature:exams` | `I-19` | Depends on Application verification service. |
| **B-06** | Wire GitHub OAuth initiate and callback endpoints in Api | `layer:api` | `layer:api`, `controller`, `feature:auth` | `I-05`, `I-06` | Depends on OAuth state helper and configuration. |
| **B-07** | Wire Codeforces problem solve verification API endpoint in Api | `layer:api` | `layer:api`, `controller`, `feature:exams` | `I-20`, `I-26` | Depends on solve matching service and rate limiter. |
| **B-08** | Wire automated GitHub task verification API endpoint in Api | `layer:api` | `layer:api`, `controller`, `feature:tracks` | `I-21`, `I-22`, `I-26` | Depends on task verification service and rate limiter. |
| **B-09** | Implement Admin endpoints for authoring ModuleProblems and GitHubTasks | `layer:api` | `layer:api`, `controller`, `feature:tracks` | `I-14`, `I-16` | Admin authoring flows; learner reads and verification come first. |
| **B-10** | Implement scheduled background link-health checker hosted service in Api | `layer:api` | `layer:api`, `setup`, `feature:tracks` | `I-30` | Background maintenance worker; public browse can function before link checker. |
| **B-11** | Implement Resource Hub read service with multi-criteria search, filters, and pagination | `layer:application` | `layer:application`, `dto`, `feature:tracks` | `I-30` | Query service for anonymous Hub browse. |
| **B-12** | Wire public anonymous Resource Hub browse endpoints in Api | `layer:api` | `layer:api`, `controller`, `feature:tracks` | `B-11` | Depends on Hub read service. |
| **B-13** | Implement Admin Resource Hub curation service and endpoints | `layer:api` | `layer:api`, `controller`, `feature:tracks` | `I-30` | Admin moderation queue; browse reads come first. |
| **B-14** | Migration Step 2: Make LessonResource.ResourceId required and drop redundant columns | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:tracks` | `I-31` | Two-step migration rule: requires verified production backfill before dropping columns. |
| **B-15** | Implement cumulative capstone milestone automated verification service | `layer:application` | `layer:application`, `feature:tracks` | `I-21`, `I-35` | Depends on GitHub task verifier and milestone persistence. |
| **B-16** | Implement derived Track completion evaluator service | `layer:application` | `layer:application`, `feature:tracks` | `B-15`, `I-24` | Evaluates derived completion when all modules and milestones pass. |
| **B-17** | Wire learner Capstone project and milestone verification endpoints in Api | `layer:api` | `layer:api`, `controller`, `feature:tracks` | `B-15`, `I-26` | Depends on milestone verification service. |
| **B-18** | Wire Admin Capstone project and milestone authoring endpoints in Api | `layer:api` | `layer:api`, `controller`, `feature:tracks` | `I-35` | Admin authoring flows for capstones. |
| **B-19** | Integration test: Full capstone milestone progression and track completion | `layer:tests` | `layer:tests`, `testing`, `feature:tracks` | `B-16`, `B-17` | End-to-end milestone verification test. |
| **B-20** | End-to-end integration test: Learner journey through enrollment, exam, Codeforces solve, GitHub task, and Capstone | `layer:tests` | `layer:tests`, `testing`, `feature:tracks`, `feature:exams` | `B-19`, `#271` | Comprehensive system test across all three pillars. |

---

## 4. First 5 Issues Recommended for Newcomers

These issues are self-contained, clearly scoped to the Domain or Application contracts, have zero external dependencies, and are marked `good first issue`:

1. **`I-07`: Add ProblemKind enum and ModuleProblem domain entity**
   - *Why start here:* Pure Domain model task with clear properties (`Id`, `ModuleId`, `Kind`, `ExternalId`, `DifficultyRating`, `Title`, `Url`, `Order`, `IsRequired`). Great introduction to Clean Architecture entity conventions.
2. **`I-08`: Add ExternalPlatform and AccountVerificationStatus enums and UserExternalAccount entity**
   - *Why start here:* Pure Domain model representing verified external accounts (Codeforces and GitHub). Teaches entity construction without database complexity.
3. **`I-12`: Define ICodeforcesClient and IGitHubClient interfaces in Application**
   - *Why start here:* Pure interface definitions in `src/TalebElm.Application/Services/Abstraction/`. Establishes the contracts for external APIs without touching HTTP networking.
4. **`I-18`: Implement Codeforces challenge token generation service**
   - *Why start here:* Small, self-contained Application utility generating 128-bit cryptographically secure random tokens with a 15-minute expiration timestamp. Easy to unit test in isolation.
5. **`I-27`: Tooling: Create example starter template repository task per TASK_TEMPLATE_SPEC**
   - *Why start here:* Non-code repository setup task following [`docs/TASK_TEMPLATE_SPEC.md`](TASK_TEMPLATE_SPEC.md). Creates the initial template layout with test suite and GitHub Actions workflow.
