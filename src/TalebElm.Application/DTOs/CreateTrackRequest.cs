namespace TalebElm.Application.DTOs
{
    /// <summary>
    /// Represents the data required to create a new track.
    /// </summary>
    /// <param name="Name">The name of the track.</param>
    /// <param name="Description">The description of the track.</param>
    public record CreateTrackRequest(string Name, string Description);
}