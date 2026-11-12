namespace TalebElm.Application.DTOs;

/// <summary>
/// The data needed to create a module in a track.
/// </summary>
/// <param name="Title">The module title. Required, at most 150 characters.</param>
/// <param name="Summary">A short description of the module. Required, at most 500 characters.</param>
/// <param name="Order">The position of the module within the track, starting at 1. Must be unique within the track.</param>
/// <param name="TrackId">The identifier of the track the module belongs to.</param>
public sealed record CreateModuleRequest(
    string Title,
    string Summary, 
    int Order, 
    Guid TrackId);
