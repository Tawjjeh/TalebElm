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
- **Track Capstone:** At the end of each Track, the learner completes a comprehensive capstone project, or builds one cumulative project progressively across the Track's modules, ensuring real-world practical experience (`Proposed`).

### Pillar 2: Problems (Externally Verified Practice)

- **Fundamentals Modules (Codeforces-Verified):**
  - Practice problems for fundamentals (syntax, algorithms, data structures) are sourced from Codeforces (`Target`).
  - The learner links their public Codeforces handle on their profile (`Target`).
  - **Ownership Verification Challenge:** The platform issues a time-boxed challenge requiring the learner to submit a deliberately failing solution (e.g., a compilation error) to a designated problem. The platform checks this submission via the public Codeforces API (`user.status`) to cryptographically prove handle ownership without asking for passwords (`Target`).
  - **Automated Solve Verification:** When the learner solves an assigned problem, the platform queries the public Codeforces API (`user.status`) to confirm an `OK` (Accepted) verdict (`Target`). The learner never self-reports solves.
- **Framework Modules (Practical GitHub Tasks):**
  - Algorithm problems do not fit framework concepts (e.g., ASP.NET Core, EF Core, Spring Boot). Instead, framework modules assign practical tasks (`Target`).
  - In the MVP, the learner submits a GitHub repository URL, reviewed manually or via an evaluation checklist (`Proposed`). Automated verification via GitHub Actions or test suites is reserved for a future phase (`Proposed`).
- **Extensibility:** The domain model supports multiple external platforms via an enum and external identifier (`PlatformEnum`, `ExternalId`), but only Codeforces and manual GitHub tasks are in scope for the initial rollout (`Target`).

### Pillar 3: Resource Hub (Public & Anonymous)

- **Public Access:** A well-organized index of learning references across languages, frameworks, and tools. Read access is 100% anonymous; no account or login is required (`Target`).
- **Three-Click Rule:** A visitor can reach any specific topic (e.g., "Spring Boot $\to$ Dependency Injection") in at most 3 clicks:
  1. Language / Ecosystem (e.g., Java, C#, TypeScript)
  2. Framework / Technology (e.g., Spring Boot, ASP.NET Core)
  3. Topic $\to$ Curated Resources
- **Rich Metadata:** Every topic supports multiple sources. Each Resource record includes:
  - Title, URL, or bibliographic citation
  - Kind (`Book`, `Documentation`, `Video`, `Article`, `Repository`, `Other`)
  - Human Language (e.g., Arabic, English)
  - Difficulty Level (`Beginner`, `Intermediate`, `Advanced`)
  - Cost (`Free`, `Paid`)
  - Approval Status (`Approved`, `PendingApproval`, `Rejected`)
- **Unified Resource Model:** Hub resources and Lesson resources share one underlying `Resource` domain concept so references are never duplicated (`Proposed`).
- **Community Contributions:** Community submissions with admin moderation are planned for a subsequent phase (`Proposed`).

---

## 5. Non-Goals

To maintain high development velocity and architectural focus, the following are explicitly out of scope:

1. **In-Browser IDE / Code Execution:** TalebElm will not build or maintain an online compiler, code sandbox, or container runner. We leverage Codeforces and GitHub.
2. **Contest Hosting Platform:** TalebElm is an educational progression engine, not a competitive programming judge.
3. **Live Classrooms & Scheduling:** No live streams, calendar scheduling, or attendance systems.
4. **Accredited Degree Granting:** TalebElm produces portfolio-ready engineers, not university credentials.

---

## 6. Risks & Mitigations

| Risk | Impact | Mitigation Strategy |
|---|---|---|
| **Content Authoring Bottleneck** | Slow release of new Tracks due to the effort of authoring lessons, exams, and curating problem sets. | Focus initial tracks on backend core (.NET). Use open-source community contributions with clear Markdown authoring templates and admin review. |
| **Dead External Links** | Third-party tutorials, docs, or repositories move or go offline. | Hub topics support multiple fallback sources. Implement automated link-health validation jobs (`Target`). |
| **Learner Gating Frustration** | Hard problem sets or strict exams cause learners to abandon the platform. | Carefully calibrate initial problem difficulty (Codeforces 800–1000 rating bounds). Admin manual-unlock capability exists for support overrides (`Target`). |
| **Codeforces Dependency & Rate Limits** | Public Codeforces API may experience downtime or enforce rate limits (~1 req / 2s). | Verify all specifics against official docs (`codeforces.com/apiHelp`). Implement aggressive local caching, request debouncing, and asynchronous background polling queues (`Target`). |
| **Beginner Difficulty Mismatch** | Competitive programming problems can overwhelm novice programmers with math riddles. | Curate problem sets strictly focused on language fundamentals (loops, conditionals, basic data structures) rather than tricky competitive algorithms (`Target`). |

---

## 7. Success Metrics

1. **Enrollment-to-Completion Ratio:** Percentage of learners who enroll in a Track and reach the capstone project.
2. **Problem Verification Throughput:** Number of Codeforces solves verified through the automated API pipeline.
3. **Resource Hub Reach:** Monthly unique anonymous visitors and search-to-resource click-through rates.
4. **Time-to-Second-Module:** Median time for a new learner to complete Module 1 (indicates healthy onboarding difficulty).
5. **Contributor Velocity:** Number of merged community pull requests adhering to Clean Architecture standards.
