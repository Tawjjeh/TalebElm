# Feature Specification: Center Management

## Status

Planned. The current codebase has no Center entity, persistence mapping, repository, service, DTO, or API endpoint.

## Scope

This feature is optional for the self-paced Track/Module journey and is not implemented. Include it in the MVP only if TalebElm must manage physical or partner learning centers. A Center is an organization/location record, not a learner account.

## Proposed Domain Contract If Approved

| Entity | Field | Type | Nullable | Notes |
|---|---|---|---|---|
| Center | Id | Guid | No | BaseEntity key |
| Center | Name | string | No | Required display name |
| Center | Description | string | Yes | Optional description |
| Center | Address | string | Yes | Optional physical address |
| Center | ContactEmail | string | Yes | Optional public contact |
| Center | IsActive | bool | No | Whether the center can be selected |
| Center | CreatedAt | DateTimeOffset | No | BaseEntity timestamp |

No Center type, configuration, repository, service, DTO, or endpoint currently exists.

## Proposed Application and API Contract

- `ICenterService`: create, get by id, list active centers, and update active state.
- `CreateCenterRequest`: Name plus optional Description, Address, and ContactEmail.
- `CenterResponse`: Id, Name, optional Description/Address/ContactEmail, IsActive.
- Proposed routes: `POST /api/centers`, `GET /api/centers`, `GET /api/centers/{centerId}`, `PUT /api/centers/{centerId}`.
- Do not add delete until behavior for Rooms, Tracks, and enrollments is defined.

## Contracts to Define

- Center fields, validation rules, and lifecycle.
- Repository and application-service responsibilities.
- API operations, request/response DTOs, and authorization.
- Whether a Center owns Tracks, Rooms, or both.
- Whether a learner belongs to a Center or only enrolls in Tracks.
- Which staff roles may create or update a Center.
- Whether Tracks are global or scoped to one Center.

No entity fields, relationships, service methods, authorization rules, or endpoint paths are finalized. Agree on them before implementation; do not treat this document as existing behavior.
