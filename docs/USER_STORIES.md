# User Stories and Acceptance Criteria

These stories describe the MVP target. They are not a claim that the behavior exists. Current status per feature is in [FEATURE_INDEX.md](FEATURE_INDEX.md).


## Actors

- **Student** — a registered user who studies a Track (the learner).
- **Instructor** — a future teaching/authoring role (defined in `UserRole`; reserved, with no distinct MVP permissions).
- **Admin** — a content author who creates Tracks, Modules, Lessons, and Exams (defined in `UserRole`).

## Student Stories

### L1. Register and sign in
As a Student (the learner), I want to create an account and sign in so my progress is saved.
- Registration needs name, email, and password; a duplicate email is rejected.
- The password is never stored raw or returned in a response.
- Protected routes return `401` without a valid token.
- Feature: [01-auth-identity.md](features/01-auth-identity.md)

### L2. Browse Tracks
As a Student, I want to see available Tracks and their Modules in order.
- Only `Published` Tracks are listed.
- Modules are returned sorted by `Order`.
- Feature: [02-tracks-content.md](features/02-tracks-content.md)

### L3. Enroll in a Track
As a Student, I want to choose a Track and start it.
- Enrolling creates one enrollment per Student and Track; a second attempt returns `409`.
- The first Module (lowest `Order`) is unlocked immediately; the rest stay locked.
- A Track with no Modules cannot be enrolled in, and nothing is created.
- Feature: [05-student-enrollment-and-progression.md](features/05-student-enrollment-and-progression.md)

### L4. Study a Module
As a Student, I want to read each Lesson and open its sources (books, documentation, videos).
- Lessons are returned in `Order` with their text and ordered resources.
- A resource has a title and either a URL or a citation.
- A locked Module's Lessons are not served.
- Feature: [03-lessons-and-learning-resources.md](features/03-lessons-and-learning-resources.md)

### L5. Pass the Module exam
As a Student, I want to take the Module exam to test my theoretical understanding.
- The exam shows questions and options but never which option is correct.
- The server calculates the score; a client-supplied score is ignored.
- `score >= PassThreshold` passes (`PassedExam = true`).
- A pass is sticky and cannot be revoked by a later failed retake.
- Passing the exam satisfies one of the two gates needed to unlock the next Module (the other being problem completion).
- Feature: [04-exams-management.md](features/04-exams-management.md)

### L6. See my progress
As a Student, I want to see which Modules are locked, open, or passed.
- Progress is returned per Module in Track order for the signed-in Student only.
- One Student never sees or changes another Student's progress.
- Both exam pass and problem completion flags are shown per Module.
- Feature: [05-student-enrollment-and-progression.md](features/05-student-enrollment-and-progression.md)

### L7. Link and verify my Codeforces handle
As a Student, I want to link my Codeforces account without sharing private credentials so my practice solves can be verified.
- The platform generates a random single-use token (at least 128 bits, expires in 15 minutes).
- I temporarily place the token in my Codeforces profile field (e.g., first name or organization).
- The platform verifies the token via the public Codeforces API (`user.info`) and stores my verified handle with a UNIQUE constraint (one handle per user).
- I can remove the token from my profile afterwards.
- Feature: [07-module-problems-and-verification.md](features/07-module-problems-and-verification.md)

### L8. Solve practice problems and unlock the next Module
As a Student, I want to solve assigned Codeforces problems and have my solves verified directly by the platform.
- Problems are displayed in order with direct links.
- Clicking "Verify Solves" checks the Codeforces API (`user.status`) for accepted submissions (`verdict == OK`).
- A per-user cooldown (60 seconds) prevents API abuse; if Codeforces is down, a 503 response is returned and no learner state is altered.
- All-time accepted solves count and verified solves are sticky (never revoked).
- Once all required problems are verified AND the exam is passed, the next Module unlocks automatically.
- Feature: [07-module-problems-and-verification.md](features/07-module-problems-and-verification.md)

### L9. Complete framework practical tasks (GitHub)
As a Student, I want to complete practical framework tasks with fully automated verification so I can build real projects without waiting for human review.
- I link my GitHub account via OAuth (read-only identity scope; immutable numeric user ID is stored).
- I create a public repository from the task's official starter template repository (`Tawjjeh/task-<slug>-template`).
- I implement the solution and push commits to the default branch, triggering GitHub Actions tests.
- When I click "Verify", the platform verifies via GitHub REST API: repository owner matches my linked ID, repository was generated from the official template, latest workflow run succeeded, and cryptographic SHAs of the workflow and `tests/` match the official `TaskVersion`.
- Upon successful verification, the task completion is recorded stickily and contributes to module unlocking.
- Feature: [10-automated-task-verification.md](features/10-automated-task-verification.md)

### L10. Browse the Public Resource Hub
As any learner or visitor, I want to find high-quality learning resources without logging in.
- I can browse Language $\to$ Framework $\to$ Topic in $\le$ 3 clicks.
- I can filter resources by human language (Arabic, English), difficulty level, format kind, and free vs. paid.
- Feature: [06-resource-hub.md](features/06-resource-hub.md)

### L11. Build cumulative Track Capstone project
As an advanced Student, I want to build a cumulative portfolio project across the entire Track with automated milestone verification.
- The capstone project is split into ordered milestones, each referencing an automated `GitHubTask`.
- I complete and verify each milestone in order using the automated GitHub task verifier.
- Track completion is derived automatically: all Modules passed AND all capstone Milestones verified.
- Optional peer or mentor feedback may be requested, but it never blocks progression or completion.
- Feature: [09-capstone-project.md](features/09-capstone-project.md)

## Admin Stories

### A1. Manage Tracks, Modules, and Lessons
As an admin, I want to create and order learning content.
- A new Track starts as `Draft` and becomes visible only when `Published`.
- Name, description, and title fields are required.
- Resources are attached to a Lesson, not embedded in its text.

### A2. Manage Module exams
As an admin, I want to attach one exam to a Module.
- An exam has a title, a pass threshold, and multiple-choice questions.
- Each question has at least two options and exactly one correct option.
- A Module has at most one exam (`409` on a duplicate).

### A3. Manage Module practice problems
As an admin, I want to attach Codeforces problems and GitHub tasks to a Module.
- Each problem has a kind (`CodeforcesProblem` or `GitHubTask`), title, order, and `IsRequired` flag.
- Codeforces problems store the problem index, contest ID, and cached difficulty rating.
- GitHub tasks reference an official template repository and `TaskVersion`.
- Problem ordering is contiguous within the Module.
- Feature: [07-module-problems-and-verification.md](features/07-module-problems-and-verification.md), [10-automated-task-verification.md](features/10-automated-task-verification.md)

### A4. Manage Resource Hub taxonomy and resources
As an admin, I want to curate the public Resource Hub.
- Create hierarchical technology nodes (`Language`, `Framework`, `Topic`, `Tool`).
- Add curated resources with metadata (level, language, cost, citation, URL).
- Moderate community submissions by approving or rejecting with notes.
- Feature: [06-resource-hub.md](features/06-resource-hub.md)

### A5. Define Track Capstones and milestones
As an admin, I want to define cumulative capstone projects with automated milestones.
- Attach a capstone project definition to a Track with architectural guidelines.
- Define ordered milestones, each referencing an official `GitHubTask` with starter template and test suite.
- Progression and track completion are 100% automated; no manual review queue is required.
- Feature: [09-capstone-project.md](features/09-capstone-project.md)

## Out of Scope

Scheduling, attendance, live streaming, in-browser IDE sandbox, and automated contest hosting.
