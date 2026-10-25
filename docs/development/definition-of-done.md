# Definition of Done

A TalebElm change is done when **all** of the following hold:

---

## Code and behavior

- [ ] Scope matches the linked issue and its acceptance criteria.
- [ ] `dotnet build TalebElm.slnx` succeeds with **zero errors and zero new warnings**.
- [ ] `dotnet test TalebElm.slnx` passes; all new behavior is covered by tests (see [`testing.md`](testing.md)).
- [ ] Architecture boundaries are respected: Application depends only on `IUnitOfWork`, controllers only call service interfaces, Infrastructure contains no business logic.
- [ ] No new EF Core configuration is missing a corresponding migration, and the migration is reviewed as code.

---

## Documentation

- [ ] `docs/FEATURE_INDEX.md` status markers are accurate (`Implemented` / `Target` / `Proposed`) after the change lands.
- [ ] `docs/ARCHITECTURE.md` or `docs/development/` are updated if a new convention, layer rule, or pattern is introduced.
- [ ] `docs/business/BUSINESS_RULES.md` or `docs/OPEN_DECISIONS.md` are updated if a business rule is added, changed, or resolved.
- [ ] `python3 scripts/check-docs.py` passes (no broken relative links in `docs/`).
- [ ] Docs are updated in the **same PR** as the code change — not in a follow-up.

---

## Security and data

- [ ] No secrets, tokens, real connection strings, hostnames, or personal data are committed.
- [ ] `*.db`, `*.db-shm`, `*.db-wal` files are not staged.
- [ ] `PasswordHash`, `IsCorrect` (exam answer key), and any other server-only fields are not serialized in learner-facing response DTOs.

---

## Pull request hygiene

- [ ] PR title uses conventional-commit format: `feat: …`, `fix: …`, `docs: …`, etc.
- [ ] `feat:` and `fix:` PRs include `Closes #NNN` so the issue closes automatically on merge.
- [ ] The change is scoped to one logical concern; unrelated changes are in a separate PR.
- [ ] The PR is opened against `main` from a branch with a `<type>/` prefix (`feat/`, `fix/`, `docs/`, `refactor/`, `test/`, `chore/`, `ci/`).

---

## CI gates (automated)

All of the following must pass before merge:

```text
dotnet build TalebElm.slnx              ← zero errors, zero warnings
dotnet test TalebElm.slnx -c Release   ← all tests pass
python3 scripts/check-docs.py          ← no broken links
PR title: conventional-commit format   ← validated by pr-validation.yml
Security / vulnerability scan          ← no unapproved High/Critical findings
```
