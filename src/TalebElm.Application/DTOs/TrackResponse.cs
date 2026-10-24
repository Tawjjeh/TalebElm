namespace TalebElm.Application.DTOs
{
    /// <summary>
    /// Represents the response data for a track.
    /// </summary>
    /// <param name="Id">The unique identifier of the track.</param>
    /// <param name="Name">The name of the track.</param>
    /// <param name="Description">The description of the track.</param>
    /// <param name="Status">The current status of the track.</param>
    public record TrackResponse(Guid Id, string Name, string Description, int Status);
}