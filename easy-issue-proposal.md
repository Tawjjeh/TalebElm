# Proposed Easy Issue: `CreateExamRequestValidator` must require a non-empty `ModuleId`

## Summary

`CreateExamRequestValidator` validates only `Title` and `PassThreshold`. The parent-id
rule that every sibling validator enforces — "the owning entity id must be non-empty" —
is missing for exams. The result: `CreateExamRequest(Guid ModuleId, ...)` accepts
`Guid.Empty` as a parent, and any exam-creation flow downstream (issue #273 / API
wiring #255) will report it as a service-level 404 "Module ... not found" instead of a
clean 400 with the standard error-code message. The fix is one three-line
`RuleFor(x => x.ModuleId).NotEmpty().WithMessage("Exam.ModuleId.Required")` plus a
unit test — the same shape the repo already accepted for the Module and Lesson
validators.

## Current Behavior

`src/TalebElm.Application/Validators/CreateExamRequestValidator.cs:8-15`:

```csharp
public CreateExamRequestValidator()
{
    RuleFor(x => x.Title)
        .NotEmpty().WithMessage("Exam.Title.Required");

    RuleFor(x => x.PassThreshold)
        .InclusiveBetween(1, 100).WithMessage("Exam.PassThreshold.Invalid");
}
```

- `CreateExamRequest` is declared as `CreateExamRequest(Guid ModuleId, string Title,
  int PassThreshold)` (`src/TalebElm.Application/DTOs/CreateExamRequest.cs:9`), so a
  client can omit `ModuleId` from the JSON body and get `Guid.Empty`.
- `ExamService.CreateAsync` is still a placeholder that throws the domain 501, so
  today the bad payload dies at 501 too; but once exam creation is wired (#255,
  #273), `Guid.Empty` flows all the way to the parent lookup and surfaces as
  `NotFoundException` → 404, when the API contract for client-side shape errors is
  `400` + field-keyed `errors` through the validation middleware.
- There is no `UpdateExamRequest` in the codebase, so only this one validator is
  affected.

## Expected Behavior

- `CreateExamRequest` with `ModuleId == Guid.Empty` fails validation with the
  error-code message `Exam.ModuleId.Required`.
- The failure flows through `ExceptionHandlingMiddleware` (#249) as `400` with the
  field-keyed `errors` object, exactly like the other validator failures.
- A valid, non-empty `ModuleId` is unaffected — existence checking of the parent
  remains the service's job (404), not the validator's.

## Scope

- Add one rule to `CreateExamRequestValidator` consistent with the sibling pattern:
  `RuleFor(x => x.ModuleId).NotEmpty().WithMessage("Exam.ModuleId.Required");`
- Add unit tests in `tests/TalebElm.Tests/UnitTests/CreateExamRequestValidatorTests.cs`
  asserting the exact message key for `Guid.Empty` and a pass case for a real GUID.

Out of scope: anything in `ExamService` DI registration, PUT/DELETE exam endpoints,
and question-level rules (they live with #273 / the ExamService issue by decision in
#272).

## Acceptance Criteria

- [ ] `CreateExamRequest` with `Guid.Empty` `ModuleId` fails with `Exam.ModuleId.Required`.
- [ ] Unit tests assert the exact failure message key (error-code style, per sibling validators).
- [ ] A non-empty `ModuleId` passes the validator.
- [ ] `dotnet build TalebElm.slnx` and `dotnet test TalebElm.slnx` stay green.

## Verification (how a beginner confirms)

1. Open the repo, run `dotnet test TalebElm.slnx` — baseline green.
2. Write/inspect the new unit test: instantiate the validator, call
   `ValidateAsync(new CreateExamRequest(Guid.Empty, "demo", 50))`, assert
   `IsValid == false` and the error message equals `Exam.ModuleId.Required`.
3. Run tests again — green with the new test.
4. Optional UI check: after implementing, tracing the validator in the debugger or
   adding a middleware-level test shows a `400` (not 404/501) path once exam wiring
   lands.

## Repository Evidence

- DTO shape: `src/TalebElm.Application/DTOs/CreateExamRequest.cs:9` —
  `CreateExamRequest(Guid ModuleId, string Title, int PassThreshold)`.
- Missing rule: `src/TalebElm.Application/Validators/CreateExamRequestValidator.cs:8-15`.
- Sibling convention to copy:
  - `CreateModuleRequestValidator.cs:22-23` → `.NotEmpty().WithMessage("Module.TrackId.Required")`
  - `CreateLessonRequestValidator.cs:14-15` → `.NotEmpty().WithMessage("Lesson.ModuleId.Required")`
- Persistence contract: `ExamConfiguration` maps a required FK on `ModuleId` with a
  unique index (`src/TalebElm.Infrastructure/Configurations/ExamConfiguration.cs`),
  so an empty parent id can never be legitimate.
- Error-code message style: "Use error codes as messages so tests and clients can
  rely on them" (comment in `CreateModuleRequestValidator.cs:10`); middleware maps
  `ValidationException` → 400 (`docs/features/08-health-and-error-handling.md`).

## Difficulty

Truly Easy. One rule + one test file in an already-tested spot (the test file for
this validator class already exists in `tests/TalebElm.Tests/UnitTests` — extend it).

## Estimated Effort

~30–45 minutes including tests.

## Duplicate Check

- `#421` — "Exam title max length in `CreateExamRequestValidator`": different rule
  (Title length vs parent-id NotEmpty) in the same file. Mentioned here so the two
  issues don't churn the same lines; either order works.
- `#272` (closed) — added the Module/Lesson validators; its `CreateExamRequest` row
  lists only Title / PassThreshold / questions rules, so `ModuleId` was intentionally
  out of that slice and no issue was opened for it.
- `#302` / `#301` (closed, unmerged PRs) — same rule table as #272; both abandon the
  implementation, and neither adds an exam `ModuleId` rule.
- `#193` (closed) — EF mapping of the exam FK, not request validation.
- `#273` / `#255` — exam service/API wiring; per #272's split, shape validation
  belongs in the validator, not the service issue.
- GitHub lexical search (`repo:Tawjjeh/TalebElm exam ModuleId required`,
  `CreateExamRequestValidator`, `ProgressController`) returned no other open issue
  or PR covering this rule.

## Labels (suggested)

`good first issue` · `help wanted` · `layer:application` · `validation` · `feature:exams`
