# TalebElm labels

Fourteen labels in four groups. Every issue and pull request carries at most one type and one
layer. Work is grouped by the six MVP milestones (M0–M5): the milestone says when the work
happens, the layer says where the code goes.

## Type: what kind of change is this?

| Label | Use when |
|---|---|
| `bug` | Something is broken: a failing test, a wrong response, a crash. |
| `enhancement` | New behavior, an endpoint, a refactor, or a test that adds coverage. |
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

## Contribution: who can pick it up?

| Label | Use when |
|---|---|
| `good first issue` | The work is small, contained, and safe for a first contribution. It is not a difficulty guarantee and it does not replace the milestone. |

## Resolution and boundaries

| Label | Use when |
|---|---|
| `duplicate` | The same work exists elsewhere; link the surviving issue. |
| `wontfix` | The work is out of MVP scope or contradicts a recorded decision. |
| `security` | The change directly touches authentication, hashing, tokens, or a vulnerability. |
| `database` | The change directly touches a schema, a migration, or a data constraint. |

## How an issue becomes ready

An issue is ready to claim when all four hold:

1. it is open,
2. it has a milestone from M0 to M5,
3. it has no assignee,
4. it has no open pull request.

An issue with no milestone is undecided, not ready. An issue with an assignee, or with an open
pull request, is taken. A comment claims an issue; the work starts once an owner assigns it.
`good first issue` marks the small ones, so a first contribution can start there.

## Rules

- One type and one layer per item. A documentation-only item needs no layer. Add `security` or
  `database` only when the change directly touches them.
- Add the milestone that owns the work. Do not use feature labels; a milestone plus a layer is
  the whole scheme.
- Milestones strictly track MVP deliverables (M0–M5). Issues for post-MVP work or unconfirmed ideas carry no milestone until committed to the roadmap.
- Never invent a label. Propose a change in an issue first.
