using TalebElm.Application.DTOs;
using TalebElm.Application.Services;

namespace TalebElm.Infrastructure.Services;

public class UserProgressService : IUserProgressService
{
    public Task<IReadOnlyList<ProgressResponse>> GetMyProgressAsync()
        => throw new NotImplementedException();

}
