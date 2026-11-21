namespace TalebElm.Application.DTOs;

public record ResourceResponse(
    Guid Id,
    Guid? TechnologyId,
    string Title,
    int Kind,
    string? Url,
    string? Citation,
    string Language,
    int Level,
    bool IsPaid,
    int Status);
