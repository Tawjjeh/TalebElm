
namespace TalebElm.Application.DTOs;

/// <summary>
/// A module within a track, as returned by the API.
/// </summary>
/// <param name="Id">The unique identifier of the module.</param>
/// <param name="Title">The module title.</param>
/// <param name="Summary">A short description of what the module covers.</param>
/// <param name="Order">The position of the module within its track, starting at 1.</param>
/// <param name="TrackId">The identifier of the track that contains the module.</param>
public record ModuleResponse(
    Guid Id,
    string Title,
    string Summary,
    int Order,
    Guid TrackId
);
