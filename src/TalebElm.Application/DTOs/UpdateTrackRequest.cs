namespace TalebElm.Application.DTOs;

/// <summary>
/// The data used to update an existing track.
/// </summary>
/// <param name="Name">The new track name. Required, at most 150 characters.</param>
/// <param name="Description">The new track description. Required, at most 1000 characters.</param>
public record UpdateTrackRequest(string Name, string Description);
