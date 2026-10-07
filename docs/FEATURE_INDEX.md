# MVP Feature Index

This index separates current code from planned product behavior. A feature document must not be read as proof that its endpoints or business rules are already implemented.

| Document | Feature | Current state |
|---|---|---|
| [01-auth-identity.md](features/01-auth-identity.md) | User profiles and authentication | User fields and service contracts exist; authentication and user API behavior are placeholders |
| [02-tracks-content.md](features/02-tracks-content.md) | Tracks, Modules, and Lessons | Core entities and Track list/create service exist; Module/Lesson delivery APIs are placeholders |
| [03-lessons-and-learning-resources.md](features/03-lessons-and-learning-resources.md) | Lesson content and source links | Lesson text exists; resource metadata is proposed and not implemented |
| [04-exams-management.md](features/04-exams-management.md) | Exams, questions, and results | Exam model/service contract exist; question model, trusted grading, create/submit behavior, and controller are missing |
| [05-student-enrollment-and-progression.md](features/05-student-enrollment-and-progression.md) | Track enrollment and per-module progress | UserProgress exists; TrackEnrollment and the unlock workflow are not implemented |
| [06-center-management.md](features/06-center-management.md) | Learning center administration | Planned; no code contract exists |
| [07-room-management.md](features/07-room-management.md) | Center rooms (records only; booking is out of scope) | Planned; no code contract exists |
| [08-health-and-error-handling.md](features/08-health-and-error-handling.md) | Health endpoint and exception mapping | Health route exists; middleware is an empty, unregistered placeholder |

## Other Documents

| Document | Purpose |
|---|---|
| [MVP_LEARNING_JOURNEY.md](MVP_LEARNING_JOURNEY.md) | End-to-end learner flow, core rules, and current implementation gaps |
| [USER_STORIES.md](USER_STORIES.md) | Learner and content-admin stories with acceptance criteria |
| [OPEN_DECISIONS.md](OPEN_DECISIONS.md) | Product/technical decisions to settle before implementation |
| [API_ENDPOINTS.md](API_ENDPOINTS.md) | Current routes, target MVP routes, and who may call them |
| [DATABASE_ERD.md](DATABASE_ERD.md) | Current and proposed data model |
| [CLASS_FLOW.md](CLASS_FLOW.md) | Student flow and layer boundaries |
| [ARCHITECTURE.md](ARCHITECTURE.md) | Technical conventions for contributors |
| [PROJECT_STRUCTURE.md](PROJECT_STRUCTURE.md) | Current source layout |
| [LOCAL_SETUP.md](LOCAL_SETUP.md) | Local database connection string |
