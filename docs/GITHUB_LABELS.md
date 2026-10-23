# TalebElm — GitHub Labels Catalog

> **Read this first:** tasks are tracked as GitHub issues in this repository.
> When you open the issue for your task, apply exactly these labels so
> the team can sort and find the work. Colors below are GitHub hex codes
> (used without the `#`). Keep the label names exactly as written here so they
> stay consistent across the whole repository.

---

## 1. Layer Labels (where the code lives)

| Label | Description | Color |
|---|---|---|
| `layer:domain` | The code goes in the Domain project — core business rules: entities, enums, exceptions, and repository interfaces. The innermost layer, depends on nothing. | `1D76DB` |
| `layer:application` | The code goes in the Application project — what the app can do: DTOs, service interfaces and implementations, validators. | `5319E7` |
| `layer:infrastructure` | The code goes in the Infrastructure project — the "how": EF Core, `AppDbContext`, entity configurations, migrations, and repositories. | `0E8A16` |
| `layer:api` | The code goes in the Api project — the web door: controllers, middleware, startup. | `FB8C00` |
| `layer:tests` | The code goes in the Tests project — unit and integration test classes. | `008672` |

## 2. Component Labels (what kind of artifact it is)

| Label | Description | Color |
|---|---|---|
| `entity` | You create a class that represents a core "thing" (User, Track, Module, Lesson, LessonResource, Exam, TrackEnrollment, UserProgress). Inherits `BaseEntity`. | `006B75` |
| `enum` | You create a fixed list of named choices (TrackStatus, LessonType, LessonResourceKind, UserRole). | `FBCA04` |
| `exception` | You create an error class that inherits from `Exception` or `DomainException`. | `D93F0B` |
| `interface` | You create a contract — a list of method names with no bodies. | `0075CA` |
| `dto` | You create a record used to move data between layers (requests and responses). | `C5DEF5` |
| `database` | The code is about the database — EF Core context, configurations, or migrations. | `116329` |
| `controller` | You create a class that handles web requests (receives HTTP, sends HTTP). | `E99695` |
| `middleware` | You create a pipeline step that every web request passes through. | `8B008B` |
| `validation` | You create an input-validation class (for example using FluentValidation). | `FF7619` |
| `testing` | You create a test class with `[Fact]` methods. | `FEF2C0` |
| `setup` | The task needs a NuGet package or foundational plumbing before any other task can use it. Great for learning the command line. | `BFD4F2` |

## 3. Feature / Vertical Slice Labels (what the work serves)

| Label | Description | Color |
|---|---|---|
| `feature:auth` | Work that serves the Authentication & Users slice: register, login, user profiles. | `D4C5F9` |
| `feature:tracks` | Work that serves the Tracks & Content Management slice: tracks, modules, lessons. | `BFDADC` |
| `feature:exams` | Work that serves the Assessments & Progress slice: exams, results, user progress, progression lock. | `FAD8D0` |
| `feature:enrollment` | Work that serves the explicit Track Enrollment slice (separating track enrollment from per-module progress). | `0969DA` |
| `feature:problems` | Work that serves the Practice Problems & External Verification slice: Codeforces handle challenge, solve checking API, GitHub tasks. | `1F883D` |
| `feature:resources` | Work that serves the Public Resource Hub slice: technology tree, curated resources, filtering, admin moderation. | `0052CC` |
| `feature:capstone` | Work that serves the Capstone Projects slice: track-level projects, milestones, repository submissions, mentor reviews. | `8957E5` |

## 4. Difficulty / Onboarding Labels

| Label | Description | Color |
|---|---|---|
| `good first issue` | Perfect for a beginner — small, clear, and safe to make mistakes on. Start here if you are new to the project. | `7057FF` |

## 5. Status & Triage Labels (issue health)

| Label | Description | Color |
|---|---|---|
| `bug` | Something isn't working. | `D73A4A` |
| `enhancement` | New feature or request. | `A2EEEF` |
| `question` | Further information is requested or learner inquiry. | `D876E3` |
| `help wanted` | Extra attention is needed. | `008672` |
| `documentation` | Improvements or additions to documentation. | `0075CA` |
| `duplicate` | This issue or pull request already exists. | `CFD3D7` |
| `invalid` | This doesn't seem right. | `E4E669` |
| `wontfix` | This will not be worked on. | `FFFFFF` |

## 6. Priority Labels

| Label | Description | Color |
|---|---|---|
| `priority: critical` | Must be done first; blocks the roadmap. | `B60205` |
| `priority: high` | Needed for the current milestone. | `D93F0B` |
| `priority: medium` | Important but not blocking. | `FBCA04` |
| `priority: low` | Nice to have / polish. | `0E8A16` |

## 7. Cross-Cutting & Area Labels

| Label | Description | Color |
|---|---|---|
| `backend` | Server-side implementation. | `1D76DB` |
| `architecture` | Structural / layering decisions. | `5319E7` |
| `security` | Auth, hashing, tokens, abuse prevention. | `B60205` |

---

## Rules for contributors

- Always include **one** `layer:` label and **one or two** `component` labels.
- Add the matching `feature:` label when you can (`feature:auth`,
  `feature:tracks`, `feature:exams`, `feature:enrollment`, `feature:problems`,
  `feature:resources`, or `feature:capstone`).
- Add `good first issue` if the task is beginner-friendly AND tiny (most
  Very Easy / Easy tasks qualify).
- Never invent new label names — propose them in a PR comment first so the
  catalog stays consistent.