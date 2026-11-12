## Summary

<!-- Explain what changed, why, and link the issue (e.g., Closes #123). -->

## Layer and Scope

- Layer(s) modified: <!-- Domain / Application / Infrastructure / Api / Tests / Docs -->
- Related issue: <!-- Closes #123 -->

## Checklist

- [ ] `dotnet build TalebElm.slnx` passes with 0 errors and 0 warnings
- [ ] `dotnet test TalebElm.slnx` passes with tests covering the change
- [ ] `python3 scripts/check-docs.py` passes (if Markdown files changed)
- [ ] Changes respect Clean Architecture boundaries (`Domain` depends on nothing; `Application` uses `IUnitOfWork`)
- [ ] No secrets, connection strings, or `*.db` files are included
