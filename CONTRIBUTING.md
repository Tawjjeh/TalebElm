# Contributing to TalebElm

We are glad you want to help build TalebElm. Every contribution helps make open-source software engineering education more rigorous and accessible.

## Your first contribution

Follow these five steps from finding an issue to submitting your change:

1. **Find an open issue:** Look for issues labeled `help wanted`, or filter by [`good first issue`](https://github.com/Tawjjeh/TalebElm/issues?q=is%3Aissue+is%3Aopen+label%3A%22good+first+issue%22+no%3Aassignee) if you are new to the repository.
2. **Ask to take it:** Leave a comment on the issue describing your planned approach, which layer you will touch, and what test you will add.
3. **Wait for assignment:** Work begins only after an owner assigns the issue to you and confirms your plan. Check that nobody else is assigned before starting.
4. **Create a small branch with tests:** Fork the repository, create a branch from latest `upstream/main`, write your code and tests, and run local verification:
   ```bash
   git fetch upstream
   git switch -c feat/module-unlock upstream/main
   dotnet build TalebElm.slnx
   dotnet test TalebElm.slnx
   python3 scripts/check-docs.py
   ```
   To test the API locally in development:
   ```bash
   dotnet run --project src/TalebElm.Api
   curl http://localhost:5236/api/health
   ```
5. **Submit your pull request:** Commit with a conventional prefix (`feat:`, `fix:`, `docs:`, `test:`), push to your fork, and open a PR:
   ```bash
   git commit -m "feat(application): implement module unlock rule (#123)"
   git push -u origin feat/module-unlock
   ```

## The green-light rule

Work begins only after an issue is assigned to you.
Unassigned pull requests will be closed kindly without review so contributors do not spend time on duplicate or overlapping work.
If you notice a bug or want to propose a change that does not have an open issue yet, open an issue first and wait for discussion.

## What a good pull request looks like

- **Focused topic:** One slice of work per pull request, touching only the files required for that change.
- **Small footprint:** Keep the diff under about 200 lines including tests so reviews can be timely and thorough.
- **Automated tests:** Every logic change includes unit or integration tests that prove the behavior and would fail without it.
- **Passing checks:** `dotnet build`, `dotnet test`, and `python3 scripts/check-docs.py` pass cleanly with zero warnings or errors.
- **Linked issue:** The description includes `Closes #N` so the corresponding issue closes automatically upon squash-merge.

## What happens next

- Automated CI runs the test suite and verifies documentation formatting on every commit pushed to your branch.
- A maintainer reviews your code, checks architectural boundaries, and leaves actionable line comments if changes are needed.
- Once checks pass and the review is approved, an owner squash-merges your branch into `main` as a single commit.
- Contributors do not merge their own pull requests; maintainers handle squashing and closing.

## Ask for help

If you have questions about architecture or need help with a failing test, ask directly on your issue thread or start a topic in [Discussions](https://github.com/Tawjjeh/TalebElm/discussions).

## Please don't

- Do not open unsolicited refactoring pull requests, reformatting changes, or cosmetic rewrites of existing code.
- Do not modify files under `.github/workflows` without explicit maintainer agreement on an issue first.
- Do not open promotional, marketing, or machine-generated placeholder pull requests.

## Standards and references

- **Coding standards:** [`docs/development/coding-standards.md`](docs/development/coding-standards.md) details layer boundaries and validation patterns.
- **Definition of done:** [`docs/development/definition-of-done.md`](docs/development/definition-of-done.md) covers PR acceptance criteria.
- **Labeling rules:** [`docs/GITHUB_LABELS.md`](docs/GITHUB_LABELS.md) documents the 14-label scheme and categorization conventions.
- **Local environment:** [`docs/LOCAL_SETUP.md`](docs/LOCAL_SETUP.md) provides database connection strings and migration instructions.
- **Getting started:** [`docs/development/getting-started.md`](docs/development/getting-started.md) walks through the full local dev environment setup.
- **Security:** Report vulnerabilities privately according to [`SECURITY.md`](SECURITY.md).
- **Code of Conduct:** All participation is governed by our [`CODE_OF_CONDUCT.md`](CODE_OF_CONDUCT.md).
