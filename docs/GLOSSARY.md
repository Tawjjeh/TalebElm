# Glossary

Terms used consistently across TalebElm code and docs.

| Term | Meaning |
|---|---|
| Track | An ordered learning path. The product never calls this a "course". |
| Module | One ordered section of a Track, with `Order >= 1`. |
| Lesson | One ordered study unit inside a Module. |
| LessonResource | A cited source attached to a Lesson. |
| Exam | At most one assessment per Module; a Module may have none. |
| UserProgress | One learner's state for one Module and the unique `(UserId, ModuleId)` row. |
| TrackEnrollment | A learner's explicit choice of a Track, separate from per-Module progress. |
| Student | A registered learner (`UserRole.Student`). Default role after registration. |
| Admin | Content author/operator. May also act as a learner. |
| Instructor | Reserved role (`UserRole.Instructor`); no MVP permission differs from Student. |
| Learner | Product concept, not a stored role, token claim, or role value. |
| Implemented / Target / Proposed | Documentation status markers. See `docs/FEATURE_INDEX.md`. |
| Zero manual gating | Every progression step is verified by the server, never by a human. |
| Dual-gated unlock | A Module unlocks when both required exam and required practice problems are complete. |

| TaskVersion | Immutable Git commit/tree SHA values used for task integrity. |
| Technology | Hierarchical node in the Resource Hub tree. |
| Resource | Unified citation used by lessons and the Resource Hub. |
| ModuleProblem | Practice problem or framework task attached to a Module. |
| Verified handle | Codeforces handle owned via the single-use token. |
| Sticky completion | Once true, never reverted. |
