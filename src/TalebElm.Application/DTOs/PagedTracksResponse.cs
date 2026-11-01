namespace TalebElm.Application.DTOs;

public record PagedTracksResponse(
    IReadOnlyList<TrackResponse> Items,
    int Page,
    int PageSize,
    int TotalCount);
