# Pass CancellationToken through the exam read path

## Summary

`ExamService.GetByIdAsync` and `IExamRepository` perform database reads without accepting a
`CancellationToken`, so an aborted HTTP request still finishes its database queries. The project
coding standard requires every new async stack to propagate `CancellationToken` from the controller
down to EF Core, and the maintainers have been fixing this path-by-path: Module (#432), Lesson
(#433), UserProgress (#434), User (#435) and Track enrollment (#436) issues exist, but no issue
covers the Exam read path. This issue fills exactly that gap with the same small, well-trodden
change.

## Current Behavior

- `src/TalebElm.Application/Services/Concrete/ExamService.cs` — `GetByIdAsync(Guid id)` (line 16)
  takes no token and calls `unitOfWork.Exams.GetByIdAsync(id)` without one.
- `src/TalebElm.Application/Services/Abstraction/IExamService.cs` — `GetByIdAsync(Guid id)`
  declares no token parameter.
- `src/TalebElm.Domain/Interfaces/IRepository.cs` — `GetByIdAsync(Guid id)` has no token
  parameter (line 7).
- `src/TalebElm.Domain/Interfaces/IExamRepository.cs` — `GetByModuleIdAsync(Guid moduleId)` has
  no token parameter (line 7).
- `src/TalebElm.Infrastructure/Persistence/Repositories/ExamRepository.cs` — `GetByIdAsync`
  (line 20) and `GetByModuleIdAsync` (line 25) pass no token to EF Core
  (`FindAsync(id)`, `FirstOrDefaultAsync(...)`).
- `src/TalebElm.Domain/Interfaces/IUnitOfWork.cs` and
  `src/TalebElm.Infrastructure/Persistence/Repositories/UnitOfWork.cs` — `SaveChangesAsync()`
  (line 13 / line 46) has no token parameter.

Aborting a request cannot stop this work; the queries and any later save run to completion.

## Expected Behavior

The token flows through the whole exam read path. When the caller aborts, EF Core stops the
database work via `ct`. No behavior changes: adding the parameter is purely additive because it
defaults to `CancellationToken.None`.

## Proposed Scope

- Add `CancellationToken ct = default` to `IExamService.GetByIdAsync` and pass it through
  `ExamService.GetByIdAsync` to `unitOfWork.Exams.GetByIdAsync(id, ct)`.
- Add `CancellationToken ct = default` to `IRepository<T>.GetByIdAsync`,
  `IExamRepository.GetByModuleIdAsync` and `IUnitOfWork.SaveChangesAsync`.
- Implementations under `src/TalebElm.Infrastructure/Persistence/Repositories/` that implement
  `IRepository<T>` take the same optional parameter and forward it to EF Core
  (`FindAsync(id, ct)`, `FirstOrDefaultAsync(..., ct)`); `UnitOfWork.SaveChangesAsync(ct)` passes
  `ct` to `_context.SaveChangesAsync(ct)`. Repositories not used here (`ExamRepository.GetAllAsync`
  sibling methods, etc.) appear only because of the shared interface signature, as PR #453 did for
  enrollment.
- Update test doubles and callers so they compile (the optional parameter keeps existing callers
  valid, but mocks asserting arguments may need updating, mirroring
  `tests/TalebElm.Tests/UnitTests/ExamServiceTests.cs` and `ExamRepositoryTests.cs`).
- Do not touch any controller or endpoint. This is the Application/Domain/Infrastructure slice
  only, like the sibling issues.

Follow the pattern of Decision 39 and the sibling issues above: one token per path, default value
`CancellationToken.None`, no behavior change.

## Acceptance Criteria

- [ ] `IExamService.GetByIdAsync`, the repositories in the exam path, and
      `IUnitOfWork.SaveChangesAsync` accept `CancellationToken` and forward it to every async call
      they make.
- [ ] An added unit test proves the token received by the service reaches the repository call (or
      the existing tests are updated to verify forwarding where mocking infrastructure allows).
- [ ] `dotnet build TalebElm.slnx` and `dotnet test TalebElm.slnx` pass with no behavior change.

## Verification

- `dotnet build TalebElm.slnx` and `dotnet test TalebElm.slnx`.
- Filtered example used by the sibling enrollment PR:
  `dotnet test TalebElm.slnx --filter FullyQualifiedName~ExamServiceTests`.
- Manual check: grep for `ExamService.cs`, `ExamRepository.cs`, `IUnitOfWork.cs` and confirm no
  `await` call in the exam read path drops the token.

## Repository Evidence

- Relevant files:
  - `src/TalebElm.Application/Services/Abstraction/IExamService.cs`
  - `src/TalebElm.Application/Services/Concrete/ExamService.cs`
  - `src/TalebElm.Domain/Interfaces/IRepository.cs`, `IExamRepository.cs`, `IUnitOfWork.cs`
  - `src/TalebElm.Infrastructure/Persistence/Repositories/ExamRepository.cs`, `UnitOfWork.cs`
- Relevant classes/methods: `ExamService.GetByIdAsync`, `ExamRepository.GetByIdAsync`,
  `ExamRepository.GetByModuleIdAsync`, `IRepository<T>.GetByIdAsync`, `IUnitOfWork.SaveChangesAsync`
- Existing project convention to follow: `docs/development/coding-standards.md` §8
  ("New code must propagate CancellationToken through the entire call stack"); sibling issues
  #432–#436 (same change per read path) and open PR #453 for the enrollment path; tests live in
  `tests/TalebElm.Tests/UnitTests/` (`ExamServiceTests.cs`, `ExamRepositoryTests.cs`).

## Difficulty

Beginner

## Estimated Effort

About 1 hour: a handful of small signature changes plus an updated test — very similar in size to
the sibling read-path issues.

## Duplicate Check

GitHub API access was available (the repository `Tawjjeh/TalebElm` is public; 106 open issues,
23 open PRs at the time of searching). I searched for duplicates with the queries
`repo:Tawjjeh/TalebElm is:open CancellationToken` and
`repo:Tawjjeh/TalebElm is:open CancellationToken exam`:

- Issues #432 (Module), #433 (Lesson), #434 (progress), #435 (user), #436 (enrollment) cover the
  other read paths — none mention the Exam read path.
- Open PR #453 covers enrollment, PR #459 covers the progress path; neither touches the Exam path.
- No open issue claims the exam read path; I did not find a possible duplicate. (Searches are
  lexical, so a very differently-worded issue could still exist; the closest matches are listed.)
