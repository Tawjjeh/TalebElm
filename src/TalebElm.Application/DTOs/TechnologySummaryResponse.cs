namespace TalebElm.Application.DTOs;

public record TechnologySummaryResponse(
    Guid Id,
    Guid? ParentId,
    string Name,
    string Slug,
    int Kind,
    int Order,
    int ChildCount,
    int ResourceCount);
