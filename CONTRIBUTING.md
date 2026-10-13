# Contributing to TalebElm

Thank you for wanting to help! You do not need to be an expert. TalebElm is a
place to **learn**. Making mistakes here is okay and even expected. We all had
to learn somewhere. This guide is written for absolute beginners, so we will
explain every command.

---

## Some friendly words first

If this is your first time working on an open-source project, this guide will
walk you through every single step. Please do not worry if:

- You do not understand something.
- Your first pull request is not perfect.
- You break something by accident (that is how we learn).

We would rather have you here making small steps than not trying at all.
If you are stuck at any point, create an **issue** asking for help, or write a
comment on an open pull request. Someone will answer you.

---

## Important words you will see

Here are four words you need to know. We use them all the time.

### Repository (repo)

A **repository** is a folder that contains all the project's files and its
whole history. It lives on GitHub. Our repository is called `TalebElm`.

### Fork

A **fork** is a personal copy of someone else's repository. It is still on
GitHub, but it belongs to your account. You can change it freely without
touching the original.

### Pull Request (PR)

A **pull request** is a request you send that says:
"I made changes in my copy. Please pull my changes into the original project."

A pull request is simply a proposal. Maintainers (the people in charge) review
it. They may say "great, thanks!" and accept it, or they may ask you to change
something. After it is accepted, your changes become part of the project.

### Branch

A **branch** is a separate line of work. Imagine the main line is called `main`.
You can create your own branch to work on something without breaking `main`.
When your branch is good, you can merge it into `main`.

---

## Before you start: check your tools

You need `git` installed. Check it works. Open a terminal and type:

```
git --version
```

If you see a version number, git is ready. If you get an error, install git from
`https://git-scm.com/downloads` and try again.

---

## Full step-by-step to make your first contribution

Do these steps in order. Take your time.

### Step 1: Create a GitHub account

If you do not have a GitHub account, create one at `https://github.com`.
It takes two minutes and it is free.

### Step 2: Fork the project

1. Go to the project page:
   `https://github.com/Twajjeh/TalebElm`
2. In the top-right corner, click the button that says **Fork**.
3. GitHub makes a copy under your own account.

> Your own copy is now at an address like:
> `https://github.com/YourName/TalebElm` .
> "YourName" will be your user name.

### Step 3: Open your terminal

Open a terminal on your computer:

- **Windows**: open "Command Prompt" or "PowerShell".
- **macOS / Linux**: open "Terminal".

### Step 4: Clone your fork

Clone means "download a copy to my computer". This time we clone your **fork**,
not the original:

```
git clone https://github.com/YourName/TalebElm.git
```

Now move into the folder:

```
cd TalebElm
```

### Step 5: Add the original project as "upstream"

We want your copy to also know about the original project. This keeps us in
touch. The original project is called **upstream**. Run:

```
git remote add upstream https://github.com/Twajjeh/TalebElm.git
```

To check it worked, run:

```
git remote -v
```

You should see two entries like:

```
origin    https://github.com/YourName/TalebElm.git
upstream  https://github.com/Twajjeh/TalebElm.git
```

### Step 6: Make a branch for your work

We never make changes directly on `main`. We always use a new branch.
The branch name should say what you are doing. For example:

```
git checkout -b feature/add-readme-links
```

A good branch name starts with one of these words:

- `feature/` when you add something new (a feature).
- `bugfix/` when you fix a problem (a bug).
- `docs/` when you change documentation (like these .md files).

Examples of good names:

- `feature/roadmap-lessons`
- `bugfix/fix-login-error`
- `docs/improve-arch-guide`

### Step 7: Make your changes

Now edit the files. For example, if you are writing documentation, open the
`README.md` file in your code editor and change it.

You can see what changed by typing:

```
git status
```

This shows which files are different from before.

### Step 8: Add and save your changes (commit)

First, we "stage" the files. This tells git which changes we want to save.
To add a specific file:

```
git add README.md
```

To add all changed files at once:

```
git add .
```

Then we save a snapshot with a message. This is called a **commit**:

```
git commit -m "docs: add links to the readme"
```

Commit messages should be short and say what you did. A good format is:

```
docs: explain how to run the project
feat: add a new learning lesson
fix: correct a wrong spelling
test: add a test for the Track class
```

The first word (like `docs:`) tells people what type of change it is.
Use one of these accepted types (a robot checks your pull request title
and will ask you to fix it if the type is wrong):

- `feat:` — a new feature (example: `feat: add login endpoint`)
- `fix:` — a bug fix (example: `fix: correct exam score`)
- `chore:` — routine work like updates or cleanups
- `docs:` — documentation only
- `refactor:` — code restructuring with no behavior change
- `test:` — adding or updating tests
- `ci:`, `build:`, `perf:`, `style:` — pipelines, builds, performance, formatting

You can add an optional scope in brackets: `fix(auth): hash passwords`.

### Pull request titles and linked issues

Give your pull request a title in the same format, for example
`feat: add login endpoint`. If your pull request is a `feat:` or a `fix:`,
also write `Closes #123` (with your issue number) in the description so
the issue closes automatically. Small `chore:` or `docs:` updates do not
need an issue link.

### Step 9: Push your branch to GitHub

"Push" means "upload my commits to GitHub". Run:

```
git push origin feature/add-readme-links
```

If it is your first time pushing this branch, git might ask you to use:
```
git push --set-upstream origin feature/add-readme-links
```

### Step 10: Create a Pull Request

1. Go to your fork on GitHub:
   `https://github.com/YourName/TalebElm`
2. You will see a yellow bar that says something like
   "Compare & pull request" for your new branch. Click it.
3. Check that these are correct:
   - **Base repository**: `Twajjeh/TalebElm`
   - **base**: `main`
   - **Head repository**: `YourName/TalebElm`
   - **compare**: `feature/add-readme-links`
4. Add a title and a short description of what you did.
5. Click **Create pull request**.

Now the maintainers can see your work. They might message you. You can always
push more changes to the same branch and they will update the pull request
automatically.

---

## When you want to start a second task later

Your fork and your main branch may fall behind the original project. To update
them, run:

```
git checkout main
git fetch upstream
git merge upstream/main
git push origin main
```

Then create a fresh branch for the new task using Step 6 again.

---

## Rules for good contributions

These simple rules make the project clean for everyone.

- One change per pull request. Do not bundle many things together.
- Use clear branch names and commit messages.
- Put your code in the right project. If you are not sure where a file belongs,
  read `ARCHITECTURE.md` (it explains each layer clearly).
- Do not commit secret things like passwords or connection strings.
- Keep the code simple. Simpler is better, especially for a learning project.

---

## Mistakes are okay

Publishing a project means growing together. Your pull request will not be
rejected just because it is small or imperfect. If maintainers ask you to change
something, it is not a bad thing. It means they care about teaching you.

Ask us. Use questions, comments, and open pull requests.
We are all here to learn.

Thank you for being part of TalebElm. We are happy you want to learn with us.