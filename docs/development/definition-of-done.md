# Definition of done

A TalebElm change is done when all of these hold:

- [ ] Scope matches the issue and acceptance criteria.
- [ ] Code builds (`dotnet build TalebElm.slnx`).
- [ ] Tests pass (`dotnet test TalebElm.slnx`).
- [ ] Docs updated in the same PR when behavior changes.
- [ ] No secrets, tokens, real connection strings, hostnames, or customer
      data were committed.
- [ ] Product rules from `docs/business/BUSINESS_RULES.md` and
      `docs/OPEN_DECISIONS.md` are intact (or recorded changes are documented).

