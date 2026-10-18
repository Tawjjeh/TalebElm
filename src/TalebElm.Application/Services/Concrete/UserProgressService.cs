using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Application.Services.Concrete;

public class UserProgressService : IUserProgressService
{
    public Task<IReadOnlyList<ProgressResponse>> GetMyProgressAsync()
        => throw new NotImplementedException("Reading learner progress is not implemented yet.");
}
