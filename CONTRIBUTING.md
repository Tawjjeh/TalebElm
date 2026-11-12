# Contributing to TalebElm

This guide explains how to set up your environment, claim an issue, open a pull request, and understand how PR reviews and permissions work.

## 1. Prerequisites and baseline check

You need Git, the [.NET 10 SDK](https://dotnet.microsoft.com/download), and Python 3. After cloning, confirm the baseline passes:

```bash
dotnet restore TalebElm.slnx
dotnet build TalebElm.slnx
dotnet test TalebElm.slnx
python3 scripts/check-docs.py
```

## 2. MVP milestones and picking an issue

All work belongs to the six MVP milestones in [`docs/business/ROADMAP.md`](docs/business/ROADMAP.md):

- **M0 Foundations** — DI composition, validation errors, `CancellationToken` pipeline, logging, and test baseline.
- **M1 Identity** — Registration, login, JWT configuration, throttling, and role policies.
- **M2 Learning flow** — Tracks, Modules, Lessons, Exams, Progress, enrollment, and sticky unlocking.
- **M3 Automated Verification** — External accounts, Codeforces verification, GitHub OAuth, `GitHubTask` / `TaskVersion` integrity, and dual-gate unlock.
- **M4 Public Resource Hub** — Technology tree, anonymous browse/filters, admin curation, link health, and shared `Resource` migration.
- **M5 Capstone Projects** — Cumulative track projects, milestone verification, and derived Track completion.

Start with issues labeled `good first issue` or `help wanted`. See [`docs/GITHUB_LABELS.md`](docs/GITHUB_LABELS.md) for the full label catalog.

To claim an issue:

1. Comment on the issue with your planned approach and the files you expect to touch.
2. Wait for confirmation and assignment so two contributors do not implement the same task.
3. Branch from a current `main` after assignment.

## 3. Fork and branch workflow

TalebElm uses a fork-based workflow. Direct pushes to `main` are not allowed.

1. Fork `Tawjjeh/TalebElm` and clone your fork:

```bash
git clone https://github.com/<YourUsername>/TalebElm.git
cd TalebElm
git remote add upstream https://github.com/Tawjjeh/TalebElm.git
```

2. Sync `main` before creating a branch:

```bash
git checkout main
git fetch upstream
git merge upstream/main
git push origin main
```

3. Create a topic branch (`feat/`, `fix/`, `docs/`, `refactor/`, `test/`, `chore/`, `ci/`):

```bash
git checkout -b feat/track-enrollment-endpoint
```

4. Implement one clean vertical slice following [`ARCHITECTURE.md`](ARCHITECTURE.md) and [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md):
   - `Domain` has zero project dependencies.
   - `Application` depends only on `Domain` and `IUnitOfWork` (never direct `DbContext`).
   - `Infrastructure` handles persistence and external HTTP clients without business logic.
   - Add or update xUnit tests covering the behavior.
   - Never commit secrets, connection strings, or `*.db` files.

5. Commit using conventional commits and push to your fork:

```bash
git commit -m "feat: add track enrollment endpoint"
git push --set-upstream origin feat/track-enrollment-endpoint
```

6. Open a pull request against `Tawjjeh/TalebElm` `main`, link the issue with `Closes #NNN` if applicable, and make sure `build-and-test` passes.

## 4. Automated PR routing (Reviewers vs. Assignees)

When a pull request is opened (and is not a draft), `.github/workflows/auto-assign.yml` reads [`.github/maintainers.json`](.github/maintainers.json) and routes the PR automatically:

- **Reviewers box:** GitHub requests code review from the selected maintainer(s) here.
- **Assignees box:** The selected reviewer(s) are mirrored into Assignees solely so our PR Governance workflow recognizes them as the designated PR Owner(s) authorized to push fixes and merge that specific PR.

### Routing rules

1. **Database and Security PRs:** Any PR touching Database files (`Data`, `Migrations`, `Persistence`, `Configurations`, `Repositories`, `Entities`, `DbContext`, `.sql`) or Security files (`Auth`, `Security`, `Identity`, `Jwt`, `Policies`, `PasswordHasher`, `TokenService`, `OAuth`, `RateLimit`), or carrying `database` or `security` labels, automatically places **both** `abdulrahmanhossam` and `abdulla7mohamed` in Reviewers (excluding the PR author) and assigns both in Assignees.
2. **General PRs (Round-Robin):** All other PRs rotate evenly across the active maintainers (`abdulrahmanhossam`, `abdulla7mohamed`, `AhmedShaabanAl-Saidi`, `Asma-Elhadiedy`, `Mohammad-Metwally`), picking from those with the fewest currently open assigned PRs.
3. **Exempted maintainers:** `SarahRefaat19` (Eng. Sara), `Sroor000` (Eng. Sorour), and `Aya-Selim` (Eng. Aya Samir) remain part of the repository team and are excluded from automatic Reviewer and Assignee rotation.

## 5. PR governance and permissions

`.github/CODEOWNERS` and `.github/workflows/pr-governance.yml` (check name: `PR Governance`) enforce clear ownership boundaries:

- **Code Owner (`abdulrahmanhossam`):** Unrestricted permission to read, edit, push, approve, and merge across all files and all PRs at any time.
- **Assigned Maintainer on their assigned PR:** Full permission on that PR to push commits, update the branch, approve, and merge (either via GitHub UI or by commenting `/merge` once checks pass).
- **Unassigned Maintainers on other PRs:** May submit code reviews (`APPROVE` or comments). They are blocked by `PR Governance` from pushing commits to another maintainer's PR branch or merging a PR not assigned to them.

## 6. Reference docs

- [`README.md`](README.md) — project overview and quick start
- [`ARCHITECTURE.md`](ARCHITECTURE.md) — layer responsibilities
- [`docs/ARCHITECTURE.md`](docs/ARCHITECTURE.md) — technical conventions
- [`docs/FEATURE_INDEX.md`](docs/FEATURE_INDEX.md) — implementation status
- [`docs/PROJECT_STRUCTURE.md`](docs/PROJECT_STRUCTURE.md) — repository layout
