# TalebElm Project Status

## What TalebElm Is

TalebElm is an open-source learning platform for people who want to become
software engineers. The first learning track focuses on the .NET ecosystem.

The learning journey is organized like this:

```text
Track -> Modules -> Lessons -> Exam -> Next module unlocked
```

Each user's progress is stored separately. A user should not move to the next
module until they pass the exam for the current module.

## Where The Project Is Now

The repository has completed most of the foundation work:

- The five-project Clean Architecture solution exists.
- Domain entities, enums, exceptions, repository interfaces, DTOs, and service
  contracts are present.
- The main exam and progress entities are present.
- Basic controllers and infrastructure classes exist as project structure.
- GitHub Actions builds the solution and runs the test project.
- The current test suite passes, but most tests are still empty structure tests.

The project is now moving from **MVP scaffolding** to **core business logic**.

## What Is Still Missing

The important parts of the product are not implemented yet:

- `AppDbContext` needs real `DbSet` declarations and configuration registration.
- A database provider, connection settings, and migrations are still needed.
- Repositories still contain placeholder implementations.
- Services still contain placeholder implementations.
- Track content cannot yet be created, read, or ordered through a real use case.
- Exam submission does not calculate a result yet.
- Passing an exam does not unlock the next module yet.
- User progress is not updated transactionally with an exam result.
- Authentication and authorization are not implemented.
- The API controllers are still placeholders and are intentionally outside the
  first implementation wave.
- Most tests verify that types exist; real behavior tests are still needed.

## Latest Repository Work

The latest completed repository work delivered the initial contracts and
structure for tracks, content, exams, progress, persistence, and tests. The
recent cleanup also standardized formatting and removed a duplicate EF Core
package reference.

The current remote `main` branch includes the recent domain, application, and
infrastructure scaffolding merges. Open GitHub issues still include the
remaining exam/progress scaffolding tasks and a few earlier test and service
tasks.

## New MVP Issue Batch

The latest implementation backlog was added as 30 small, focused GitHub issues.
They deliberately stay below the API layer for now.

### Database foundation

- #185 Choose a local database provider for development
- #186 Add safe local database settings
- #187 Add DbSet properties to AppDbContext
- #188 Register entity configurations in AppDbContext
- #189 Create the first EF Core migration
- #190 Configure BaseEntity keys and timestamps
- #191 Configure Track and Module relationships
- #192 Configure Module and Lesson relationships
- #193 Configure Exam and UserProgress relationships
- #194 Prevent duplicate progress rows

### Repositories and services

- #195 Implement TrackRepository reads
- #196 Implement TrackRepository AddAsync
- #197 Implement ModuleRepository
- #198 Implement LessonRepository
- #199 Implement ExamRepository
- #200 Finish UnitOfWork database coordination
- #201 Implement TrackService read use cases
- #202 Implement TrackService create use case
- #203 Implement UserService use cases
- #204 Implement UserProgressRepository

### Core rules and tests

- #205 Implement ExamService load use case
- #206 Calculate an exam result from the pass threshold
- #207 Implement UserProgressService reads
- #208 Unlock the next module after a passed exam
- #209 Keep the next module locked after a failed exam
- #210 Add repository tests with a local database
- #211 Add TrackService behavior tests
- #212 Add exam scoring tests
- #213 Add progression-lock tests
- #214 Add CreateTrackRequest validation tests

The recommended starting point is #185, then work through the database issues
before taking the repository, service, and progression-lock issues.

## Recommended MVP Order

The next work should stay below the API layer and move from the inside out:

1. Finish the database foundation and EF Core model configuration.
2. Implement repositories and the unit of work.
3. Implement track, module, lesson, exam, and progress services.
4. Add the progression-lock rules around exam submission.
5. Add behavior-focused unit and integration tests without depending on API
   controllers first.
6. Wire the completed application services into the API in a later phase.

## Definition Of MVP Progress

The core MVP is ready when a test can create a track with ordered modules and
lessons, enroll a user, submit an exam, calculate the result, and verify that
passing one module unlocks only the next module for that user.

## Contribution Guidance

New issues should be small, focused, and written in plain language. Each issue
should explain the expected result, the likely location, and a simple
acceptance checklist. For the first implementation wave, prefer Domain,
Application, Infrastructure, and Tests work. Keep API work for after the core
use cases are working and covered by tests.