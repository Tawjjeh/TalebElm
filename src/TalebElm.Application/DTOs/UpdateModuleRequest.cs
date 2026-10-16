namespace TalebElm.Application.DTOs;

public record UpdateModuleRequest(
    string Title,
    string? Summary,
    int Order,
    Guid TrackId);