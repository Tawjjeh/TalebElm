# Coding standards

TalebElm targets .NET 10 with nullable contexts enabled, warnings-as-errors
preferred for new code. These standards extend `ARCHITECTURE.md`.

## Style

- File-scoped namespaces.
- One public type per file.
- Async methods end in `Async`, accept `CancellationToken`, and do not use
  `async void` outside event handlers.
- Express time as UTC; resolve clocks through injectable abstractions.
- Never call `.Result` or `.Wait()`; avoid fire-and-forget without a tracked
  background mechanism.

## Boundaries

- `Application` depends on `IUnitOfWork`, never `DbContext`.
- Controllers are thin: bind input, call an Application service, translate to
  HTTP.
- EF Core mappings, migrations, and repositories live in `Infrastructure`.

## Persistence

- Migrations are explicit and reviewed before merge.
- Update `Missing` mapping or migration changes in the same PR.
- Use `Restrict` for learning-path foreign keys (see `docs/business/BUSINESS_RULES.md`).
- Use parameterised queries and input validation at the boundary.

## Errors

- Use the project's existing exception hierarchy (`DomainException` derived types).
- Do not leak provider details or user data in problem-detail responses.

## Validation and tests

- Add or update tests for every behavior change.
- Failed tests must be reported with the command and output.
- Do not claim tests passed unless they were run.
