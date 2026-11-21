namespace TalebElm.Application.DTOs;

public record TechnologyDetailResponse(
    Guid Id,
    Guid? ParentId,
    string Name,
    string Slug,
    string? Description,
    int Kind,
    int Order,
    IReadOnlyList<TechnologySummaryResponse> Children,
    PagedResult<ResourceResponse> Resources);
