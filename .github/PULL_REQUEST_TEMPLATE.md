## Summary

<!-- Explain what changed, why, and link the issue (e.g., Closes #123). -->

## Layer and Scope

- Layer(s) modified: <!-- Domain / Application / Infrastructure / Api / Tests / Docs -->
- Claimed issue: <!-- Closes #123; comment your plan on the issue before opening this pull request -->

## Checklist

The full list is in the [definition of done](https://github.com/Tawjjeh/TalebElm/blob/main/docs/development/definition-of-done.md).

- [ ] I commented on the issue before opening this pull request
- [ ] The pull request covers one vertical slice and touches only the files it needs
- [ ] `dotnet build TalebElm.slnx` passes with 0 errors and 0 warnings
- [ ] `dotnet test TalebElm.slnx` passes with tests covering the change
- [ ] `python3 scripts/check-docs.py` passes (if Markdown files changed)
- [ ] Changes respect Clean Architecture boundaries (`Domain` depends on nothing; `Application` uses `IUnitOfWork`)
- [ ] No secrets, connection strings, or `*.db` files are included
