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
   - Pre-existing issues (#274, #324, #321, #325, #323, #248, #278, #279, #280, #289, #244, #246, #273, #250, #254, #255, #256, #208, #268, #266, etc.) are foundational and linked directly.
   - Issues #322 and #326 were successfully resolved and closed via PRs #329 and #328.
   - This plan specifies new work for automated problem verification, the public 3-click Resource Hub, cumulative capstone projects, and missing domain/security foundations.
4. **Label Usage:**
   - Issues use **only approved labels** from [`docs/GITHUB_LABELS.md`](GITHUB_LABELS.md), including the newly created feature labels: `feature:enrollment`, `feature:problems`, `feature:resources`, and `feature:capstone`.
5. **Staged Publication Waves:**
   - **First Wave (Issues #332–#355):** Published on GitHub. Covers binding decision questions, core domain entities, test fakes, EF Core mappings/migrations, and automated verification services.
   - **Backlog (Deferred to Wave 2):** Kept in this plan's backlog table. Scheduled after the initial wave merges to keep the open issue queue healthy and welcoming for newcomers.

---

## 2. First Wave Issues (Published on GitHub)

| Issue | Title | Layer | Labels | Difficulty | Blocked By | Blocks |
|---|---|---|---|---|---|---|
| [#332](https://github.com/Tawjjeh/TalebElm/issues/332) | [Decision] Settle LessonType enum lifecycle (Decision 13) | `layer:domain` | `question`, `layer:domain`, `feature:tracks` | Easy | None | Authoring CRUD |
| [#333](https://github.com/Tawjjeh/TalebElm/issues/333) | [Decision] Select OAuth application model for GitHub automated task verification (Decision 24) | `layer:application` | `question`, `layer:application`, `feature:problems` | Easy | None | Task verifier |
| [#334](https://github.com/Tawjjeh/TalebElm/issues/334) | Add ProblemKind enum and ModuleProblem domain entity | `layer:domain` | `layer:domain`, `entity`, `enum`, `feature:problems`, `good first issue` | Very Easy | None | #343 |
| [#335](https://github.com/Tawjjeh/TalebElm/issues/335) | Add ExternalPlatform and AccountVerificationStatus enums and UserExternalAccount entity | `layer:domain` | `layer:domain`, `entity`, `enum`, `feature:problems`, `good first issue` | Very Easy | None | #344 |
| [#336](https://github.com/Tawjjeh/TalebElm/issues/336) | Add GitHubTask domain entity | `layer:domain` | `layer:domain`, `entity`, `feature:problems`, `good first issue` | Very Easy | None | #337 |
| [#337](https://github.com/Tawjjeh/TalebElm/issues/337) | Add TaskVersion domain entity with Git commit and tree SHAs | `layer:domain` | `layer:domain`, `entity`, `feature:problems`, `good first issue` | Easy | #336 | #345 |
| [#338](https://github.com/Tawjjeh/TalebElm/issues/338) | Add UserProblemCompletion unified completion domain entity | `layer:domain` | `layer:domain`, `entity`, `feature:problems`, `good first issue` | Easy | #334, #337 | #346 |
| [#339](https://github.com/Tawjjeh/TalebElm/issues/339) | Add TechnologyKind enum and Technology self-referencing tree domain entity | `layer:domain` | `layer:domain`, `entity`, `enum`, `feature:resources`, `good first issue` | Very Easy | None | #340 |
| [#340](https://github.com/Tawjjeh/TalebElm/issues/340) | Add ResourceKind, ResourceLevel, ResourceStatus enums and Resource domain entity | `layer:domain` | `layer:domain`, `entity`, `enum`, `feature:resources`, `good first issue` | Very Easy | #339 | #347 |
| [#341](https://github.com/Tawjjeh/TalebElm/issues/341) | Define ICodeforcesClient and IGitHubClient interfaces in Application | `layer:application` | `layer:application`, `interface`, `feature:problems`, `good first issue` | Very Easy | None | #342 |
| [#342](https://github.com/Tawjjeh/TalebElm/issues/342) | Create FakeCodeforcesClient and FakeGitHubClient in Tests for offline testing | `layer:tests` | `layer:tests`, `testing`, `feature:problems`, `good first issue` | Easy | #341 | #349, #351 |
| [#343](https://github.com/Tawjjeh/TalebElm/issues/343) | Configure EF Core mapping and add migration for ModuleProblem | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:problems` | Medium | #334 | #346 |
| [#344](https://github.com/Tawjjeh/TalebElm/issues/344) | Configure EF Core mapping and add migration for UserExternalAccount | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:problems` | Medium | #335 | #349 |
| [#345](https://github.com/Tawjjeh/TalebElm/issues/345) | Configure EF Core mapping and add migration for GitHubTask and TaskVersion | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:problems` | Medium | #337 | #346 |
| [#346](https://github.com/Tawjjeh/TalebElm/issues/346) | Configure EF Core mapping and add migration for UserProblemCompletion | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:problems` | Medium | #338, #343 | #350 |
| [#347](https://github.com/Tawjjeh/TalebElm/issues/347) | Configure EF Core mapping and add migration for Technology and Resource | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:resources` | Medium | #340 | Resource Hub reads |
| [#348](https://github.com/Tawjjeh/TalebElm/issues/348) | Implement Codeforces challenge token generation service | `layer:application` | `layer:application`, `feature:problems`, `good first issue` | Easy | #335 | #349 |
| [#349](https://github.com/Tawjjeh/TalebElm/issues/349) | Implement Codeforces handle profile verification service via user.info | `layer:application` | `layer:application`, `feature:problems` | Medium | #344, #348 | #350 |
| [#350](https://github.com/Tawjjeh/TalebElm/issues/350) | Implement Codeforces solve matching service via user.status with 60s cooldown | `layer:application` | `layer:application`, `feature:problems` | Medium | #346, #349 | Dual unlock |
| [#351](https://github.com/Tawjjeh/TalebElm/issues/351) | Implement GitHub repository provenance and CI workflow run verifier | `layer:application` | `layer:application`, `feature:problems` | Medium | #342, #345 | #352 |
| [#352](https://github.com/Tawjjeh/TalebElm/issues/352) | Implement Git workflow and test suite SHA integrity-check service | `layer:application` | `layer:application`, `feature:problems` | Medium | #337, #351 | #353 |
| [#353](https://github.com/Tawjjeh/TalebElm/issues/353) | Unit test GitHub SHA integrity-checker with recorded Git tree API fixtures | `layer:tests` | `layer:tests`, `testing`, `feature:problems` | Medium | #352 | Capstone task verifier |
| [#354](https://github.com/Tawjjeh/TalebElm/issues/354) | [Tracking] Phase 3: Automated Practice Problems & External Verification | `layer:application` | `setup`, `layer:application`, `feature:problems` | Easy | None | Phase 3 completion |
| [#355](https://github.com/Tawjjeh/TalebElm/issues/355) | [Tracking] Phase 4: Public Resource Hub & Curated Learning Materials | `layer:application` | `setup`, `layer:application`, `feature:resources` | Easy | None | Phase 4 completion |

---

## 3. Backlog Implementation Plan (Deferred to Wave 2)

These issues are scheduled for publication after the first-wave domain and service foundations merge, preventing issue tracker exhaustion.

| Order | Issue Title | Layer | Labels | Blocked By | Rationale for Deferral |
|---|---|---|---|---|---|
| **B-01** | [Tracking] Phase 5: Cumulative Capstone Projects & Track Completion | `layer:application` | `setup`, `layer:application`, `feature:capstone` | `#354` | Slices Phase 5 delivery after task verifier lands. |
| **B-02** | Implement CodeforcesClient typed HttpClient in Infrastructure with resilience handler | `layer:infrastructure` | `layer:infrastructure`, `setup`, `feature:problems` | `#341` | Production HTTP client; offline tests use `FakeCodeforcesClient` (`#342`). |
| **B-03** | Implement GitHubClient typed HttpClient in Infrastructure with OAuth bearer support | `layer:infrastructure` | `layer:infrastructure`, `setup`, `feature:problems` | `#341` | Production HTTP client; offline tests use `FakeGitHubClient` (`#342`). |
| **B-04** | Wire Codeforces handle linking and verification API endpoints in Api | `layer:api` | `layer:api`, `controller`, `feature:problems` | `#349` | Depends on Application verification service. |
| **B-05** | Wire GitHub OAuth initiate and callback endpoints in Api | `layer:api` | `layer:api`, `controller`, `feature:auth` | `#333` | Depends on OAuth application model decision. |
| **B-06** | Wire Codeforces problem solve verification API endpoint in Api | `layer:api` | `layer:api`, `controller`, `feature:problems` | `#350` | Depends on solve matching service. |
| **B-07** | Wire automated GitHub task verification API endpoint in Api | `layer:api` | `layer:api`, `controller`, `feature:problems` | `#351`, `#352` | Depends on task verification and SHA integrity service. |
| **B-08** | Implement Admin endpoints for authoring ModuleProblems and GitHubTasks | `layer:api` | `layer:api`, `controller`, `feature:problems` | `#343`, `#345` | Learner verification flow prioritized over authoring endpoints. |
| **B-09** | Implement scheduled background link-health checker hosted service in Api | `layer:api` | `layer:api`, `setup`, `feature:resources` | `#347` | Background worker; public read browse works before scheduled checker. |
| **B-10** | Implement Resource Hub read service with multi-criteria search, filters, and pagination | `layer:application` | `layer:application`, `dto`, `feature:resources` | `#347` | Query service for anonymous Hub browse. |
| **B-11** | Wire public anonymous Resource Hub browse endpoints in Api | `layer:api` | `layer:api`, `controller`, `feature:resources` | `B-10` | Depends on Hub read service. |
| **B-12** | Implement Admin Resource Hub curation service and endpoints | `layer:api` | `layer:api`, `controller`, `feature:resources` | `#347` | Moderation queue; public browse read prioritized. |
| **B-13** | Migration Step 1: Add ResourceId FK to LessonResource and backfill existing rows | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:resources` | `#347` | Non-destructive Step 1 schema change. |
| **B-14** | Migration Step 2: Make LessonResource.ResourceId required and drop redundant columns | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:resources` | `B-13` | Two-step migration rule: requires verified production backfill. |
| **B-15** | Add CapstoneProject and ProjectMilestone domain entities | `layer:domain` | `layer:domain`, `entity`, `feature:capstone`, `good first issue` | `#336` | Pure domain entity task for cumulative track capstones. |
| **B-16** | Configure EF Core mapping and add migration for Capstone entities | `layer:infrastructure` | `layer:infrastructure`, `database`, `feature:capstone` | `B-15` | EF Core mapping and migration for capstone milestone entities. |
| **B-17** | Implement cumulative capstone milestone automated verification service | `layer:application` | `layer:application`, `feature:capstone` | `B-16`, `#351` | Reuses GitHub automated task verifier for milestone deliverables. |
| **B-18** | Implement derived Track completion evaluator service | `layer:application` | `layer:application`, `feature:capstone` | `B-17`, `#208` | Derives track completion when all modules and milestones pass. |
| **B-19** | Wire learner Capstone project and milestone verification endpoints in Api | `layer:api` | `layer:api`, `controller`, `feature:capstone` | `B-17` | Endpoints for submitting repository links for milestone verification. |
| **B-20** | End-to-end integration test: Learner journey through enrollment, exam, Codeforces solve, GitHub task, and Capstone | `layer:tests` | `layer:tests`, `testing`, `feature:capstone` | `B-18`, `#271` | Comprehensive system test validating zero manual gating. |

---

## 4. First 5 Issues Recommended for Newcomers

These issues are self-contained, clearly scoped to the Domain or Application contracts, have zero external dependencies, and are marked `good first issue`:

1. **[#334](https://github.com/Tawjjeh/TalebElm/issues/334): Add ProblemKind enum and ModuleProblem domain entity**
   - *Why start here:* Pure Domain model task with clear properties (`Id`, `ModuleId`, `Kind`, `ExternalIdentifier`, `Title`, `Order`, `IsRequired`, `Rating`). Great introduction to Clean Architecture entity conventions.
2. **[#335](https://github.com/Tawjjeh/TalebElm/issues/335): Add ExternalPlatform and AccountVerificationStatus enums and UserExternalAccount entity**
   - *Why start here:* Pure Domain model representing verified external accounts (Codeforces and GitHub). Teaches entity construction without database complexity.
3. **[#336](https://github.com/Tawjjeh/TalebElm/issues/336): Add GitHubTask domain entity**
   - *Why start here:* Self-contained Domain entity for framework task templates (`TemplateRepository`, `WorkflowFileName`, `MinPassingTests`, `IsActive`).
4. **[#341](https://github.com/Tawjjeh/TalebElm/issues/341): Define ICodeforcesClient and IGitHubClient interfaces in Application**
   - *Why start here:* Pure interface definitions in `src/TalebElm.Application/Services/Abstraction/`. Establishes the contracts for external APIs without touching HTTP networking.
5. **[#348](https://github.com/Tawjjeh/TalebElm/issues/348): Implement Codeforces challenge token generation service**
   - *Why start here:* Small, self-contained Application utility generating 128-bit cryptographically secure random tokens with a 15-minute expiration timestamp. Easy to unit test in isolation.
