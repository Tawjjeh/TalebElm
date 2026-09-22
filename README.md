# TalebElm

TalebElm is an open-source educational platform made by the **Twajjeh** organization.
Its goal is to teach people how to become software engineers from zero.
The first track teaches the **.NET ecosystem** (C#, ASP.NET Core, and more).

The word "TalebElm" comes from Arabic. It means "Seeker of Knowledge".
It is a person who wants to learn. That is who this project is for.

---

## What is TalebElm?

TalebElm is not just a website. It is a complete learning system. It will provide:

- **Roadmaps**: a clear list of what to learn and in what order.
- **Lessons**: simple explanations of each topic.
- **Examples**: small pieces of working code to read and try.
- **Projects**: bigger tasks you build yourself to practice.

We start with .NET, but the project is built so we can add more tracks later
(like JavaScript, mobile development, and more).

---

## What is this project right now?

Right now, this repository is a **skeleton** (an empty starting point).
It contains an empty **Clean Architecture** solution ready for development.

This means:

- There is no real website yet.
- There is no business logic yet.
- There is only a structure of empty projects.

This is intentional. It gives us a solid foundation to build on.
Beginners can look at the structure and understand where everything will live.

---

## The projects in this solution

The solution has **five projects**. Here they are:

| Project name | Folder | What it will hold |
| --- | --- | --- |
| TalebElm.Domain | `src/TalebElm.Domain` | The core rules of the platform |
| TalebElm.Application | `src/TalebElm.Application` | The work the platform does |
| TalebElm.Infrastructure | `src/TalebElm.Infrastructure` | Databases and outside services |
| TalebElm.Api | `src/TalebElm.Api` | The web API that people call |
| TalebElm.Tests | `tests/TalebElm.Tests` | Tests that check the code works |

Do not worry if these words sound confusing. Read the file called
`ARCHITECTURE.md`. It explains each project in simple words.

---

## What you need before you start

You need **one main tool**: the .NET SDK.

The .NET SDK is a free program made by Microsoft. It lets you build and run
.NET projects on your computer.

### Step 1: Install the .NET SDK

Go to this website:

```
https://dotnet.microsoft.com/download
```

Download the **.NET SDK** (not the runtime). The SDK includes the runtime.

Then follow the install instructions for your computer.

### Step 2: Check that it works

Open a terminal (Command Prompt on Windows, Terminal on macOS, or your terminal on Linux).

Type this command and press Enter:

```
dotnet --version
```

You should see a version number, like this:

```
10.0.0
```

If you see a number, the SDK is installed correctly.
If you see an error, try installing again, then close and reopen your terminal.

### Step 3: Get a code editor (optional but recommended)

A code editor is a program that helps you read and edit code.
Any of these work well with .NET:

- **Visual Studio** (Windows only)
- **Visual Studio Code** (free, works on all systems)
- **Rider** by JetBrains

---

## How to run this project locally

Follow these steps in order.

### Step 1: Clone the repository

Clone means "download a copy to your computer".
Open your terminal and type:

```
git clone https://github.com/Twajjeh/TalebElm.git
```

Then move into the project folder:

```
cd TalebElm
```

### Step 2: Restore packages

Packages are small pieces of code made by other people. .NET downloads them
for you. This step is called "restore". Type:

```
dotnet restore
```

### Step 3: Build the solution

Build means "turn the code into a working program".
It also checks that there are no errors. Type:

```
dotnet build
```

You should see a message that says **Build succeeded**.

### Step 4: Run the API

The API is the web part of the project. To start it, type:

```
dotnet run --project src/TalebElm.Api
```

The program will start and print a web address (URL). It looks like this:

```
http://localhost:5000
```

Open that address in your web browser. You should see a page or a JSON response.
This means your API is running.

To stop the API, go back to the terminal and press `Ctrl + C`.

### Step 5: Run the tests

Tests are small programs that check that our code does what it should.
To run them, type:

```
dotnet test
```

You should see a message that says your tests passed.

---

## A note for beginners

Do not worry if you do not understand everything right now. That is normal.
The best way to learn is to do things one step at a time.

If you want to help with this project, read `CONTRIBUTING.md`. It explains
how to contribute in very simple words, and we will help you at every step.

---

## Project links

- **Main project**: `TalebElm.slnx` (the solution file, contains all projects)
- **Source code**: `src/` folder
- **Tests**: `tests/` folder
- **Architecture guide**: `ARCHITECTURE.md`
- **Contributing guide**: `CONTRIBUTING.md`
