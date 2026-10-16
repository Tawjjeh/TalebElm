namespace TalebElm.Application.DTOs;

public sealed record CreateModuleRequest(
    string Title,
    string Summary, 
    int Order, 
    Guid TrackId);
