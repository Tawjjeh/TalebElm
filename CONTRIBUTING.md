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

Start with a `good first issue` if you are new. They are small, self-contained, and safe to change. `help wanted` marks accepted work that is ready for a contributor. Comment on the issue with what you plan to change, then open a pull request when the build and tests pass. You do not need to wait for an assignment.

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

Reviewers are assigned automatically via round-robin rotation; path-based labels are applied by CI.

---

## 3. Quality gates

All pull requests must satisfy the following before merge:

- [ ] `dotnet build TalebElm.slnx` succeeds without errors.
- [ ] New or affected behavior is covered by tests (`dotnet test TalebElm.slnx` passes).
- [ ] Changes respect Clean Architecture boundaries described in `ARCHITECTURE.md`.
- [ ] Naming, folder, DTO, and EF Core conventions in `docs/ARCHITECTURE.md` are followed.
- [ ] No secrets, production connection strings, or database artifacts are committed.
- [ ] PR title uses conventional-commit format; `feat:` / `fix:` PRs link their issue.

CI additionally enforces Release-mode build/test parity, dependency vulnerability review (fails on high/critical), full-tree NuGet audit, and automatic labeling.

---

## 4. Review process

- Maintainers review against correctness, architecture conformance, test coverage, and documentation impact.
- Address feedback by pushing additional commits to the same branch; the PR updates automatically.
- Keep the PR scope tight so it can be reviewed and reverted safely. Split unrelated changes into separate PRs.

---

## 5. Reference documentation

- [`README.md`](README.md) — platform overview and operator guide
- [`ARCHITECTURE.md`](ARCHITECTURE.md) — layer responsibilities and request lifecycle
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — normative technical conventions
- [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) — implementation status per feature
- [`docs/PROJECT_STRUCTURE.md`](docs/PROJECT_STRUCTURE.md) — source layout and scope boundaries
