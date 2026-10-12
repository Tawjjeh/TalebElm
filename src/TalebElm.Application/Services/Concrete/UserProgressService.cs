using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;

namespace TalebElm.Application.Services.Concrete;

public class UserProgressService : IUserProgressService
{
    public Task<IReadOnlyList<ProgressResponse>> GetMyProgressAsync()
        => throw new NotImplementedException();

}
