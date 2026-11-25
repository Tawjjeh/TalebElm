# Contributing to TalebElm

Welcome to TalebElm (طالب علم). We build an open learning platform for software engineers with clear architecture, server-graded exams, and automated verification. Every accepted pull request helps keep that path reliable and accessible for everyone.

This guide walks you through finding work, setting up your environment, making your change, and getting it merged. We want contributing to feel clear, friendly, and respectful of your time.

## Finding work

We organize work into six milestones (M0 through M5). Ready issues have a milestone, no assignee, and no open pull request:

```text
is:issue is:open no:assignee -linked:pr -no:milestone NOT "[Tracking]" in:title
```

Browse ready issues with that query on GitHub:
https://github.com/Tawjjeh/TalebElm/issues?q=is%3Aissue+is%3Aopen+no%3Aassignee+-linked%3Apr+-no%3Amilestone+NOT+%22%5BTracking%5D%22+in%3Atitle

If you are new to the project, start with `good first issue`:
https://github.com/Tawjjeh/TalebElm/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22+no%3Aassignee+-linked%3Apr

These are small, isolated tasks such as adding missing unit tests or fixing documentation discrepancies.

## Claiming an issue

We use a claim-by-comment workflow:

1. Leave a short comment on the issue describing what you plan to change and which files you will touch.
2. Start working right away. You do not need to wait for anyone to assign the issue to you.
3. Open a pull request within 7 days. If you need more time or run into questions, leave an update on the issue thread.
4. If an assigned issue stays idle for more than 14 days without an open pull request or branch, we clear the assignment so someone else can pick it up.

To keep things fair and manageable, claim at most two issues at a time. The first pull request that meets the architectural rules and passes all checks wins; if two arrive for the same issue, the earlier one is reviewed first.

## Local setup

You will need Git, Python 3, and the [.NET 10 SDK](https://dotnet.microsoft.com/download).

1. Fork the repository and clone your fork:
   ```bash
   git clone https://github.com/<your-username>/TalebElm.git
   cd TalebElm
   git remote add upstream https://github.com/Tawjjeh/TalebElm.git
   ```

2. Verify that everything builds and tests pass cleanly on main before making any edits:
   ```bash
   dotnet build TalebElm.slnx
   DOTNET_ROLL_FORWARD=Major dotnet test TalebElm.slnx
   python3 scripts/check-docs.py
   ```

   These commands work identically on Linux, macOS, and Windows.

## Branching and vertical slices

Always branch from fresh `upstream/main`:

```bash
git fetch upstream
git switch -c feat/my-topic upstream/main
```

Use descriptive branch prefixes based on what you are changing:
- `feat/<topic>` for new capabilities or endpoints
- `fix/<topic>` for bug fixes
- `docs/<topic>` for documentation changes
- `test/<topic>` for test additions or test harness improvements
- `refactor/<topic>` for refactoring approved by an open decision

Keep each pull request to one vertical slice. Aim for roughly 200 lines of changes including tests. Touch only the files needed for that slice; avoid unrelated formatting or drive-by cleanups.

## Architecture and code boundaries

TalebElm follows Clean Architecture with strict inward dependency rules:
- **Domain (`src/TalebElm.Domain`)**: Entities, enums, exceptions, repository interfaces, `IUnitOfWork`. Depends on nothing.
- **Application (`src/TalebElm.Application`)**: Service abstractions and concrete services, DTOs, FluentValidation validators, client interfaces. Never takes a direct `DbContext`; it uses `IUnitOfWork`.
- **Infrastructure (`src/TalebElm.Infrastructure`)**: EF Core `AppDbContext`, entity configurations, migrations, repository implementations, typed HTTP clients. Holds persistence only, no business logic.
- **Api (`src/TalebElm.Api`)**: Controllers, middleware, dependency injection composition roots in `Program.cs`.
- **Tests (`tests/TalebElm.Tests`)**: Unit tests and SQLite integration tests.

Every new asynchronous method across services and repositories must accept `CancellationToken ct = default` as its last parameter and forward it throughout.

## Tests and documentation

Every behavioral change requires tests that prove it. The test should fail before your change and pass after it. Unit tests live alongside existing suites; integration tests use SQLite in-memory or throwaway instances.

When you change an API contract, add a route, or alter business behavior, update the corresponding documentation under `docs/` in the same pull request. Keep existing progress markers (`Implemented`, `Target`, `Proposed`) accurate.

Run `python3 scripts/check-docs.py` before pushing. It checks that all markdown links, anchors, and references resolve cleanly.

## Commit style

We use Conventional Commits in the imperative mood:
- `feat: add Track publish endpoint`
- `fix: prevent duplicate enrollment on published track`
- `docs: document unlock dual gate rules`
- `test: verify exam retake score preservation`
- `refactor: consolidate service registrations in Application`

Keep commit messages plain and descriptive. Do not include generated-by footers, co-author lines, or filler.

## Opening a pull request

When your branch is ready:
1. Push your branch to your GitHub fork:
   ```bash
   git push -u origin feat/my-topic
   ```
2. Open a pull request against `Tawjjeh/TalebElm` branch `main`.
3. In the PR body, link the issue using `Closes #123`.
4. Summarize what changed, why, and how you verified it.
5. Ensure all CI checks pass. If a check fails, inspect the log and push fixes to your branch.

## Review and merge process

Here is what to expect after opening your pull request:
- **Timely reviews**: I review open pull requests within 48 hours.
- **Constructive feedback**: Reviews focus on architectural boundaries, test coverage, and correctness. Feedback is specific and actionable.
- **Iterative updates**: If changes are requested, push additional commits to your branch. There is no need to close the PR or force-push.
- **Squash merge**: Once approved and checks pass, your change is squash-merged into `main` as a single clean commit with your name as author.

## Questions and community

- Questions about the architecture, roadmap, or ideas: [GitHub Discussions](https://github.com/Tawjjeh/TalebElm/discussions).
- Questions about a specific task: Comment directly on the issue thread.
- Security vulnerabilities: Follow [`SECURITY.md`](SECURITY.md) and report privately. Do not open public issues for security concerns.

## Guidelines and boundaries

- Never trust client-supplied scores, handles, or problem completion flags.
- Never return `IsCorrect` in any learner-facing response or DTO.
- Never commit database files (`*.db`), secrets, connection strings, or personal access tokens.
- Do not modify `.github/workflows/` without prior coordination on an issue.
- Respect the Clean Architecture boundaries at all times.

Thank you for contributing to TalebElm!
