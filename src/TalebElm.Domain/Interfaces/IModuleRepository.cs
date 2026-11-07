using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface IModuleRepository : IRepository<Module>
{
    Task<Module?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<Module>> GetByTrackIdAsync(Guid trackId, CancellationToken ct = default);
}
