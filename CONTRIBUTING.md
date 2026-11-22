# Contributing to TalebElm

We are glad you want to contribute to طالب علم (TalebElm). Every accepted change helps build a clear, open path for anyone learning software engineering.

## Your first contribution

Follow these five steps to find an issue, set up your branch, and submit your change:

1. **Fork the repository:** Fork TalebElm to your personal GitHub account:
   ```bash
   gh repo fork Tawjjeh/TalebElm --clone=false
   ```
2. **Clone your fork:** Clone your fork locally and configure the upstream remote:
   ```bash
   git clone https://github.com/<your-username>/TalebElm.git
   cd TalebElm
   git remote add upstream https://github.com/Tawjjeh/TalebElm.git
   ```
3. **Build and test:** Confirm the solution builds and all existing tests pass:
   ```bash
   dotnet build TalebElm.slnx
   dotnet test TalebElm.slnx
   python3 scripts/check-docs.py
   ```
4. **Pick an issue:** Pick an issue with the `help wanted` label that nobody has taken (or start with [`good first issue`](https://github.com/Tawjjeh/TalebElm/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22+no%3Aassignee)), comment that you want it, and start once it is assigned to you.
5. **Create a branch:** Fetch latest changes and create a branch from `upstream/main`:
   ```bash
   git fetch upstream
   git switch -c feat/my-topic upstream/main
   ```

## Contribution rule

Pick an issue with the help wanted label that nobody has taken, comment that you want it, and start once it is assigned to you.
Please link the issue with `Closes #N` in your pull request description to keep work organized and avoid duplicate effort.

## A good pull request

- **One focused change:** Touch only the files needed for that single task, keeping diffs under about 200 lines.
- **Automated tests:** Include unit or integration tests proving the new behavior that would fail without your change.
- **Docs updated:** Update relevant guides or index tables if behavior, arguments, or contracts changed.
- **Linked issue:** Include `Closes #N` in the description so the issue closes on merge.
- **Conventional title:** Prefix the title with a conventional type such as `feat:`, `fix:`, `docs:`, or `test:`.

## What happens next

- Automated CI builds the code, runs the test suite, and checks doc formatting on every push.
- An assigned reviewer reads your change, checks architecture rules, and leaves actionable comments.
- Once all checks pass and the review is approved, an owner merges your pull request.
- We squash-merge each branch into `main` as a single clean commit.

## Ask for help

If you run into an unexpected build failure or have a question about architecture, ask on your assigned issue thread or open a topic in [Discussions](https://github.com/Tawjjeh/TalebElm/discussions).

## Please don't

- Do not open unsolicited refactoring, code reformatting, or cosmetic changes.
- Do not edit files under `.github/workflows` without prior agreement on an issue.
- Do not open promotional PRs or submit changes without tests proving their correctness.

## Standards and labels

Read [`docs/development/coding-standards.md`](docs/development/coding-standards.md) for architectural boundaries and [`docs/development/definition-of-done.md`](docs/development/definition-of-done.md) for completion criteria.
Available issue and pull request labels are documented in [`docs/GITHUB_LABELS.md`](docs/GITHUB_LABELS.md).
