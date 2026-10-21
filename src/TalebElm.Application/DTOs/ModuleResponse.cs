
namespace TalebElm.Application.DTOs;

public record ModuleResponse(
    Guid Id,
    string Title,
    string Summary,
    int Order,
    Guid TrackId
);
