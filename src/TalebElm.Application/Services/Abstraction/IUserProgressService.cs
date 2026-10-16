using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface IUserProgressService
{
    Task<IReadOnlyList<ProgressResponse>> GetMyProgressAsync();
}