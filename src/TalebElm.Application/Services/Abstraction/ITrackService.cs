using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction
{
    public interface ITrackService
    {
        Task<IReadOnlyList<TrackResponse>> GetAllAsync();
        Task<TrackResponse> CreateAsync(CreateTrackRequest request);
    }
}
