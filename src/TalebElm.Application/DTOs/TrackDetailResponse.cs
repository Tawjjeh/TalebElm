namespace TalebElm.Application.DTOs;

/// <summary>
/// Represents detailed track information, including its ordered modules.
/// </summary>
/// <param name="Id">The unique identifier of the track.</param>
/// <param name="Name">The name of the track.</param>
/// <param name="Description">The description of the track.</param>
/// <param name="Status">The current status of the track.</param>
/// <param name="Modules">The modules of the track, in track order.</param>
public record TrackDetailResponse(
    Guid Id,
    string Name,
    string Description,
    int Status,
    IReadOnlyList<ModuleResponse> Modules);
