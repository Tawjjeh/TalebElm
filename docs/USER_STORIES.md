# User Stories and Acceptance Criteria

These stories describe the MVP target. They are not a claim that the behavior exists. Current status per feature is in [FEATURE_INDEX.md](FEATURE_INDEX.md).

## Actors

- **Learner** — a registered user who studies a Track.
- **Admin** — a content author who creates Tracks, Modules, Lessons, and Exams (role is proposed; see [OPEN_DECISIONS.md](OPEN_DECISIONS.md)).

## Learner Stories

### L1. Register and sign in
As a learner, I want to create an account and sign in so my progress is saved.
- Registration needs name, email, and password; a duplicate email is rejected.
- The password is never stored raw or returned in a response.
- Protected routes return `401` without a valid token.
- Feature: [01-auth-identity.md](features/01-auth-identity.md)

### L2. Browse Tracks
As a learner, I want to see available Tracks and their Modules in order.
- Only `Published` Tracks are listed.
- Modules are returned sorted by `Order`.
- Feature: [02-tracks-content.md](features/02-tracks-content.md)

### L3. Enroll in a Track
As a learner, I want to choose a Track and start it.
- Enrolling creates one enrollment per learner and Track; a second attempt returns `409`.
- The first Module (lowest `Order`) is unlocked immediately; the rest stay locked.
- A Track with no Modules cannot be enrolled in, and nothing is created.
- Feature: [05-student-enrollment-and-progression.md](features/05-student-enrollment-and-progression.md)

### L4. Study a Module
As a learner, I want to read each Lesson and open its sources (books, documentation, videos).
- Lessons are returned in `Order` with their text and ordered resources.
- A resource has a title and either a URL or a citation.
- A locked Module's Lessons are not served.
- Feature: [03-lessons-and-learning-resources.md](features/03-lessons-and-learning-resources.md)

### L5. Pass the Module exam
As a learner, I want to take the Module exam to move forward.
- The exam shows questions and options but never which option is correct.
- The server calculates the score; a client-supplied score is ignored.
- `score >= PassThreshold` passes. A pass unlocks only the next Module.
- A fail records the score, unlocks nothing, and the learner may retake.
- Passing the last Module unlocks nothing further.
- Feature: [04-exams-management.md](features/04-exams-management.md)

### L6. See my progress
As a learner, I want to see which Modules are locked, open, or passed.
- Progress is returned per Module in Track order for the signed-in learner only.
- One learner never sees or changes another learner's progress.
- Feature: [05-student-enrollment-and-progression.md](features/05-student-enrollment-and-progression.md)

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

## Out of Scope

Centers, Rooms, scheduling, attendance, payments, certificates, and exam attempt history. See [06-center-management.md](features/06-center-management.md) and [07-room-management.md](features/07-room-management.md) for the optional features.
