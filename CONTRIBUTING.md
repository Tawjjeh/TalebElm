# Contributing to TalebElm

Thank you for contributing to TalebElm. This guide defines the workflow, quality standards, and review expectations for all contributors.

---

## 1. Prerequisites

- Git (`git --version`)
- [.NET 10 SDK](https://dotnet.microsoft.com/download) (`dotnet --version` → `10.x`)
- A GitHub account with the repository forked

Verify the baseline before starting work:

```bash
dotnet restore TalebElm.slnx
dotnet build TalebElm.slnx
dotnet test TalebElm.slnx
```

---

## Finding work

Issues are grouped by the MVP milestones described in [`docs/business/ROADMAP.md`](docs/business/ROADMAP.md):

- **M0 Foundations** — the build, validation, and API baseline.
- **M1 Identity** — registration, login, JWT, and user access.
- **M2 Learning flow** — tracks, lessons, exams, grading, and progression.
- **M3 Codeforces problems** — external accounts, Codeforces verification, and the problem gate.
- **M3b GitHub tasks** — GitHub OAuth, task versions, integrity checks, and task authoring. Starts after M3.
- **M4 Public Resource Hub (read-only)** — anonymous browse and filters.
- **After MVP** — capstone projects and Hub authoring.

Start with a `good first issue` if you are new. They are small, self-contained, and safe to change. `help wanted` marks work the project has accepted.

### Claiming an issue

Anyone can open an issue. It is ready to build only when all three are true:

- it carries the `help wanted` label,
- it has a milestone and a scope you can finish in one pull request,
- and someone from the project has replied on the issue saying it is a go.

A label on its own is not a green light, and neither is an issue you opened yourself. GitHub cannot enforce this order, so we ask you to wait for the reply.

To claim an issue:

1. Comment with what you plan to change and which files you expect to touch.
2. Wait for the go-ahead. When you get it, you are assigned, and one person per issue.
3. Create your branch after that, not before, so your work lands on top of the current `main`.

If you already have two open issues assigned to you, finish one first. If you go quiet for about two weeks we will check in, and after three we may free the issue for someone else. That is not a judgement on you; it keeps the queue honest.

Open a pull request only for an issue that got the go-ahead, and write `Closes #123` in the description so the issue closes when the pull request merges. If the work turns out bigger than the issue suggested, say so on the issue before you keep going.

A `linked-issue` check then runs on the pull request and turns green when the issue you close is open for work, carries `help wanted`, and is assigned to you. It is not a merge gate by itself, but we will ask about a red one before merging.

The label catalog is in [`docs/GITHUB_LABELS.md`](docs/GITHUB_LABELS.md). Every issue has one `layer:` label and one or two component labels so you can find work in the part of the codebase you want to learn.

---

## 2. Contribution workflow

TalebElm uses a **fork-based workflow**. All changes land via pull request against `main`; direct pushes to `main` are not accepted.

### Step 1 — Fork and clone

1. Fork `Tawjjeh/TalebElm` via the GitHub **Fork** button.
2. Clone your fork:

```bash
git clone https://github.com/<YourUsername>/TalebElm.git
cd TalebElm
```

### Step 2 — Track upstream

```bash
git remote add upstream https://github.com/Tawjjeh/TalebElm.git
git remote -v
```

Expected remotes:

```text
origin    https://github.com/<YourUsername>/TalebElm.git
upstream  https://github.com/Tawjjeh/TalebElm.git
```

### Step 3 — Sync before branching

```bash
git checkout main
git fetch upstream
git merge upstream/main
git push origin main
```

### Step 4 — Create a scoped branch

Branch from an up-to-date `main` using the `<type>/<short-description>` convention:

```bash
git checkout -b feat/track-enrollment-endpoint
```

Branch prefixes:

- `feat/` — new capability
- `fix/` — defect correction
- `docs/` — documentation only
- `refactor/`, `test/`, `chore/`, `ci/`, `build/`, `perf/`, `style/` — as applicable

Examples: `feat/enrollment-endpoint`, `fix/exam-score-rounding`, `docs/api-endpoint-matrix`.

### Step 5 — Implement

- One logical change per pull request.
- Place code in the correct Clean Architecture layer — see [`ARCHITECTURE.md`](ARCHITECTURE.md) and [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md).
- Application services depend only on `IUnitOfWork`; controllers call service interfaces; Infrastructure contains no business logic.
- Add or update tests covering the change.
- Never commit secrets, connection strings, `*.db` files, or environment-specific credentials.

### Step 6 — Commit

Stage and commit with a conventional-commit message:

```bash
git add <files>
git commit -m "feat: add track enrollment endpoint"
```

Accepted commit/PR-title types (validated by CI; optional scope permitted):

- `feat`, `fix`, `chore`, `refactor`, `docs`, `test`, `ci`, `build`, `perf`, `style`

Examples:

```text
feat: add track enrollment endpoint
fix(auth): reject duplicate user email with 409
docs: clarify exam grading contracts
test: cover module ordering constraints
```

### Step 7 — Push and open the pull request

```bash
git push --set-upstream origin feat/track-enrollment-endpoint
```

Then open a pull request:

- **Base:** `Tawjjeh/TalebElm` @ `main`
- **Head:** `<YourUsername>/TalebElm` @ your branch
- **Title:** conventional-commit format (e.g., `feat: add track enrollment endpoint`)
- **Description:** complete the PR template, including `Closes #NNN` for `feat:` and `fix:` PRs so the issue closes automatically. `chore:` / `docs:` / `refactor:` / `test:` / `ci:` maintenance PRs do not require issue linkage.

We assign one person to each new pull request, and path-based labels are applied by CI. You do not need to request a reviewer.

---

## 3. Quality gates

All pull requests must satisfy the following before merge:

- [ ] `dotnet build TalebElm.slnx` succeeds without errors.
- [ ] New or affected behavior is covered by tests (`dotnet test TalebElm.slnx` passes).
- [ ] Changes respect Clean Architecture boundaries described in `ARCHITECTURE.md`.
- [ ] Naming, folder, DTO, and EF Core conventions in `docs/ARCHITECTURE.md` are followed.
- [ ] No secrets, production connection strings, or database artifacts are committed.
- [ ] PR title uses conventional-commit format; `feat:` / `fix:` PRs link their issue.
- [ ] The issue was approved and assigned to you before you started, or this is a docs/chore PR that needs no issue (see [Claiming an issue](#claiming-an-issue)).

CI additionally enforces Release-mode build/test parity, dependency vulnerability review (fails on high/critical), full-tree NuGet audit, and automatic labeling.

---

## 4. Review process

- The person assigned to your pull request reviews it. Assignment happens automatically when you open the pull request: security and database changes go to one of the two core reviewers, everything else rotates among the regular reviewers.
- We review for correctness, architecture, tests, and documentation impact.
- A pull request needs one approving review, a green `build-and-test` check, and every conversation resolved before it can merge.
- After approving, the assignee merges it by commenting `/merge`. The owner can comment `/merge` on any pull request. You do not press a button and you do not need to ask someone else to merge for you; a `/merge` from anyone else is politely declined.
- If `main` moved while your pull request was open, merge `main` into your branch and push, so the required check runs against the current base.
- Address feedback by pushing additional commits to the same branch; the PR updates automatically. A new commit removes the earlier approval, so the assignee approves the latest commit again.
- Keep the PR scope tight so it can be reviewed and reverted safely. Split unrelated changes into separate PRs.

---

## 5. Reference documentation

- [`README.md`](README.md) — platform overview and operator guide
- [`ARCHITECTURE.md`](ARCHITECTURE.md) — layer responsibilities and request lifecycle
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — normative technical conventions
- [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) — implementation status per feature
- [`docs/PROJECT_STRUCTURE.md`](docs/PROJECT_STRUCTURE.md) — source layout and scope boundaries
