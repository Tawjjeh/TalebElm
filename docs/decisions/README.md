# Architecture decision records

Use an ADR when a decision is architectural, expensive to reverse, or
constrains multiple layers or features.

## When to open an ADR

- Adding a new dependency, queue, store, or hosting component.
- Changing a persistence invariant, tenancy boundary, or auth model.
- Changing the deployment state machine or a public API contract.

## How to add one

1. Copy `0000-adr-template.md`, name it `NNNN-<short-kebab>.md` (`0001-...`).
2. Mark the status `Proposed` until the owner accepts it.
3. Add a row to `docs/OPEN_DECISIONS.md` only when the decision is settled.
4. Link the ADR from affected archives in `docs/` and code comments where useful.

## Existing decisions

TalebElm records binding product decisions in `docs/OPEN_DECISIONS.md` and
rules in `docs/business/BUSINESS_RULES.md`. These ADR files are reserved for
technical decisions only; do not duplicate the same rule in both places.
