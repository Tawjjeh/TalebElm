# TalebElm Architecture Guide

This file explains how the TalebElm project is organized. It is written for
absolute beginners. We use simple words and lots of examples.

If you are new to .NET, read this file first. It will help you understand
**where** things go when you write code later.

---

## What is "Clean Architecture" in one line?

Clean Architecture is a way to organize a project into **separate parts**
(called layers) so that each part does only one job.

The most important idea is:

> Each layer knows only about the layer inside of it.
> No layer reaches into the wrong place to do another layer's job.

Think of it like a house floor plan. The kitchen has its own room. The bedroom
does not contain the kitchen sink. If you keep each room for its own purpose,
the house is easy to build, easy to fix, and easy to understand.

The same is true for software. If code is in the wrong "room", it gets confusing
and hard to change. Clean Architecture keeps every room clean.

---

## A picture of the layers

Imagine a set of rings or boxes, one inside the other:

```
                  +---------------------+
                  |      Api            |   (the web API)
                  +---------------------+
                       |        |
              +---------------------+
              |  Infrastructure     |   (database and outside tools)
              +---------------------+
                       |        |
              +---------------------+
              |    Application      |   (the work of the app)
              +---------------------+
                       |        |
              +---------------------+
              |      Domain         |   (the most important rules)
              +---------------------+

  Tests are extra. They can look at everything.
```

The arrows point **inward**. That is the rule:
- Outer layers may use inner layers.
- Inner layers never use outer layers.

The center (Domain) is the most important and depends on nothing.

---

## The projects in this solution

We have five projects. Here is a short summary of each, with more detail below.

| Project | Folder | Role in the house |
| --- | --- | --- |
| Domain | `src/TalebElm.Domain` | The rules of the business |
| Application | `src/TalebElm.Application` | The actions the app can do |
| Infrastructure | `src/TalebElm.Infrastructure` | The database and outside stuff |
| Api | `src/TalebElm.Api` | The door people knock on (HTTP) |
| Tests | `tests/TalebElm.Tests` | The check that everything works |

---

## Layer 1: Domain (the heart of the business)

Full project folder: `src/TalebElm.Domain`

### What it is for

This is the **most important** layer. It holds the core rules of your business.
The "business" means: what is TalebElm really about? It is about learning tracks,
lessons, roadmaps, and students. Those things are "business ideas".

Domain represents those ideas as simple code objects.

### What files belong here

- **Entities**: the "things" in the business, like `Track` (a learning track)
  or `Module` (one part of a track).
- **Enums**: a list of fixed choices. For example, a track status could be
  "Draft" or "Published".
- **Exceptions**: errors that are specific to this business, like
  "A track with this id was not found".
- **Interface** that define what the Domain needs, but not how it is done.

### What NOT to put here (very important)

- Do **not** put database code here. (No database imports, no SQL.)
- Do **not** put web code here. (No controllers, no HTTP.)
- Do **not** put anything about outer services (email, files, login) here.

Rule to remember: If a file talks about "how the real world implements
something", it does not belong in Domain. The Domain only says **what things
are**, never **how they are stored or served**.

### Example to make it clear

Bad (do not do this):

- A `Track` class that saves itself to a database.
- A `Track` class that knows about HTTP requests.

Good (do this):

- A `Track` class that only holds its own data (name, description) and its own
  rules (for example, a track must have a name).
- An interface that says "I need a way to save tracks" without saying how.

---

## Layer 2: Application (the work of the platform)

Full project folder: `src/TalebElm.Application`

### What it is for

If Domain says "what the business is", Application says **"what the business
can do"**. It is the list of actions, like "create a track", "show all tracks",
or "delete a module".

This layer holds the **use cases** (the things a user can do), but it does not
do the actual database work itself. It only asks for help.

### What files belong here

- **Service Interfaces**: the actions the app can do, like "I need a way to create a track".
- **DTOs**: short for "Data Transfer Object". These are simple boxes for sending data between the web API and the Application. They are usually not the same as Domain entities.

### What NOT to put here

- Do **not** put actual database code. (The real database is in Infrastructure.)
- Do **not** put actual web controllers.
- Do **not** put how to connect to other services.

The Application should not care whether we use SQL Server, a plain file, or a cloud database. It just says "I need to save a track" and another layer decides how.

---

## Layer 3: Infrastructure (the how)

Full project folder: `src/TalebElm.Infrastructure`

### What it is for

Infrastructure is the layer that actually **does the real work** that the inner layers asked for. This is where "the how" lives.

The most common example is the **database**. Infrastructure uses a tool called **Entity Framework Core** to talk to real databases.

### What files belong here

- **Service Implementations**: the real code that does the work defined in the Application layer.
- The database context (the file that talks to the database).
- The models that represent how the data is stored in the database.
- Settings that say how to connect to the database.
- Code that emails, files, or any outside tool like talking to APIs.
- Repositories: real code that saves and loads data.

### What NOT to put here

- Do **not** put business rules here. Those belong in Domain.
- Do **not** put use cases here. Those belong in Application.
- Do **not** put controllers here. Those belong in Api.

Infrastructure answers "how" questions. It does not decide "what rules" the
business has.

---

## Layer 4: Api (the door)

Full project folder: `src/TalebElm.Api`

### What it is for

This is the **web API**. It is the part real people and other apps talk to over
the internet. When a browser visits a web address, it hits this layer.

### What a web API does

A web API receives a request (like "give me all tracks") and sends back a
response (a JSON list of tracks). Each address on the API is called an
**endpoint** or a **route**.

### What files belong here

- **Controllers**: the files that handle incoming requests.
- **Middleware**: small steps that every request passes through (like error
  handling).
- The startup file (`Program.cs`) that starts the app.
- Settings files (`appsettings.json`) with options like connection strings.

### What NOT to put here

- Do **not** put business rules (those are in Domain).
- Do **not** put use cases detail if they can live in Application.
- Do **not** put real database code (that is in Infrastructure).

The Api should stay "thin". It receives a request, passes it to the
Application, and returns the result. It should not do all the thinking itself.

---

## Layer 5: Tests (the checker)

Full project folder: `tests/TalebElm.Tests`

### What it is for

Tests are small programs that **check that our code works correctly**.
A test says "given this input, I expect this output". If the output is wrong,
the test fails, and we know there is a problem.

We use a tool called **xUnit** to write tests. The word "unit" means we test
one small piece of code at a time.

### What files belong here

- Test files that check the Application actions (like "create a track should
  return the new track id").
- Test files that check the coordinates and rules in Domain.
- Later, tests that check the Api by sending a real HTTP request.

### Why are tests separate?

Tests sit outside the main project, so they can look at many layers at once.
The Domain, Application, Infrastructure, and Api do not need to know about
tests. Tests are allowed to look at them, not the other way around.

You run all the tests with this command:

```
dotnet test
```

You should get a happy message that says all tests passed.

---

## How the projects connect (references)

The way the projects connect is very important. We set these connections up
so that power points inward. Here is what we did:

1. Domain has **no references to any other project**. It is the center.
2. Application can use Domain (it references Domain).
3. Infrastructure can use Application and Domain.
4. Api can use Infrastructure (and, because of that, also Application and Domain).
5. Tests can use everything (it can see Api, Application, and Domain).

If you open each `.csproj` file, you can see these references.

---

## How a request flows (a small example)

Imagine someone opens the website and asks: "Show me all the tracks."

The request flows like this:

1. **Api** receives the web request "get me all tracks".
2. **Api** passes this request to the **Application**.
3. **Application** says "I need the list of tracks. I will ask a repository."
4. **Infrastructure** has the real repository that reads the database.
5. **Infrastructure** gets the data and gives it back.
6. **Application** returns the data to **Api**.
7. **Api** sends the data back to the browser as JSON.

Notice the trip: Api -> Application -> Infrastructure (with Domain rules
inside all the way). No step jumps ahead of the line, and every layer does
only its own job.

---

## A simple rule of thumb

When you write a new file, ask yourself: "What job does this file do?"

- Is it a core business rule? -> Domain.
- Is it an action the app can do? -> Application.
- Is it about the real database or outside services? -> Infrastructure.
- Is it an internet endpoint (web request)? -> Api.
- Is it checking that code works? -> Tests.

When you are not sure, ask for help in your pull request. This is a learning
project, and helping each other is the goal.

---

## Final words

You do not need to memorize everything today. This guide exists so you can
come back to it. The most important thing to remember is the **one rule**:

> Each layer does one job, and the layers only reach inward.

Start small. Build from the Domain and work outward. If you ever feel lost,
read this guide again, and reach out for help. We are all seekers of knowledge
together.