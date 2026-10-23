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
- The platform issues a challenge: submit code with a compilation error to a designated problem within 30 minutes.
- The platform verifies the submission via the public Codeforces API and marks the account verified.
- Feature: [07-module-problems-and-verification.md](features/07-module-problems-and-verification.md)

### L8. Solve practice problems and unlock the next Module
As a Student, I want to solve assigned Codeforces problems and have my solves verified directly by the platform.
- Problems are displayed in order with direct links.
- Clicking "Verify Solves" checks the Codeforces API for accepted submissions (`verdict == OK`).
- Once all required problems are verified AND the exam is passed, the next Module unlocks automatically.
- Feature: [07-module-problems-and-verification.md](features/07-module-problems-and-verification.md)

### L9. Submit framework practical tasks
As a Student, I want to submit a GitHub repository for practical framework modules.
- I provide a public GitHub repository URL and optional notes.
- Submissions are evaluated against a checklist by an Admin or mentor.
- Feature: [07-module-problems-and-verification.md](features/07-module-problems-and-verification.md)

### L10. Browse the Public Resource Hub
As any learner or visitor, I want to find high-quality learning resources without logging in.
- I can browse Language $\to$ Framework $\to$ Topic in $\le$ 3 clicks.
- I can filter resources by human language (Arabic, English), difficulty level, and free vs. paid.
- Feature: [06-resource-hub.md](features/06-resource-hub.md)

### L11. Deliver Track Capstone project
As an advanced Student who finished all Track modules, I want to build a comprehensive portfolio project.
- Capstone requirements and rubric are unlocked once all modules are passed and completed.
- I submit my public Git repository URL and optional live demo.
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
- Each problem has an external ID, title, URL, order, and `IsRequired` flag.
- Problem ordering is contiguous within the Module.
- Feature: [07-module-problems-and-verification.md](features/07-module-problems-and-verification.md)

### A4. Manage Resource Hub taxonomy and resources
As an admin, I want to curate the public Resource Hub.
- Create hierarchical technology nodes (`Language`, `Framework`, `Topic`, `Tool`).
- Add curated resources with metadata (level, language, cost, citation, URL).
- Moderate community submissions by approving or rejecting with notes.
- Feature: [06-resource-hub.md](features/06-resource-hub.md)

### A5. Define Track Capstones and review submissions
As an admin/mentor, I want to review capstone project submissions.
- Attach a capstone project definition and milestones to a Track.
- Review submitted Git repositories against the rubric and set status (`Approved` or `NeedsRevision`).
- Feature: [09-capstone-project.md](features/09-capstone-project.md)

## Out of Scope

Scheduling, attendance, live streaming, in-browser IDE sandbox, and automated contest hosting.
