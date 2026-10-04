using TalebElm.Application.DTOs;
using TalebElm.Application.Services;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Services;

public class TrackService(IUnitOfWork unitOfWork) : ITrackService
{
    public async Task<IReadOnlyList<TrackResponse>> GetAllAsync()
    {
        var tracks = await unitOfWork.Tracks.GetAllAsync();

        if (tracks is null || tracks.Count == 0)
            return Array.Empty<TrackResponse>();

        return tracks
            .Select(track => new TrackResponse(
                track.Id,
                track.Name,
                track.Description,
                (int)track.Status))
            .ToList();
    }

    public Task<TrackResponse> CreateAsync(CreateTrackRequest request)
    {
        throw new NotImplementedException();
    }
}