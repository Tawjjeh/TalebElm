using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface ITrackEnrollmentService
{
    Task<EnrollmentResponse> EnrollAsync(Guid userId, Guid trackId);
}
