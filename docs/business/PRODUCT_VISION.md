# TalebElm Product Vision & Core Pillars

> **Status discipline:** Every concept in this document is marked **Implemented** (present in code today), **Target** (decided MVP product behavior, scheduled for implementation), or **Proposed** (needs an architectural or product decision before code is written).
> Terms match the Clean Architecture Domain model in `src/TalebElm.Domain/`.

---

## 1. Problem Statement

Learning software engineering today is broken in two distinct ways:

1. **Static roadmaps lack verification:** Platforms like roadmap.sh tell learners *what* technologies exist, but they do not provide curated study material, do not verify understanding, and do not provide hands-on practice. Learners are left guessing whether they have actually mastered a topic.
2. **Video courses encourage passive learning ("tutorial hell"):** Learners watch dozens of hours of video tutorials without writing code independently. This creates an illusion of competence that shatters as soon as they encounter real-world problems or job interviews.

**TalebElm** solves this by combining structured, curated learning paths with **objective, external verification**. Learners cannot simply click "mark as completed"; they must pass knowledge exams and solve externally verified coding problems to unlock subsequent stages.

---

## 2. Target Users

| Persona | Role in System | Needs & Pain Points | Status |
|---|---|---|---|
| **Learner** | `Student` (`UserRole.Student`) | Wants a clear, step-by-step path from zero to job-ready. Needs structured lessons, high-quality curated sources, and verified problem solving without getting stuck in tutorial hell. | Target |
| **Hub Visitor** | Anonymous public visitor | Wants fast, trustworthy reference material across programming languages and frameworks. Needs an organized index without mandatory registration or paywalls. | Target |
| **Admin / Content Author** | `Admin` (`UserRole.Admin`) | Curates tracks, modules, lessons, exam question banks, external problem links, and hub resources. Requires robust authoring tools and auditable moderation. | Target (`UserRole.Admin` implemented; authoring tools target) |
| **Open-Source Contributor** | Developer / Maintainer | Wants clear Clean Architecture conventions, bite-sized issues, well-defined domain contracts, and strong CI gates to contribute effectively. | Implemented (guidelines & architecture active) |

---

## 3. Value Proposition & Differentiators

| Dimension | Video Platforms (Udemy, Coursera) | Roadmap Sites (roadmap.sh) | TalebElm |
|---|---|---|---|
| **Learning Structure** | Video playlists, often unstructured or outdated | High-level topic trees with no inline study flow | Strictly ordered Tracks $\to$ Modules $\to$ Lessons |
| **Verification of Work** | Self-reported or basic quiz | None (purely informational) | External automated verification (Codeforces API + GitHub tasks) |
| **Progression Gating** | Unrestricted skipping | No gating | Sticky progression: pass exam **and** complete problems to unlock |
| **Reference Material** | Scattered in video descriptions | Generic external links | Public 3-click Resource Hub sharing the underlying resource model |
| **License & Community** | Proprietary, paid | Proprietary or ad-driven | 100% Free and Open Source (GPL-3.0) |

---

## 4. The Three Product Pillars

### Pillar 1: Guided Tracks (Structured Learning Delivery)

- **Hierarchical Path:** A learner registers, browses published Tracks, and enrolls in one (`Target`).
- **Content Structure:** A Track contains ordered Modules (`Implemented`). A Module contains ordered Lessons (`Implemented`). Each Module provides authored conceptual explanations and one or more curated sources (`LessonResource`, `Implemented`).
- **Module Assessment:** At the end of each Module, the learner faces:
  1. An Exam (knowledge recall with multiple-choice questions, `Target`).
  2. A set of practice problems (3 or more coding problems/tasks, `Target`).
- **Dual-Gated Sticky Progression:** To unlock the next Module in the Track, the learner must **both** pass the Exam **and** complete all required practice problems (`Target`). Both completion flags are permanent and sticky (`Target`).
- **Track Capstone:** A single cumulative portfolio project built progressively across ordered milestones in the Track, verified automatically via GitHub tasks (`Target`).

### Pillar 2: Problems (100% Automated Verification)

- **Guiding Principle: Zero Manual Gating:** Every step that unlocks a learner is verified automatically by the server. Humans only author content and curate public Hub resources.
- **Problem Kinds (`ProblemKind` Enum):**
  1. `CodeforcesProblem`: Algorithmic problems on Codeforces for fundamentals modules (`Target`).
  2. `GitHubTask`: CodeCrafters-style automated repository tasks for framework modules (`Target`).
- **Codeforces Handle Ownership Challenge:** The server generates a random 128-bit single-use token (expires in 15 minutes). The learner places the token into their Codeforces profile (`firstName` or `organization`). The server reads the profile via the public `user.info` API and stores the verified handle (`Target`).
- **Automated Solve Verification:** The platform queries the public Codeforces API (`user.status`) for `OK` (Accepted) verdicts matching `contestId` and problem `index` (`Target`). All-time accepted solves count.
- **Automated GitHub Tasks:** Learners link their GitHub account via OAuth (numeric `ExternalUserId` stored). For each framework task, the learner generates a repository from an official template (`Tawjjeh/task-<slug>-template`), implements code, and pushes. The server queries the GitHub REST API to verify that:
  - The repository owner matches the linked numeric account ID.
  - The repository was generated from the official template.
  - The latest workflow run on the default branch succeeded.
  - Cryptographic blob/tree SHAs of `.github/workflows/test.yml` and `tests/` match the official `TaskVersion` (`Target`).
- **Extensible Verifier Architecture:** Verifiers sit behind the `IProblemVerifier` interface in Application (`Target`).

### Pillar 3: Resource Hub (Public & Anonymous)

- **Public Access:** A well-organized index of learning references across languages, frameworks, and tools. Read access is 100% anonymous; no account or login is required (`Target`).
- **Three-Click Rule:** A visitor can reach any specific topic (e.g., "Spring Boot $\to$ Dependency Injection") in at most 3 clicks via a self-referencing `Technology` tree (`Target`).
- **Unified Resource Model:** Hub resources and Lesson references share one underlying `Resource` entity (`Target`). `LessonResource` becomes a join table, populated via a two-step non-destructive migration.
- **Automated Quality & Link Health:** Scheduled background link checker probes URLs and marks resources `Broken` after 3 consecutive failures, hiding them from public listings until resolved (`Target`).

---

## 5. Non-Goals

To maintain high development velocity and architectural focus, the following are explicitly out of scope:

1. **In-Browser IDE / Code Execution:** TalebElm will not build or maintain an online compiler, code sandbox, or container runner. We leverage Codeforces and GitHub.
2. **Contest Hosting Platform:** TalebElm is an educational progression engine, not a competitive programming judge.
3. **Live Classrooms & Scheduling:** No live streams, calendar scheduling, or attendance systems.
4. **Accredited Degree Granting:** TalebElm produces portfolio-ready engineers, not university credentials.
5. **Human-in-the-Loop Progression Review:** No human code review or manual checklist gating for learner unlocking.

---

## 6. Known Limitations & Future Hardening

### Automated GitHub Task Verification Limitation
- **The Limitation:** Because the test suite executes on GitHub Actions within the learner's own repository, a determined, technically sophisticated user could theoretically manipulate the CI environment (e.g., monkey-patching runner binaries, intercepting process outputs, or spoofing execution hooks) to make the workflow pass while keeping the file SHAs intact.
- **Future Hardening (Post-MVP):**
  - **Platform-Owned Runners:** In a future phase, webhooks on push will trigger task execution on TalebElm-owned ephemeral runner sandboxes (e.g., isolated Docker containers or Firecracker microVMs).
  - **Hidden / Canary Test Suites:** The platform will inject private, undisclosed test cases at evaluation time that do not exist in the public starter template.
- **MVP Assessment:** For the MVP, Git blob/tree SHA verification combined with template provenance provides strong tamper resistance against 99% of learners while maintaining zero server compute cost.

---

## 7. Risks & Mitigations

| Risk | Impact | Mitigation Strategy |
|---|---|---|
| **Content Authoring Bottleneck** | Slow release of new Tracks due to the effort of authoring lessons, exams, and curating problem sets. | Focus initial tracks on backend core (.NET). Use open-source community contributions with clear Markdown authoring templates and admin review. |
| **Dead External Links** | Third-party tutorials, docs, or repositories move or go offline. | Hub topics support multiple fallback sources. Scheduled automated background link checker (`Target`). |
| **Learner Gating Frustration** | Hard problem sets or strict exams cause learners to abandon the platform. | Calibrate initial problem difficulty: Module 1 problems must be Codeforces rating $\le 800$, rising gradually. Admin manual-unlock support override exists for stuck learners (`Target`). |
| **Codeforces Dependency & Rate Limits** | Public Codeforces API may experience downtime or enforce rate limits (~1 req / 2s). | Verify all specifics against official docs (`codeforces.com/apiHelp`). Implement 60s per-user debouncing, in-memory caching, and fail-safe 503 responses that alter zero learner state (`Target`). |
| **Beginner Difficulty Mismatch** | Competitive programming problems can overwhelm novice programmers with math riddles. | Curate problem sets strictly focused on language fundamentals (loops, conditionals, basic data structures) with soft validator warnings during authoring (`Target`). |

---

## 8. Success Metrics

1. **Enrollment-to-Completion Ratio:** Percentage of learners who enroll in a Track and reach the capstone project.
2. **Problem Verification Throughput:** Number of Codeforces solves and GitHub tasks verified through the automated API pipeline.
3. **Resource Hub Reach:** Monthly unique anonymous visitors and search-to-resource click-through rates.
4. **Time-to-Second-Module:** Median time for a new learner to complete Module 1 (indicates healthy onboarding difficulty).
5. **Contributor Velocity:** Number of merged community pull requests adhering to Clean Architecture standards.
