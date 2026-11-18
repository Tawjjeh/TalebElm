# Contributing to TalebElm

This page covers setup, the path from issue to merge, and what a pull request must contain. Coding
rules live in [`docs/development/coding-standards.md`](docs/development/coding-standards.md), and
"done" is defined in [`docs/development/definition-of-done.md`](docs/development/definition-of-done.md).

## 1. Set up

You need Git, the [.NET 10 SDK](https://dotnet.microsoft.com/download), and Python 3.

```bash
git clone https://github.com/Tawjjeh/TalebElm.git
cd TalebElm
dotnet restore TalebElm.slnx && dotnet build TalebElm.slnx && dotnet test TalebElm.slnx
python3 scripts/check-docs.py
```

All of them must pass on a fresh clone. If one fails, say so in an issue instead of working around
it. Database setup, migration commands, and common errors are in
[`docs/development/getting-started.md`](docs/development/getting-started.md).

## 2. Get the green light

Work starts only when an issue has `help wanted` and is assigned to you.

1. Find an open issue. Start with
   [`good first issue` + `help wanted`](https://github.com/Tawjjeh/TalebElm/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22+label%3A%22help+wanted%22+no%3Aassignee).
2. Comment with your plan: what you will change, which files you expect to touch, and the test you
   will add.
3. Wait for a reply and an assignment, and check that nobody else is assigned.

If the issue is unclear, ask in the issue. If you found a bug that has no issue, open one with the
steps to reproduce it. A pull request for work nobody approved may be closed without review.

## 3. Branch and commit

Work on a fork; `main` is protected.

```bash
git clone https://github.com/<YourUsername>/TalebElm.git
cd TalebElm
git remote add upstream https://github.com/Tawjjeh/TalebElm.git
git fetch upstream
git switch -c feat/track-enrollment-endpoint upstream/main

git commit -m "feat: add track enrollment endpoint"
git push --set-upstream origin feat/track-enrollment-endpoint
```

Use one of these branch prefixes: `feat/`, `fix/`, `docs/`, `test/`, `refactor/`, `chore/`,
`ci/`. Commit subjects use the same prefixes and the imperative mood, for example
`fix: reject an empty exam title`.

## 4. What the pull request must contain

- **One slice of work.** One endpoint, one fix, one migration. Around 200 changed lines, tests
  included, is easy to review.
- **Tests that prove the behavior** and would fail without the change. Use SQLite for EF Core
  mapping. Never call a real external API in a test.
- **No secrets and no database files.** No tokens, passwords, connection strings, or `*.db` files.
- **Docs updated in the same pull request** when behavior, a route, a rule, or a status marker
  changes.
- **The issue linked** with `Closes #N` in the description, so it closes when the pull request is
  squashed.
- **The template filled in honestly:** summary, layer, related issue, and the checklist.

## 5. Review and merge

Opening a pull request that is not a draft asks one reviewer to look at it automatically; you do
not need to request anyone. `main` requires a passing `build-and-test` check, one approving review,
and all conversations resolved.

Push more commits to the same branch when you get feedback. Do not force-push while a review is
open; it erases the reviewer's line notes. Merging is a squash, so one pull request becomes one
commit on `main`.

## 6. Where to ask

- Questions and ideas: [Discussions](https://github.com/Tawjjeh/TalebElm/discussions).
- Bugs and documentation errors: the issue templates.
- Security problems: [`SECURITY.md`](SECURITY.md), never a public issue.
- Unfriendly behavior: [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md).

## Do not

- Start work before the issue is assigned to you.
- Mix unrelated changes into one pull request.
- Commit secrets, connection strings, or `*.db` files.
- Reformat files you did not need to change.
