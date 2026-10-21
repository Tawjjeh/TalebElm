using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public class ModuleService(IUnitOfWork unitOfWork) : IModuleService
{
    public async Task<IReadOnlyList<ModuleResponse>> GetByTrackAsync(Guid trackId)
    {
        if (await unitOfWork.Tracks.GetByIdAsync(trackId) is null)
        {
            throw new NotFoundException($"Track with id '{trackId}' was not found.");
        }

        var modules = await unitOfWork.Modules.GetByTrackIdAsync(trackId);

        return modules
            .OrderBy(module => module.Order)
            .Select(module => new ModuleResponse(
                module.Id,
                module.Title,
                module.Summary,
                module.Order,
                module.TrackId))
            .ToList();
    }

    public async Task<ModuleResponse> GetByIdAsync(Guid id)
    {
        var module = await unitOfWork.Modules.GetByIdAsync(id);

        if (module is null)
        {
            throw new NotFoundException($"Module with id '{id}' was not found.");
        }

        return new ModuleResponse(
            module.Id,
            module.Title,
            module.Summary,
            module.Order,
            module.TrackId);
    }
}
