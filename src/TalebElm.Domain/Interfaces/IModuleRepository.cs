using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface IModuleRepository : IRepository<Module>
{
    Task<IReadOnlyList<Module>> GetByTrackIdAsync(Guid trackId, CancellationToken ct = default);
    Task<Module?> GetNextModuleAsync(Guid trackId, int order, CancellationToken ct = default);
}
