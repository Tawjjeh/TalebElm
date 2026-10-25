# TalebElm — Testing Guide

> Status: current as of 2026-10-25.
> All tests run in-process with SQLite. No external services, Docker, or network access are required.

---

## 1. Test layout

```text
tests/TalebElm.Tests/
├── UnitTests/          Entity invariants, DTO contracts, validators, service logic
├── IntegrationTests/   Controller and cross-layer relational-constraint coverage
└── Infrastructure/     SQLite-backed repository and configuration tests
```

| Scope | What it covers |
|---|---|
| **Unit** | Entity invariants, DTO shape, FluentValidation rules, Application service logic with mocked `IUnitOfWork` |
| **Repository** | SQLite-backed persistence: EF Core configurations, unique indexes, `DeleteBehavior.Restrict`, relationship behavior |
| **Integration** | Controller actions, cross-layer behavior, relational constraints over a real SQLite schema |

---

## 2. Running tests

```bash
# All tests (debug configuration)
dotnet test TalebElm.slnx

# Release mode — matches CI
dotnet test TalebElm.slnx --configuration Release

# Single project
dotnet test tests/TalebElm.Tests

# Filter by name pattern
dotnet test TalebElm.slnx --filter "FullyQualifiedName~TrackService"
```

All tests run without network access or external processes. Failed tests must produce a readable failure message that identifies the exact assertion. Do not claim tests passed unless you ran the command and read the output.

---

## 3. Unit test expectations

Unit tests have **no I/O**. They use `Moq` to mock `IUnitOfWork` and individual repositories.

| Rule | Detail |
|---|---|
| Mock at the `IUnitOfWork` boundary | Do not mock `AppDbContext` or EF internals |
| Cover the happy path and at least one failure path | Happy + one meaningful failure = minimum |
| Test one behavior per `[Fact]` | Do not combine unrelated assertions |
| Use descriptive names | `CreateTrack_WithValidInput_ReturnsCreatedTrack` |
| No time-sensitive code | Inject `TimeProvider` or test with a fixed UTC instant |

Example test structure:

```csharp
[Fact]
public async Task GetByIdAsync_WhenTrackDoesNotExist_ThrowsNotFoundException()
{
    // Arrange
    var uow = new Mock<IUnitOfWork>();
    uow.Setup(u => u.Tracks.GetByIdAsync(It.IsAny<Guid>()))
       .ReturnsAsync((Track?)null);
    var service = new TrackService(uow.Object);

    // Act & Assert
    await Assert.ThrowsAsync<NotFoundException>(
        () => service.GetByIdAsync(Guid.NewGuid()));
}
```

---

## 4. Repository and configuration test expectations

Repository tests use a real SQLite database created for each test run. They verify:

- EF Core column constraints (`HasMaxLength`, `IsRequired`, `HasDefaultValue`).
- Unique index enforcement (`UNIQUE(TrackId, Order)`, `UNIQUE(ModuleId)`, etc.).
- `DeleteBehavior.Restrict` — blocked deletes throw a `ConflictException` (via `UnitOfWork`), not an EF exception.
- Relationship navigation (e.g., loading Module with its Lessons).

Rules:

- Every `IEntityTypeConfiguration<T>` class must have at least one test that inserts a valid entity and one that violates a key constraint.
- Migration guard test: confirm the latest migration matches the current model snapshot; fail loudly if the snapshot is stale.

---

## 5. Integration test expectations

Integration tests exercise the full HTTP pipeline over a real SQLite database. They verify:

- Controller routing and action return types.
- Domain exception → HTTP status mapping via `ExceptionHandlingMiddleware`.
- `409 Conflict` on duplicate enrollment or duplicate module order.
- `404 Not Found` on unknown resource IDs.
- `400 Bad Request` on invalid FluentValidation input.
- `501 Not Implemented` for placeholder actions (acceptable until the feature is wired).

Rules:

- Use `WebApplicationFactory<Program>` for HTTP integration tests.
- Seed only the data needed for the test scenario; clean up in teardown.
- Do not share mutable state between tests.

---

## 6. Coverage expectations

| Scope | Minimum expectation |
|---|---|
| Domain entity invariants | 100% of invariant rules have a test |
| Application service methods | Happy path + primary failure path per method |
| EF Core configurations | Each unique index and constraint tested |
| Controllers (target behavior) | One integration test per implemented endpoint |

Coverage is a **floor, not a goal**. A high-coverage test that asserts nothing is a defect.

---

## 7. Test data and security

- Never use real production data, emails, passwords, or tokens in tests.
- Generated test credentials are disposable and unique per test run.
- Test databases are created in memory or as temporary files and removed after the test run.
- Logs from failed tests must not reveal sensitive values.
- Do not call real external APIs (Codeforces, GitHub) from tests. Use fakes or stubs for any `ICodeforcesClient` or `IGitHubClient` implementations (target behavior; those interfaces do not exist yet).

---

## 8. Determinism and isolation rules

- No test depends on execution order.
- No shared mutable static state between tests.
- Use a fixed UTC instant when testing time-dependent behavior.
- Each test creates its own database context or SQLite file.
- No test calls a real external network endpoint.

---

## 9. CI test gates

Pull request checks:

```text
dotnet build TalebElm.slnx                 # zero warnings, zero errors
dotnet test TalebElm.slnx -c Release      # all tests pass
python3 scripts/check-docs.py             # no broken doc links
```

A pull request with any failing test, any build warning from new code, or a broken doc link must not be merged.

---

## 10. Definition of done for tests

A behavior change is done with respect to testing when:

- [ ] The happy path is covered by at least one test.
- [ ] At least one meaningful failure path is covered (e.g., `NotFoundException`, `ConflictException`, validation failure).
- [ ] If a new EF Core configuration was added, a repository test verifies the constraint.
- [ ] If a new controller action was wired, an integration test verifies the status code.
- [ ] `dotnet test TalebElm.slnx --configuration Release` passes with the new tests included.
- [ ] No test depends on a real external service.
- [ ] Test names are descriptive and follow `Method_Scenario_ExpectedBehavior` or equivalent.
