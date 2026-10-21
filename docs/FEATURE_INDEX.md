# MVP Feature Index

This index separates current code from planned product behavior. A feature document must not be read as proof that its endpoints or business rules are already implemented.

| Document | Feature | Current state |
|---|---|---|
| [01-auth-identity.md](features/01-auth-identity.md) | User profiles and authentication | User fields (`PasswordHash`, `Role`), PBKDF2 password hashing, repositories, and unique email index exist; registration, login, tokens, and authorization policies are not implemented |
| [02-tracks-content.md](features/02-tracks-content.md) | Tracks, Modules, and Lessons | Core entities, Track list/create service, and ordered Module/Lesson read services exist; delivery APIs are placeholders |
| [03-lessons-and-learning-resources.md](features/03-lessons-and-learning-resources.md) | Lesson content and source links | Lesson text and `LessonResource` entity/configuration/repository/migration exist; lesson delivery endpoints are placeholders |
| [04-exams-management.md](features/04-exams-management.md) | Exams, questions, and results | Exam model/service contract and a placeholder ExamsController exist; question model, trusted grading, and create/submit behavior are missing |
| [05-student-enrollment-and-progression.md](features/05-student-enrollment-and-progression.md) | Track enrollment and per-module progress | `UserProgress` and `TrackEnrollment` persistence exist; the enrollment/unlock workflow and endpoints are not implemented |
| [08-health-and-error-handling.md](features/08-health-and-error-handling.md) | Health endpoint and exception mapping | Health route and registered exception middleware exist, including `409 Conflict`; most feature controllers still return `501` |

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
| [business/BUSINESS_RULES.md](business/BUSINESS_RULES.md) | Binding product rules with Implemented/Target status per rule |
| [business/WORKFLOWS.md](business/WORKFLOWS.md) | Business processes, state transitions, and failure map |
