
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface IModuleService
{
    Task<IReadOnlyList<ModuleResponse>> GetByTrackAsync(Guid trackId);

    Task<ModuleResponse> GetByIdAsync(Guid id);
}
