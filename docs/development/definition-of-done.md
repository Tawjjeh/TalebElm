# Definition of done

A TalebElm change is done when all of these hold:

- [ ] Scope matches the issue and acceptance criteria.
- [ ] Code builds (`dotnet build TalebElm.slnx`).
- [ ] Tests pass (`dotnet test TalebElm.slnx`).
- [ ] Docs updated in the same PR when behavior changes.
- [ ] No secrets, tokens, real connection strings, hostnames, or customer
      data were committed.
- [ ] `.local/` stays uncommitted and unlinked from public docs.
- [ ] Product rules from `docs/business/BUSINESS_RULES.md` and
      `docs/OPEN_DECISIONS.md` are intact (or recorded changes are documented).

## Extra gate for code changes

Every code change needs a beginner-friendly Arabic explanation under
`.local/learn/NNN-<topic>.ar.md` before the work is called done. This file
is never committed or linked from public docs.
