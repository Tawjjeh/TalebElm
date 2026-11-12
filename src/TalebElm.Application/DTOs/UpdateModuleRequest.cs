namespace TalebElm.Application.DTOs;

/// <summary>
/// The data used to update an existing module.
/// </summary>
/// <param name="Title">The new module title. Required, at most 150 characters.</param>
/// <param name="Summary">The new short description of the module, at most 500 characters. Optional.</param>
/// <param name="Order">The new position of the module within the track, starting at 1. Must be unique within the track.</param>
/// <param name="TrackId">The identifier of the track the module belongs to.</param>
public record UpdateModuleRequest(
    string Title,
    string? Summary,
    int Order,
    Guid TrackId);
