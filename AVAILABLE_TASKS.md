# Available Tasks for Contributors

Welcome! This file is your starting point as a contributor.

To claim your first task and practice making a Pull Request, follow these steps:

1. Pick one task from the list below.
2. Fork the repository and clone it to your computer.
3. Delete your chosen task from this file (just that one task).
4. Commit the change and open a Pull Request.
5. Once your Pull Request is accepted, open that task as a real GitHub issue
   and start writing the actual code.

Full instructions for every git command are in CONTRIBUTING.md. Take your time.

Every task asks you to create an empty structure only: a class, an interface,
a record, or an enum with simple properties. There is NO business logic and NO
method implementation required. If a task asks for a method, write only its
signature (the first line) and leave the body empty or throw
NotImplementedException.

Welcome to TalebElm. We are glad you are here.

---

## Labels & Difficulty Overview

### What the GitHub labels mean

Every task lists suggested GitHub labels. Use these labels when you open the
issue for your task. They help everyone find and sort the work.

- `good first issue`: this task is perfect for a beginner. Small, clear, and
  safe to make mistakes on. Start here if you are new.
- `layer:domain`: the code goes in the Domain project.
- `layer:application`: the code goes in the Application project.
- `layer:infrastructure`: the code goes in the Infrastructure project.
- `layer:api`: the code goes in the Api project.
- `layer:tests`: the code goes in the Tests project.
- `entity`: you create a class that represents a thing (like a User).
- `enum`: you create a list of named choices.
- `exception`: you create an error class.
- `interface`: you create a contract (a list of method names, no bodies).
- `dto`: you create a record used to move data.
- `database`: the code is about the database.
- `controller`: you create a class that handles web requests.
- `testing`: you create a test class.

### What the difficulty levels mean

- `Very Easy`: a small file with only properties or only a signature. Great
  for your first-ever pull request.
- `Easy`: a small file, but it needs a few more properties or an extra step
  (like inheriting from another class).
- `Medium`: the file connects to other pieces (like implementing an interface
  or configuring a database class). Read the related files first, and ask if
  you are unsure.

### Which tasks are the hardest?

Not every task is equal. Here is what to expect:

- **The generic `IRepository<T>` (Task 12) is one of the hardest in the whole
  list.** It uses a type parameter `<T>`, which means "this interface works
  with any class". Type parameters are a bigger idea. Do this one after you
  have done a few simpler tasks, and ask for help if the angle brackets look
  strange.
- **`IUnitOfWork` (Task 15)** is also trickier because it lists other
  interfaces as properties.
- **The whole of Phase 3 (Tasks 26-35) is the hardest phase.** It is labeled
  `database` and `layer:infrastructure`. These tasks need the Entity Framework
  Core package and use classes like `DbContext`,
  `IEntityTypeConfiguration`, and `EntityTypeBuilder`. They also require adding
  a NuGet package with the command line. Expect to read the files around yours
  and to ask questions. That is normal and welcome.
- **The validator task in Phase 2** is medium: it inherits from a
  FluentValidation class that needs a package to be added first.

If you are brand new, start with Phase 1 (Tasks 1-11) and Phase 5. Those are
the gentlest. Leave Phase 3 for later, or for when you feel braver. There is
no rush, and there are no silly questions.

---

## Phase 1: Domain Layer (Tasks 1-15)

Focus on core entities, enums, and basic interfaces. The Domain layer holds the
core rules only. No database code, no web code, no methods with real behavior.








## Phase 2: Application Layer (Tasks 16-25)

Focus on Interfaces and empty DTO Records. The Application layer describes what
the app can do. Here we create empty interfaces and empty DTO records. No logic.


---

## Phase 3: Infrastructure Layer (Tasks 26-35)

Focus on the AppDbContext, empty EntityTypeConfigurations, and empty Repository
classes. The Infrastructure layer talks to the database. These tasks are the
hardest in the list. They need the Entity Framework Core package and connect to
several other files. Empty bodies or NotImplementedException are fine.

---

## Phase 4: API Layer (Tasks 36-42)

Focus on empty Controllers and Action method signatures. The API layer is the
door of the app. Controllers only receive and send requests. No real logic.

---

## Phase 5: Tests Layer (Tasks 43-50)

Focus on empty xUnit test classes and empty [Fact] methods. The Tests layer
checks that our code works. These are gentle tasks, great for beginners. No logic.

---

## Phase 6: Domain Layer — Exams & Progress (Tasks 51-54)

Focus on the two new MVP entities and their repository interfaces. They power
the "progression lock" feature: users must pass a module's exam to unlock the
next module (see `docs/DATABASE_ERD.md`). Same rules as Phase 1: empty
structures only, no business logic.

---

## Phase 7: Application Layer — Exams & Progress (Tasks 55-61)

Focus on the new DTO records and service interfaces for exams and user
progress. Same rules as Phase 2: empty DTO records and empty interfaces, no
logic.

---

## Phase 8: Infrastructure Layer — Exams & Progress (Tasks 62-67)

Focus on the new EntityTypeConfigurations, repositories, and service
implementations. Same rules as Phase 3: empty bodies or NotImplementedException
are fine. Read the related files first.

---

## Phase 9: API Layer — Exams & Progress (Tasks 68-69)

Focus on the two new controllers for exams and user progress. Same rules as
Phase 4: empty Controllers and Action method signatures. No real logic.

---

## Phase 10: Tests Layer — Exams & Progress (Tasks 70-73)

Focus on empty xUnit test classes for the new entities and DTOs. Same rules as
Phase 5: gentle tasks, empty [Fact] methods, no logic.

---

## After you finish your task

1. Build the project to check for errors:
   `dotnet build TalebElm.slnx`
2. Run the tests:
   `dotnet test TalebElm.slnx`
3. If a file needs a package that is not installed yet, ask in your Pull
   Request comment. We will help you add it.

Thank you for contributing to TalebElm. Every small task brings the project
closer to real, working lessons for everyone.