# TalebElm labels

Fifteen labels, in four groups. Every issue and pull request carries at most one type and one
layer. Work is grouped by the six MVP milestones (M0–M5), not by feature labels: the milestone
says where the work fits, the layer says where the code goes.

## Type: what kind of change is this?

| Label | Use when |
|---|---|
| `bug` | Something is broken: a failing test, a wrong response, a crash. |
| `enhancement` | New behavior, a new endpoint, a refactor, or a test that adds coverage. |
| `documentation` | A change to Markdown, templates, or the docs check. Needs no layer. |
| `question` | A decision or clarification is needed before code is written. |

## Layer: where does the code live?

| Label | Use when the change touches |
|---|---|
| `layer:domain` | Entities, enums, exceptions, repository interfaces, `IUnitOfWork`. |
| `layer:application` | Services, DTOs, validators, and client interfaces. |
| `layer:infrastructure` | EF Core, configurations, migrations, repositories, HTTP clients. |
| `layer:api` | Controllers, middleware, `Program.cs`, Swagger. |
| `layer:tests` | Unit, integration, and migration tests. |

## State: can someone pick it up?

| Label | Use when |
|---|---|
| `help wanted` | The issue is approved and free for a contributor to claim. |
| `good first issue` | It is also small, contained, and safe for a first contribution. |

## Resolution and boundaries

| Label | Use when |
|---|---|
| `duplicate` | The same work exists elsewhere; link the survivor. |
| `wontfix` | The work is out of MVP scope or contradicts a recorded decision. |
| `security` | Authentication, hashing, tokens, or a vulnerability report. |
| `database` | A schema change, a migration, or a data constraint. |

## Rules

- One type and one layer per item. A documentation-only item needs no layer; `security` and
  `database` are added only when the change directly touches them.
- Add the milestone that owns the work, and `help wanted` only when the issue is ready to claim.
  Never invent a label; propose a change in an issue first.
