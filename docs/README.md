# TalebElm documentation

This is the documentation index for `Tawjjeh/TalebElm`.

## Start here

| If you want to… | Read |
|---|---|
| Understand the product and learner journey | [`README.md`](../README.md), [`docs/MVP_LEARNING_JOURNEY.md`](MVP_LEARNING_JOURNEY.md) |
| Understand code layers and conventions | [`ARCHITECTURE.md`](../ARCHITECTURE.md), [`docs/PROJECT_STRUCTURE.md`](PROJECT_STRUCTURE.md) |
| See what is implemented vs. planned | [`docs/FEATURE_INDEX.md`](FEATURE_INDEX.md) |
| Find a feature specification | [`docs/features/01-auth-identity.md`](features/01-auth-identity.md) … [`features/10-automated-task-verification.md`](features/10-automated-task-verification.md) |
| Follow the product roadmap | [`docs/business/ROADMAP.md`](business/ROADMAP.md) |
| Learn the business rules and constraints | [`docs/business/BUSINESS_RULES.md`](business/BUSINESS_RULES.md) |
| Check open product decisions | [`docs/OPEN_DECISIONS.md`](OPEN_DECISIONS.md) |
| Understand the API surface | [`docs/API_ENDPOINTS.md`](API_ENDPOINTS.md) |
| Understand the data model | [`docs/DATABASE_ERD.md`](DATABASE_ERD.md) |
| Set up the local database | [`docs/development/getting-started.md`](development/getting-started.md) |
| Look up labels | [`docs/GITHUB_LABELS.md`](GITHUB_LABELS.md) |
| See how tasks are verified | [`docs/TASK_TEMPLATE_SPEC.md`](TASK_TEMPLATE_SPEC.md) |

## Conventions

- New documentation is written in English and linked from this index in the same pull request.
- Permanent product rules live in [`docs/business/BUSINESS_RULES.md`](business/BUSINESS_RULES.md) or [`docs/OPEN_DECISIONS.md`](OPEN_DECISIONS.md), not in feature code comments.
- Feature docs mark behavior as `Implemented`, `Target`, or `Proposed`. Do not read a feature doc as proof that its endpoints are implemented.
- Update Markdown links when a file is renamed; keep relative links inside `docs/`.
- Keep secrets, personal data, real connection strings, hostnames, and private URLs out of all files.

## Validation

- `python3 scripts/check-docs.py` checks the files listed here and all relative Markdown links.
- Documentation is updated in the same pull request as code when behavior changes.

## Related root files

- [`README.md`](../README.md), [`ARCHITECTURE.md`](../ARCHITECTURE.md), [`CONTRIBUTING.md`](../CONTRIBUTING.md), [`LICENSE`](../LICENSE)
- [`AGENTS.md`](../AGENTS.md) — operating contract for AI coding assistants and contributors

## Complete file index

- [`ARCHITECTURE.md`](../ARCHITECTURE.md)
- [`API_ENDPOINTS.md`](API_ENDPOINTS.md)
- [`CLASS_FLOW.md`](CLASS_FLOW.md)
- [`DATABASE_ERD.md`](DATABASE_ERD.md)
- [`FEATURE_INDEX.md`](FEATURE_INDEX.md)
- [`GITHUB_LABELS.md`](GITHUB_LABELS.md)
- [`GLOSSARY.md`](GLOSSARY.md)
- [`LOCAL_SETUP.md`](LOCAL_SETUP.md)
- [`MVP_LEARNING_JOURNEY.md`](MVP_LEARNING_JOURNEY.md)
- [`OPEN_DECISIONS.md`](OPEN_DECISIONS.md)
- [`PROJECT_STRUCTURE.md`](PROJECT_STRUCTURE.md)
- [`TASK_TEMPLATE_SPEC.md`](TASK_TEMPLATE_SPEC.md)
- [`USER_STORIES.md`](USER_STORIES.md)
- [`business/BUSINESS_RULES.md`](business/BUSINESS_RULES.md)
- [`business/PRODUCT_VISION.md`](business/PRODUCT_VISION.md)
- [`business/ROADMAP.md`](business/ROADMAP.md)
- [`business/SUSTAINABILITY.md`](business/SUSTAINABILITY.md)
- [`business/WORKFLOWS.md`](business/WORKFLOWS.md)
- [`decisions/0000-adr-template.md`](decisions/0000-adr-template.md)
- [`decisions/README.md`](decisions/README.md)
- [`development/coding-standards.md`](development/coding-standards.md)
- [`development/definition-of-done.md`](development/definition-of-done.md)
- [`development/getting-started.md`](development/getting-started.md)
- [`development/testing.md`](development/testing.md)
- [`features/01-auth-identity.md`](features/01-auth-identity.md)
- [`features/02-tracks-content.md`](features/02-tracks-content.md)
- [`features/03-lessons-and-learning-resources.md`](features/03-lessons-and-learning-resources.md)
- [`features/04-exams-management.md`](features/04-exams-management.md)
- [`features/05-student-enrollment-and-progression.md`](features/05-student-enrollment-and-progression.md)
- [`features/06-resource-hub.md`](features/06-resource-hub.md)
- [`features/07-module-problems-and-verification.md`](features/07-module-problems-and-verification.md)
- [`features/08-health-and-error-handling.md`](features/08-health-and-error-handling.md)
- [`features/09-capstone-project.md`](features/09-capstone-project.md)
- [`features/10-automated-task-verification.md`](features/10-automated-task-verification.md)