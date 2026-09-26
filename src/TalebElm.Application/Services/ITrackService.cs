using System;
using System.Collections.Generic;
using System.Text;
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services
{
    public interface ITrackService
    {
        Task<IReadOnlyList<TrackResponse>> GetAllAsync();
        Task<TrackResponse> CreateAsync(CreateTrackRequest request);
    }
}
