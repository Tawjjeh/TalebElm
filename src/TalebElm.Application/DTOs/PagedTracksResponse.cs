namespace TalebElm.Application.DTOs;

/// <summary>
/// Represents a paginated collection of tracks.
/// </summary>
/// <param name="Items">The tracks included in the current page.</param>
/// <param name="Page">The current page number.</param>
/// <param name="PageSize">The maximum number of tracks per page.</param>
/// <param name="TotalCount">The total number of matching tracks across all pages.</param>
public record PagedTracksResponse(
    IReadOnlyList<TrackResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
