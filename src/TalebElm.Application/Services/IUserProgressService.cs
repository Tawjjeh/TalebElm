using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services;

public interface IUserProgressService
{
    Task<IReadOnlyList<ProgressResponse>> GetMyProgressAsync();
}