# Contributing to TalebElm

This page explains how to set up the project, claim an issue, and get a pull request merged.

## 1. Set up

You need Git, the [.NET 10 SDK](https://dotnet.microsoft.com/download), and Python 3.

```bash
dotnet restore TalebElm.slnx
dotnet build TalebElm.slnx
dotnet test TalebElm.slnx
python3 scripts/check-docs.py
```

All four commands must pass before you start. If one fails on a fresh clone, tell us in an issue
instead of working around it.

Setup details, migration commands, and common errors are in
[`docs/development/getting-started.md`](docs/development/getting-started.md).

## 2. Claim an issue

1. Pick an open issue. `good first issue` and `help wanted` are the easiest ones.
2. Comment on it with your plan: what you will change and which files you expect to touch.
3. Wait until a maintainer assigns it to you. This stops two people from writing the same fix.
4. Branch from an up-to-date `main` only after the issue is assigned.

If the issue is unclear, ask in the issue before writing code. If you find a bug that has no
issue, open one with the steps to reproduce it.

## 3. Branch, commit, push

Work on a fork. `main` is protected and does not accept direct pushes.

```bash
git clone https://github.com/<YourUsername>/TalebElm.git
cd TalebElm
git remote add upstream https://github.com/Tawjjeh/TalebElm.git

git checkout main
git fetch upstream
git merge upstream/main

git checkout -b feat/track-enrollment-endpoint
```

Name the branch with one of these prefixes: `feat/`, `fix/`, `docs/`, `refactor/`, `test/`,
`chore/`, `ci/`.

Commit messages start with the same prefix:

```bash
git commit -m "feat: add track enrollment endpoint"
git push --set-upstream origin feat/track-enrollment-endpoint
```

## 4. What a pull request must contain

- **One slice of work.** One endpoint, one fix, one migration. A pull request around 200 changed
  lines, tests included, is easy to review.
- **Tests that prove the behavior.** Unit tests for logic, SQLite tests for EF Core mapping. Never
  call a real external API in a test.
- **No secrets and no database files.** No connection strings, tokens, passwords, or `*.db` files
  in the diff.
- **Docs updated when behavior changes.** If an endpoint, rule, or status changes, update the
  matching file in [`docs/`](docs/README.md) in the same pull request.
- **A linked issue** when one exists: write `Closes #123` in the description.

Keep each layer in its place:

- `Domain` has no project references.
- `Application` uses `IUnitOfWork`; it never touches `DbContext`.
- `Infrastructure` owns EF Core and external HTTP clients, and holds no business rules.
- `Api` controllers stay thin and call Application services.

The reasoning behind these rules is in [`ARCHITECTURE.md`](ARCHITECTURE.md).

## 5. Review and merge

When you open a pull request that is not a draft, the repository asks one maintainer to review it
automatically. You do not need to ask for a reviewer yourself.

The reviewer checks four things:

1. `build-and-test` passes.
2. The change does what the linked issue asked, and nothing more.
3. Tests cover the new behavior and would fail without the change.
4. Docs and layer boundaries are respected.

Then the reviewer approves, resolves the conversations, and merges with **Squash and merge**. One
pull request becomes one commit on `main`.

If the reviewer asks for changes, push more commits to the same branch. Do not force-push while a
review is open; it erases the reviewer's line notes.

## 6. Reporting problems

- Security issues: follow [`SECURITY.md`](SECURITY.md) and do not open a public issue.
- Behavior that feels wrong: open a bug issue with the request, the expected result, and the
  actual result.
- Unfriendly behavior: report it to the maintainers, see [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md).
