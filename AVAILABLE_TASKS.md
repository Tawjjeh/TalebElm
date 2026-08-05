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



- **Task 2: Create User Entity**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:domain`, `entity`
  - **Location:** `TalebElm.Domain/Entities/`
  - **Instructions:** Create a public class named `User`. It should inherit from
    `BaseEntity`. Add empty properties for `Name`, `Email`, and `JoinedAt`.
    Do not add any methods.
  - **Hint:** `public class User : BaseEntity { public string Name { get; set; } public string Email { get; set; } }`

- **Task 3: Create Track Entity**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:domain`, `entity`
  - **Location:** `TalebElm.Domain/Entities/`
  - **Instructions:** Create a public class named `Track`. It should inherit from
    `BaseEntity`. Add empty properties for `Name`, `Description`, and `Status`.
    Do not add any methods.
  - **Hint:** `public class Track : BaseEntity { public string Name { get; set; } public string Description { get; set; } }`

- **Task 4: Create Module Entity**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:domain`, `entity`
  - **Location:** `TalebElm.Domain/Entities/`
  - **Instructions:** Create a public class named `Module`. It should inherit from
    `BaseEntity`. Add empty properties for `Title`, `Summary`, `Order`, and
    `TrackId` (a Guid). Do not add any methods.
  - **Hint:** `public class Module : BaseEntity { public string Title { get; set; } public int Order { get; set; } }`

- **Task 5: Create Lesson Entity**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:domain`, `entity`
  - **Location:** `TalebElm.Domain/Entities/`
  - **Instructions:** Create a public class named `Lesson`. It should inherit from
    `BaseEntity`. Add empty properties for `Title`, `Content`, `Order`, and
    `ModuleId` (a Guid). Do not add any methods.
  - **Hint:** `public class Lesson : BaseEntity { public string Title { get; set; } public int Order { get; set; } }`

- **Task 6: Create TrackStatus Enum**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:domain`, `enum`
  - **Location:** `TalebElm.Domain/Enums/`
  - **Instructions:** Create a public enum named `TrackStatus`. Add three named
    options: `Draft`, `Published`, and `Archived`. Give each a number.
    Enums do not have methods.
  - **Hint:** `public enum TrackStatus { Draft = 0, Published = 1, Archived = 2 }`

- **Task 7: Create LessonType Enum**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:domain`, `enum`
  - **Location:** `TalebElm.Domain/Enums/`
  - **Instructions:** Create a public enum named `LessonType`. Add three named
    options: `Text`, `Video`, and `Exercise`. Give each a number.
    Enums do not have methods.
  - **Hint:** `public enum LessonType { Text = 0, Video = 1, Exercise = 2 }`

- **Task 8: Create UserRole Enum**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:domain`, `enum`
  - **Location:** `TalebElm.Domain/Enums/`
  - **Instructions:** Create a public enum named `UserRole`. Add three named
    options: `Student`, `Instructor`, and `Admin`. Give each a number.
    Enums do not have methods.
  - **Hint:** `public enum UserRole { Student = 0, Instructor = 1, Admin = 2 }`

- **Task 9: Create DomainException Class**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:domain`, `exception`
  - **Location:** `TalebElm.Domain/Exceptions/`
  - **Instructions:** Create a public class named `DomainException`. It should
    inherit from the built-in `Exception` class. Add one constructor that takes
    a `string message` and passes it to the base class. Do not add any methods.
  - **Hint:** `public class DomainException : Exception { public DomainException(string message) : base(message) { } }`

- **Task 10: Create NotFoundException Class**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:domain`, `exception`
  - **Location:** `TalebElm.Domain/Exceptions/`
  - **Instructions:** Create a public class named `NotFoundException`. It should
    inherit from `DomainException`. Add one constructor that takes a
    `string message` and passes it to the base class. Do not add any methods.
  - **Hint:** `public class NotFoundException : DomainException { public NotFoundException(string message) : base(message) { } }`

- **Task 11: Create ValidationException Class**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:domain`, `exception`
  - **Location:** `TalebElm.Domain/Exceptions/`
  - **Instructions:** Create a public class named `ValidationException`. It should
    inherit from `DomainException`. Add one constructor that takes a
    `string message` and passes it to the base class. Do not add any methods.
  - **Hint:** `public class ValidationException : DomainException { public ValidationException(string message) : base(message) { } }`

- **Task 12: Create IRepository Interface**
  - **Difficulty:** Medium
  - **Labels:** `layer:domain`, `interface`, `setup`
  - **Location:** `TalebElm.Domain/Interfaces/`
  - **Instructions:** Create a public interface named `IRepository<T>`. It uses a
    type parameter `T`. This is harder than the other tasks because of the
    `<T>` part: it means "this interface works with any class". Add method
    signatures only: `GetByIdAsync` (takes a Guid and returns a Task of T or
    null), `GetAllAsync` (returns a Task of a list of T), and `AddAsync` (takes
    a T and returns a Task). No bodies, no logic.
  - **Hint:** `public interface IRepository<T> where T : BaseEntity { Task<T?> GetByIdAsync(Guid id); Task<IReadOnlyList<T>> GetAllAsync(); Task AddAsync(T entity); }`

- **Task 13: Create IUserRepository Interface**
  - **Difficulty:** Easy
  - **Labels:** `good first issue`, `layer:domain`, `interface`
  - **Location:** `TalebElm.Domain/Interfaces/`
  - **Instructions:** Create a public interface named `IUserRepository`. It should
    inherit from `IRepository<User>`. Do not add any members yet. An empty
    interface is fine.
  - **Hint:** `public interface IUserRepository : IRepository<User> { }`

- **Task 14: Create ITrackRepository Interface**
  - **Difficulty:** Easy
  - **Labels:** `good first issue`, `layer:domain`, `interface`
  - **Location:** `TalebElm.Domain/Interfaces/`
  - **Instructions:** Create a public interface named `ITrackRepository`. It
    should inherit from `IRepository<Track>`. Do not add any members yet. An
    empty interface is fine.
  - **Hint:** `public interface ITrackRepository : IRepository<Track> { }`

- **Task 15: Create IUnitOfWork Interface**
  - **Difficulty:** Medium
  - **Labels:** `layer:domain`, `interface`, `setup`
  - **Location:** `TalebElm.Domain/Interfaces/`
  - **Instructions:** Create a public interface named `IUnitOfWork`. It
    represents one shared database session. It lists other interfaces as
    properties, which is why it is a bit harder. Add two read-only properties:
    `Users` (IUserRepository) and `Tracks` (ITrackRepository), plus a
    `SaveChangesAsync` method signature that returns a Task of int. No bodies.
  - **Hint:** `public interface IUnitOfWork { IUserRepository Users { get; } ITrackRepository Tracks { get; } Task<int> SaveChangesAsync(); }`

---

## Phase 2: Application Layer (Tasks 16-25)

Focus on Interfaces and empty DTO Records. The Application layer describes what
the app can do. Here we create empty interfaces and empty DTO records. No logic.

- **Task 16: Create IApplicationDbContext Interface**
  - **Difficulty:** Easy
  - **Labels:** `layer:application`, `interface`, `database`
  - **Location:** `TalebElm.Application/Interfaces/`
  - **Instructions:** Create a public interface named `IApplicationDbContext`.
    It will be a contract for the real database context. Add a
    `SaveChangesAsync` method signature that returns a Task of int. No bodies.
  - **Hint:** `public interface IApplicationDbContext { Task<int> SaveChangesAsync(CancellationToken cancellationToken = default); }`

- **Task 17: Create CreateUserRequest Record**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:application`, `dto`
  - **Location:** `TalebElm.Application/DTOs/`
  - **Instructions:** Create a public record named `CreateUserRequest`. Add two
    fields: `Name` and `Email`. Records do not have methods.
  - **Hint:** `public record CreateUserRequest(string Name, string Email);`

- **Task 18: Create CreateTrackRequest Record**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:application`, `dto`
  - **Location:** `TalebElm.Application/DTOs/`
  - **Instructions:** Create a public record named `CreateTrackRequest`. Add two
    fields: `Name` and `Description`. Records do not have methods.
  - **Hint:** `public record CreateTrackRequest(string Name, string Description);`

- **Task 19: Create UpdateTrackRequest Record**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:application`, `dto`
  - **Location:** `TalebElm.Application/DTOs/`
  - **Instructions:** Create a public record named `UpdateTrackRequest`. Add
    three fields: `Id` (a Guid), `Name`, and `Description`. Records do not have methods.
  - **Hint:** `public record UpdateTrackRequest(Guid Id, string Name, string Description);`

- **Task 20: Create UserResponse Record**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:application`, `dto`
  - **Location:** `TalebElm.Application/DTOs/`
  - **Instructions:** Create a public record named `UserResponse`. Add three
    fields: `Id` (a Guid), `Name`, and `Email`. Records do not have methods.
  - **Hint:** `public record UserResponse(Guid Id, string Name, string Email);`

- **Task 21: Create TrackResponse Record**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:application`, `dto`
  - **Location:** `TalebElm.Application/DTOs/`
  - **Instructions:** Create a public record named `TrackResponse`. Add four
    fields: `Id` (a Guid), `Name`, `Description`, and `Status` (an int).
    Records do not have methods.
  - **Hint:** `public record TrackResponse(Guid Id, string Name, string Description, int Status);`

- **Task 22: Create LessonResponse Record**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:application`, `dto`
  - **Location:** `TalebElm.Application/DTOs/`
  - **Instructions:** Create a public record named `LessonResponse`. Add three
    fields: `Id` (a Guid), `Title`, and `ModuleId` (a Guid). Records do not have methods.
  - **Hint:** `public record LessonResponse(Guid Id, string Title, Guid ModuleId);`

- **Task 23: Create IUserService Interface**
  - **Difficulty:** Easy
  - **Labels:** `layer:application`, `interface`
  - **Location:** `TalebElm.Application/Services/`
  - **Instructions:** Create a public interface named `IUserService`. Add two
    method signatures only: `GetAllAsync` (returns a Task of a list of
    UserResponse) and `CreateAsync` (takes a CreateUserRequest and returns a
    Task of UserResponse). No bodies, no logic.
  - **Hint:** `public interface IUserService { Task<IReadOnlyList<UserResponse>> GetAllAsync(); Task<UserResponse> CreateAsync(CreateUserRequest request); }`

- **Task 24: Create ITrackService Interface**
  - **Difficulty:** Easy
  - **Labels:** `layer:application`, `interface`
  - **Location:** `TalebElm.Application/Services/`
  - **Instructions:** Create a public interface named `ITrackService`. Add two
    method signatures only: `GetAllAsync` (returns a Task of a list of
    TrackResponse) and `CreateAsync` (takes a CreateTrackRequest and returns a
    Task of TrackResponse). No bodies, no logic.
  - **Hint:** `public interface ITrackService { Task<IReadOnlyList<TrackResponse>> GetAllAsync(); Task<TrackResponse> CreateAsync(CreateTrackRequest request); }`

- **Task 25: Create CreateTrackRequestValidator Class**
  - **Difficulty:** Medium
  - **Labels:** `layer:application`, `setup`, `validation`
  - **Location:** `TalebElm.Application/Validators/`
  - **Instructions:** Create a public class named `CreateTrackRequestValidator`.
    It should inherit from the FluentValidation class
    `AbstractValidator<CreateTrackRequest>`. Add an empty constructor. Do not
    write any validation rules yet. No logic. This one is Medium because it
    needs the FluentValidation package, which must be added to the project
    first with the command
    `dotnet add src/TalebElm.Application package FluentValidation.DependencyInjectionExtensions`.
  - **Hint:** `public class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest> { public CreateTrackRequestValidator() { } }`

---

## Phase 3: Infrastructure Layer (Tasks 26-35)

Focus on the AppDbContext, empty EntityTypeConfigurations, and empty Repository
classes. The Infrastructure layer talks to the database. These tasks are the
hardest in the list. They need the Entity Framework Core package and connect to
several other files. Empty bodies or NotImplementedException are fine.

- **Task 26: Create AppDbContext Class**
  - **Difficulty:** Medium
  - **Labels:** `layer:infrastructure`, `database`, `setup`
  - **Location:** `TalebElm.Infrastructure/Persistence/`
  - **Instructions:** Create a public class named `AppDbContext`. It should
    inherit from `DbContext`. Add a constructor that takes
    `DbContextOptions<AppDbContext>` and passes it to the base class. Do not add
    logic. The `Microsoft.EntityFrameworkCore` package is required; add it with the
    command `dotnet add src/TalebElm.Infrastructure package Microsoft.EntityFrameworkCore`.
    Ask for help if the package install is confusing.
  - **Hint:** `public class AppDbContext : DbContext { public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { } }`

- **Task 27: Create UserConfiguration Class**
  - **Difficulty:** Medium
  - **Labels:** `layer:infrastructure`, `database`
  - **Location:** `TalebElm.Infrastructure/Persistence/`
  - **Instructions:** Create a public class named `UserConfiguration`. It should
    implement `IEntityTypeConfiguration<User>`. Add a method signature
    `Configure(EntityTypeBuilder<User> builder)` and leave the body empty. No logic.
    Do this after the AppDbContext exists, and read that file first.
  - **Hint:** `public class UserConfiguration : IEntityTypeConfiguration<User> { public void Configure(EntityTypeBuilder<User> builder) { } }`

- **Task 28: Create TrackConfiguration Class**
  - **Difficulty:** Medium
  - **Labels:** `layer:infrastructure`, `database`
  - **Location:** `TalebElm.Infrastructure/Persistence/`
  - **Instructions:** Create a public class named `TrackConfiguration`. It should
    implement `IEntityTypeConfiguration<Track>`. Add a method signature
    `Configure(EntityTypeBuilder<Track> builder)` and leave the body empty. No logic.
  - **Hint:** `public class TrackConfiguration : IEntityTypeConfiguration<Track> { public void Configure(EntityTypeBuilder<Track> builder) { } }`

- **Task 29: Create ModuleConfiguration Class**
  - **Difficulty:** Medium
  - **Labels:** `layer:infrastructure`, `database`
  - **Location:** `TalebElm.Infrastructure/Persistence/`
  - **Instructions:** Create a public class named `ModuleConfiguration`. It should
    implement `IEntityTypeConfiguration<Module>`. Add a method signature
    `Configure(EntityTypeBuilder<Module> builder)` and leave the body empty. No logic.
  - **Hint:** `public class ModuleConfiguration : IEntityTypeConfiguration<Module> { public void Configure(EntityTypeBuilder<Module> builder) { } }`

- **Task 30: Create LessonConfiguration Class**
  - **Difficulty:** Medium
  - **Labels:** `layer:infrastructure`, `database`
  - **Location:** `TalebElm.Infrastructure/Persistence/`
  - **Instructions:** Create a public class named `LessonConfiguration`. It should
    implement `IEntityTypeConfiguration<Lesson>`. Add a method signature
    `Configure(EntityTypeBuilder<Lesson> builder)` and leave the body empty. No logic.
  - **Hint:** `public class LessonConfiguration : IEntityTypeConfiguration<Lesson> { public void Configure(EntityTypeBuilder<Lesson> builder) { } }`

- **Task 31: Create UserRepository Class**
  - **Difficulty:** Medium
  - **Labels:** `layer:infrastructure`, `database`, `interface`
  - **Location:** `TalebElm.Infrastructure/Repositories/`
  - **Instructions:** Create a public class named `UserRepository`. It should
    implement `IUserRepository`. For each interface method, write the signature
    and throw `NotImplementedException`. Do not write any logic.
  - **Hint:** `public class UserRepository : IUserRepository { public Task<User?> GetByIdAsync(Guid id) => throw new NotImplementedException(); public Task<IReadOnlyList<User>> GetAllAsync() => throw new NotImplementedException(); public Task AddAsync(User entity) => throw new NotImplementedException(); }`

- **Task 32: Create TrackRepository Class**
  - **Difficulty:** Medium
  - **Labels:** `layer:infrastructure`, `database`, `interface`
  - **Location:** `TalebElm.Infrastructure/Repositories/`
  - **Instructions:** Create a public class named `TrackRepository`. It should
    implement `ITrackRepository`. For each interface method, write the signature
    and throw `NotImplementedException`. Do not write any logic.
  - **Hint:** `public class TrackRepository : ITrackRepository { public Task<Track?> GetByIdAsync(Guid id) => throw new NotImplementedException(); public Task<IReadOnlyList<Track>> GetAllAsync() => throw new NotImplementedException(); public Task AddAsync(Track entity) => throw new NotImplementedException(); }`

- **Task 33: Create UnitOfWork Class**
  - **Difficulty:** Medium
  - **Labels:** `layer:infrastructure`, `database`, `interface`
  - **Location:** `TalebElm.Infrastructure/Repositories/`
  - **Instructions:** Create a public class named `UnitOfWork`. It should
    implement `IUnitOfWork`. Add the interface members. For properties, write
    the signature only. For `SaveChangesAsync`, write the signature and throw
    `NotImplementedException`. No logic.
  - **Hint:** `public class UnitOfWork : IUnitOfWork { public IUserRepository Users => throw new NotImplementedException(); public ITrackRepository Tracks => throw new NotImplementedException(); public Task<int> SaveChangesAsync() => throw new NotImplementedException(); }`

- **Task 34: Create UserService Class**
  - **Difficulty:** Medium
  - **Labels:** `layer:infrastructure`, `interface`
  - **Location:** `TalebElm.Infrastructure/Services/`
  - **Instructions:** First create the folder `TalebElm.Infrastructure/Services/`
    if it does not exist. Then create a public class named `UserService`. It
    should implement `IUserService`. For each interface method, write the
    signature and throw `NotImplementedException`. No logic.
  - **Hint:** `public class UserService : IUserService { public Task<IReadOnlyList<UserResponse>> GetAllAsync() => throw new NotImplementedException(); public Task<UserResponse> CreateAsync(CreateUserRequest request) => throw new NotImplementedException(); }`

- **Task 35: Create TrackService Class**
  - **Difficulty:** Medium
  - **Labels:** `layer:infrastructure`, `interface`
  - **Location:** `TalebElm.Infrastructure/Services/`
  - **Instructions:** Create a public class named `TrackService`. It should
    implement `ITrackService`. For each interface method, write the signature
    and throw `NotImplementedException`. No logic.
  - **Hint:** `public class TrackService : ITrackService { public Task<IReadOnlyList<TrackResponse>> GetAllAsync() => throw new NotImplementedException(); public Task<TrackResponse> CreateAsync(CreateTrackRequest request) => throw new NotImplementedException(); }`

---

## Phase 4: API Layer (Tasks 36-42)

Focus on empty Controllers and Action method signatures. The API layer is the
door of the app. Controllers only receive and send requests. No real logic.

- **Task 36: Create UsersController Class**
  - **Difficulty:** Easy
  - **Labels:** `layer:api`, `controller`, `good first issue`
  - **Location:** `TalebElm.Api/Controllers/`
  - **Instructions:** Create a public class named `UsersController`. It should
    inherit from `ControllerBase`. Add the `[ApiController]` attribute and the
    route `[Route("api/[controller]")]`. Add an empty `Get` action and an empty
    `Post` action. Both return `NotImplemented()`. No logic.
  - **Hint:** `[ApiController] [Route("api/[controller]")] public class UsersController : ControllerBase { [HttpGet] public IActionResult Get() => NotImplemented(); [HttpPost] public IActionResult Post() => NotImplemented(); }`

- **Task 37: Create TracksController Class**
  - **Difficulty:** Easy
  - **Labels:** `layer:api`, `controller`, `good first issue`
  - **Location:** `TalebElm.Api/Controllers/`
  - **Instructions:** Create a public class named `TracksController`. It should
    inherit from `ControllerBase`. Add the `[ApiController]` attribute and the
    route `[Route("api/[controller]")]`. Add an empty `Get` action and an empty
    `Post` action. Both return `NotImplemented()`. No logic.
  - **Hint:** `[ApiController] [Route("api/[controller]")] public class TracksController : ControllerBase { [HttpGet] public IActionResult Get() => NotImplemented(); [HttpPost] public IActionResult Post() => NotImplemented(); }`

- **Task 38: Create ModulesController Class**
  - **Difficulty:** Easy
  - **Labels:** `layer:api`, `controller`, `good first issue`
  - **Location:** `TalebElm.Api/Controllers/`
  - **Instructions:** Create a public class named `ModulesController`. It should
    inherit from `ControllerBase`. Add the `[ApiController]` attribute and the
    route `[Route("api/[controller]")]`. Add an empty `Get` action. It returns
    `NotImplemented()`. No logic.
  - **Hint:** `[ApiController] [Route("api/[controller]")] public class ModulesController : ControllerBase { [HttpGet] public IActionResult Get() => NotImplemented(); }`

- **Task 39: Create LessonsController Class**
  - **Difficulty:** Easy
  - **Labels:** `layer:api`, `controller`, `good first issue`
  - **Location:** `TalebElm.Api/Controllers/`
  - **Instructions:** Create a public class named `LessonsController`. It should
    inherit from `ControllerBase`. Add the `[ApiController]` attribute and the
    route `[Route("api/[controller]")]`. Add an empty `Get` action. It returns
    `NotImplemented()`. No logic.
  - **Hint:** `[ApiController] [Route("api/[controller]")] public class LessonsController : ControllerBase { [HttpGet] public IActionResult Get() => NotImplemented(); }`

- **Task 40: Create AuthController Class**
  - **Difficulty:** Easy
  - **Labels:** `layer:api`, `controller`, `good first issue`
  - **Location:** `TalebElm.Api/Controllers/`
  - **Instructions:** Create a public class named `AuthController`. It should
    inherit from `ControllerBase`. Add the `[ApiController]` attribute and the
    route `[Route("api/[controller]")]`. Add an empty `Login` action that returns
    `NotImplemented()`. No logic.
  - **Hint:** `[ApiController] [Route("api/[controller]")] public class AuthController : ControllerBase { [HttpPost("login")] public IActionResult Login() => NotImplemented(); }`

- **Task 41: Create HealthController Class**
  - **Difficulty:** Very Easy
  - **Labels:** `layer:api`, `controller`, `good first issue`
  - **Location:** `TalebElm.Api/Controllers/`
  - **Instructions:** Create a public class named `HealthController`. It should
    inherit from `ControllerBase`. Add the `[ApiController]` attribute and the
    route `[Route("api/[controller]")]`. Add an empty `Get` action that returns
    `Ok("healthy")`. No other logic.
  - **Hint:** `[ApiController] [Route("api/[controller]")] public class HealthController : ControllerBase { [HttpGet] public IActionResult Get() => Ok("healthy"); }`

- **Task 42: Create ExceptionHandlingMiddleware Class**
  - **Difficulty:** Medium
  - **Labels:** `layer:api`, `setup`, `middleware`
  - **Location:** `TalebElm.Api/Middlewares/`
  - **Instructions:** Create a public class named `ExceptionHandlingMiddleware`.
    Add a constructor that takes `RequestDelegate next` and stores it. Add an
    `InvokeAsync` method signature that takes `HttpContext` and returns a Task.
    Leave the body empty. No logic. This one is Medium because it uses
    `RequestDelegate`, which is a class you have not seen before. It is just a
    stand-in for "the next step in the web pipeline".
  - **Hint:** `public class ExceptionHandlingMiddleware { private readonly RequestDelegate _next; public ExceptionHandlingMiddleware(RequestDelegate next) { _next = next; } public async Task InvokeAsync(HttpContext context) { } }`

---

## Phase 5: Tests Layer (Tasks 43-50)

Focus on empty xUnit test classes and empty [Fact] methods. The Tests layer
checks that our code works. These are gentle tasks, great for beginners. No logic.

- **Task 43: Create UserEntityTests Class**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:tests`, `testing`
  - **Location:** `TalebElm.Tests/UnitTests/`
  - **Instructions:** Create a public class named `UserEntityTests`. Add one
    empty test method marked with the `[Fact]` attribute. No logic.
  - **Hint:** `public class UserEntityTests { [Fact] public void User_ShouldHaveEmptyConstructor() { } }`

- **Task 44: Create TrackEntityTests Class**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:tests`, `testing`
  - **Location:** `TalebElm.Tests/UnitTests/`
  - **Instructions:** Create a public class named `TrackEntityTests`. Add one
    empty test method marked with the `[Fact]` attribute. No logic.
  - **Hint:** `public class TrackEntityTests { [Fact] public void Track_ShouldHaveEmptyConstructor() { } }`

- **Task 45: Create ModuleEntityTests Class**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:tests`, `testing`
  - **Location:** `TalebElm.Tests/UnitTests/`
  - **Instructions:** Create a public class named `ModuleEntityTests`. Add one
    empty test method marked with the `[Fact]` attribute. No logic.
  - **Hint:** `public class ModuleEntityTests { [Fact] public void Module_ShouldHaveEmptyConstructor() { } }`

- **Task 46: Create LessonEntityTests Class**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:tests`, `testing`
  - **Location:** `TalebElm.Tests/UnitTests/`
  - **Instructions:** Create a public class named `LessonEntityTests`. Add one
    empty test method marked with the `[Fact]` attribute. No logic.
  - **Hint:** `public class LessonEntityTests { [Fact] public void Lesson_ShouldHaveEmptyConstructor() { } }`

- **Task 47: Create TrackStatusEnumTests Class**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:tests`, `testing`
  - **Location:** `TalebElm.Tests/UnitTests/`
  - **Instructions:** Create a public class named `TrackStatusEnumTests`. Add one
    empty test method marked with the `[Fact]` attribute. No logic.
  - **Hint:** `public class TrackStatusEnumTests { [Fact] public void TrackStatus_ShouldHaveThreeValues() { } }`

- **Task 48: Create UserDtoTests Class**
  - **Difficulty:** Very Easy
  - **Labels:** `good first issue`, `layer:tests`, `testing`
  - **Location:** `TalebElm.Tests/UnitTests/`
  - **Instructions:** Create a public class named `UserDtoTests`. Add one empty
    test method marked with the `[Fact]` attribute. No logic.
  - **Hint:** `public class UserDtoTests { [Fact] public void CreateUserRequest_ShouldStoreValues() { } }`

- **Task 49: Create TracksControllerTests Class**
  - **Difficulty:** Easy
  - **Labels:** `good first issue`, `layer:tests`, `testing`
  - **Location:** `TalebElm.Tests/IntegrationTests/`
  - **Instructions:** Create a public class named `TracksControllerTests`. Add
    one empty test method marked with the `[Fact]` attribute. No logic.
  - **Hint:** `public class TracksControllerTests { [Fact] public void Get_ReturnsSuccessStatus() { } }`

- **Task 50: Create UsersControllerTests Class**
  - **Difficulty:** Easy
  - **Labels:** `good first issue`, `layer:tests`, `testing`
  - **Location:** `TalebElm.Tests/IntegrationTests/`
  - **Instructions:** Create a public class named `UsersControllerTests`. Add one
    empty test method marked with the `[Fact]` attribute. No logic.
  - **Hint:** `public class UsersControllerTests { [Fact] public void Get_ReturnsSuccessStatus() { } }`

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