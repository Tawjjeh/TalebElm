# How work moves through TalebElm

This page follows an issue from the moment it is opened to the moment its change is on
`main`. It describes what happens today: the board, the automation, and the decisions a
person still makes by hand.

## Statuses on the board

The public project board has five statuses:

| Status | What it means |
|---|---|
| Backlog | New or not triaged yet, or an idea kept for later. No decision has been made. |
| Todo | Accepted and ready for anyone to start: it has an MVP milestone and no open pull request. |
| In progress | Someone is working on it. A draft pull request or a branch exists. |
| In review | The pull request is ready for review. |
| Done | Merged or closed. |

The **Active** tab on the repository issue list shows open items whose status is Todo, In
progress, or In review. It is the work in flight, not the whole backlog: Backlog and items
with no status do not appear there. The board is curated by hand, so a status can lag
behind the thread for a few minutes.

If the board is not available in your browser, the issue list is the source of truth for
whether work is free to take. This query finds the same issues:

```text
is:issue is:open no:assignee -linked:pr -no:milestone NOT "[Tracking]" in:title
```

## For a contributor

1. Find work in Todo, or run the query above. `good first issue` marks the small ones.
2. Comment on the issue with what you plan to change and which files you expect to touch.
   That comment claims it. Do not wait for an assignment.
3. Start right away. Open a draft pull request within 7 days, with `Closes #NNN` in the
   body. Two claimed issues at a time is the limit.
4. Branch from fresh `upstream/main`, keep one vertical slice (about 200 lines including
   tests), and run the three checks before asking for review.
5. Mark the pull request ready for review when it is complete. A reviewer is requested
   automatically.
6. Answer review comments on the branch. Push new commits; there is no need to close the
   pull request or force-push.
7. A reviewer merges. Authors do not merge their own pull requests.

The first pull request that meets the definition of done wins. If two arrive for one issue,
the earlier one is reviewed first.

## For a regular reviewer

`Asma-Elhadiedy` and `Mohammad-Metwally` review the non-sensitive pull requests. A workflow
requests one of them automatically when a draft becomes ready, choosing the one with fewer
open review requests. The request is a request for review, not an assignment of the work.

- Read the linked issue, the whole diff, and the checks.
- Approve, or request changes with the blocking points in order and a concrete fix for each.
- Merge a non-sensitive pull request with squash when all of these hold: one approval on
  the latest commit, `build-and-test` green, conversations resolved, and the linked issue
  valid. The branch does not have to be up to date, because `main` does not require it.
- Do not merge a security or database pull request. Those wait for a core reviewer.
- Plain `documentation`, `chore`, and workflow pull requests from the owner may be merged by
  the owner with the admin bypass when the only missing requirement is the approval.

## For the two core reviewers

The owner (`abdulrahmanhossam`) and `abdulla7mohamed` share security and database work.

- A pull request that touches migrations, persistence configuration, `AppDbContext`,
  `IUnitOfWork`, repository queries, authentication or authorization, grading, external
  verification, HTTP clients, secrets, configuration, or packages needs one of them.
- They alternate one at a time: one reviews, the other does not duplicate the work. One
  approval is enough. The owner merges; a core reviewer other than the owner posts the
  approval and leaves the merge to the owner.
- The owner may merge any pull request. Contributors never merge.
- If a security or database pull request is ready, say so in a comment so the next core
  reviewer sees it without opening the whole thread.

## What moves by itself, and what does not

Automatic:

- The issue forms add the type label, and the triage workflow adds the layer label from the
  form answer. It never adds a second layer label.
- A workflow requests a regular or core reviewer for each ready pull request.
- `build-and-test` runs on every pull request and on `main`: Debug and Release build, the
  OpenAPI document check, the test suite, and the documentation links.
- Merging a pull request with `Closes #NNN` closes the issue.
- The stale-questions workflow reminds after 30 idle days and closes after 60. It skips
  assigned issues, tracking issues, and issues labelled `security`.

By hand:

- Moving a card between Backlog, Todo, In progress, In review, and Done.
- Accepting an issue: adding the milestone and the layer, and removing duplicate or
  out-of-scope work.
- Reviewing, approving, merging, reopening, and closing.
- Updating a recorded decision when the rule itself changes.

## When something goes wrong

| Situation | What the reviewer does |
|---|---|
| CI fails | Read the failing job, name the failing test or step in one comment, and say what to change. Do not re-run blindly. |
| The branch has conflicts | Ask the author to rebase on `upstream/main` and resolve them in the branch. |
| The branch is behind `main` only | It can still merge. If a check needs the newer base, update the branch once and wait for the checks. |
| There is no linked issue | Ask for the issue link before reviewing further. Small doc fixes may proceed when the change is obvious. |
| It duplicates another pull request | Point to the earlier pull request and to another Todo issue, kindly. |
| The author is silent | Nudge once after 14 days. After 28 days, list it for the owner. Never close someone else's pull request. |
| The author started before the issue was in Todo | If the work is good and fits the milestone, treat it as accepted and say so. |

## Who decides

Product direction, scope, and architecture belong to the owner. Permanent rules live in
`docs/business/BUSINESS_RULES.md` and `docs/OPEN_DECISIONS.md`; a change to a recorded rule
is written there in the same pull request. Reviewers decide whether a change meets the
definition of done, not what the product should be. When the two disagree, the recorded
decision wins until it is changed in the open.
