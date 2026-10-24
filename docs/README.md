# TalebElm documentation index

> Public documentation map for `Tawjjeh/TalebElm`.
> Status discipline: every behavior is `Implemented`, `Target`, or `Proposed`. See
> `docs/FEATURE_INDEX.md` for current implementation status.
> Last updated: 2026-10-25.

## Start here

| If you want to… | Read |
|---|---|
| Understand the product and learner journey | `README.md` (root), `docs/MVP_LEARNING_JOURNEY.md` |
| Understand code layers and conventions | `docs/ARCHITECTURE.md`, `docs/PROJECT_STRUCTURE.md` |
| See what is implemented vs. planned | `docs/FEATURE_INDEX.md` |
| Find a feature specification | `docs/features/01-auth-identity.md` … `10-automated-task-verification.md` |
| Follow the product roadmap | `docs/business/ROADMAP.md` |
| Learn the business rules and constraints | `docs/business/BUSINESS_RULES.md` |
| Check open product decisions | `docs/OPEN_DECISIONS.md` |
| Understand the API surface | `docs/API_ENDPOINTS.md` |
| Understand the data model | `docs/DATABASE_ERD.md` |
| Set up the local database | `docs/LOCAL_SETUP.md` |
| Look up labels and issue status | `docs/GITHUB_LABELS.md` |
| See how tasks are verified | `docs/TASK_TEMPLATE_SPEC.md` |

## Documentation areas

| Area | Files |
|---|---|
| Architecture | `ARCHITECTURE.md`, `CLASS_FLOW.md`, `PROJECT_STRUCTURE.md` |
| Domain features | `features/01-…10-*.md` |
| Product and business | `business/PRODUCT_VISION.md`, `business/BUSINESS_RULES.md`, `business/WORKFLOWS.md`, `business/ROADMAP.md`, `business/SUSTAINABILITY.md` |
| Data | `DATABASE_ERD.md` |
| API | `API_ENDPOINTS.md` |
| Developer setup | `LOCAL_SETUP.md`, `development/coding-standards.md`, `development/definition-of-done.md` |
| Decisions and ADRs | `OPEN_DECISIONS.md`, `decisions/README.md`, `decisions/0000-adr-template.md` |
| Glossary | `GLOSSARY.md` |
| Tracks / issues | `USER_STORIES.md`, `TASK_TEMPLATE_SPEC.md`, `GITHUB_LABELS.md` |

## Conventions

- New docs are written in English and linked from this index in the same pull request.
- Permanent product rules live in `docs/business/BUSINESS_RULES.md` or `docs/OPEN_DECISIONS.md`, not in feature code comments.
- Feature docs mark behavior as `Implemented`, `Target`, or `Proposed`. Do not read a feature doc as proof that its endpoints are implemented.
- Architecture decisions use an ADR in `docs/decisions/` (template: `docs/decisions/0000-adr-template.md`).
- Update the Markdown link when a file is renamed; keep relative links inside `docs/`.
- Keep secrets, personal data, real connection strings, hostnames, and private URLs out of all of these files.

## Validation

- `python3 scripts/check-docs.py` checks the files listed here and all relative Markdown links.
- Documentation is updated in the same pull request as code when behavior changes.

## Related root files

- `README.md`, `ARCHITECTURE.md`, `CONTRIBUTING.md`, `CONTRIBUTORS.md`, `LICENSE`
- `AGENTS.md` — operating contract for AI coding assistants and contributors
