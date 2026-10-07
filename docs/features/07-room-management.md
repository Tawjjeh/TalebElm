# Feature Specification: Room Management

## Status

Planned. The current codebase has no Room entity, persistence mapping, repository, service, DTO, or API endpoint.

## Scope

Room management is optional for the self-paced learning flow and is not implemented. Add it only if Centers need physical or virtual classrooms in the MVP. This spec covers room records only; booking/scheduling is a separate workflow.

## Proposed Domain Contract If Approved

| Entity | Field | Type | Nullable | Notes |
|---|---|---|---|---|
| Room | Id | Guid | No | BaseEntity key |
| Room | CenterId | Guid | No | FK to the owning Center, if Center Management is approved |
| Room | Name | string | No | Required name unique within its Center |
| Room | Capacity | int | No | Positive maximum number of people |
| Room | IsActive | bool | No | Whether the room is available for future use |
| Room | CreatedAt | DateTimeOffset | No | BaseEntity timestamp |

No Room type, repository, service, DTO, configuration, or endpoint exists in the current source.

## Proposed Application and API Contract

- `IRoomService`: create, get by id, and list rooms for a Center.
- `CreateRoomRequest`: Name and positive Capacity.
- `RoomResponse`: Id, CenterId, Name, Capacity, IsActive.
- Proposed routes: `POST /api/centers/{centerId}/rooms`, `GET /api/centers/{centerId}/rooms`, `GET /api/rooms/{roomId}`.
- Return a not-found response if the Center or Room does not exist.

## Contracts to Define

- Room fields, validation rules, and lifecycle.
- Whether rooms are physical, virtual, or both.
- Whether each Room belongs to exactly one Center.
- Whether scheduling and booking are part of the first release.
- Whether room names must be unique within the Center (proposed above).
- Repository and application-service responsibilities.
- API operations, request/response DTOs, and authorization.

No entity fields, relationships, service methods, authorization rules, or endpoint paths are finalized. Agree on them before implementation; do not treat this document as describing existing behavior.
