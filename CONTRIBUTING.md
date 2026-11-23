# Contributing to TalebElm

We are glad you want to contribute to طالب علم (TalebElm). Every accepted change helps build a clear, open path for anyone learning software engineering.

## Start here

1. **Fork and clone.**
   ```bash
   gh repo fork Tawjjeh/TalebElm --clone=false
   git clone https://github.com/<your-username>/TalebElm.git
   cd TalebElm
   git remote add upstream https://github.com/Tawjjeh/TalebElm.git
   ```
2. **Build and test before you change anything.**
   ```bash
   dotnet build TalebElm.slnx
   dotnet test TalebElm.slnx
   python3 scripts/check-docs.py
   ```
3. **Pick a ready issue.** It must be open, carry an M0–M5 milestone, have no assignee, and have no open pull request. Small ones carry [`good first issue`](https://github.com/Tawjjeh/TalebElm/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22+no%3Aassignee).
4. **Claim it in a comment.** Say what you plan to change and where. Wait until an owner assigns the issue to you, then start. One issue has one pull request; link it with `Closes #N`.
5. **Branch from `upstream/main`.**
   ```bash
   git fetch upstream
   git switch -c feat/my-topic upstream/main
   ```

## A good pull request

- **One focused change.** Touch only the files that task needs, and keep the diff near 200 lines.
- **Tests that prove it.** The new test must fail without your change.
- **Docs in the same pull request.** Update the feature doc, table, or rule marker when behavior or a contract changes.
- **Conventional title.** `feat:`, `fix:`, `docs:`, `test:`, `refactor:`, `chore:`, or `ci:`.
- **Accurate description.** What changed, why, how you verified it, and anything still open.

## Checks and review

CI runs `dotnet build`, `dotnet test`, the OpenAPI validation, and `python3 scripts/check-docs.py`. An assigned reviewer reads the change against the architecture rules and the recorded decisions. When the checks pass and the review is approved, an owner squash-merges the branch into `main` as one commit.

If a review asks for changes, reply on the thread and push a follow-up commit. Do not force-push over someone else's branch, and do not reopen a pull request that was closed for a duplicate.

## Where to ask

- A question about the project or an idea: open a [Discussion](https://github.com/Tawjjeh/TalebElm/discussions).
- A blocked task or an unclear rule: comment on your issue thread.
- A security problem: follow [`SECURITY.md`](SECURITY.md) and do not open a public issue.

## Please don't

- Do not open unsolicited refactoring, reformatting, or cosmetic pull requests.
- Do not change `.github/workflows` without agreeing on an issue first.
- Do not trust client input for scores, handles, solves, or task results.
- Do not commit databases, secrets, or editor user files.

Read [`docs/development/coding-standards.md`](docs/development/coding-standards.md) for the layer rules, [`docs/development/definition-of-done.md`](docs/development/definition-of-done.md) for completion, and [`docs/GITHUB_LABELS.md`](docs/GITHUB_LABELS.md) for the label scheme.
