using TalebElm.Application.DTOs.External;

namespace TalebElm.Application.Services.Abstraction;

public interface ICodeforcesClient
{
    Task<CodeforcesUser?> GetUserProfileAsync(string handle, CancellationToken cancellationToken = default);

    Task<IReadOnlyList<CodeforcesSubmission>> GetUserSubmissionsAsync(string handle, CancellationToken cancellationToken = default);
}
