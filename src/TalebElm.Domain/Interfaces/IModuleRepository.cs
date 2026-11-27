using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface IModuleRepository : IRepository<Module>
{
    Task<IReadOnlyList<Module>> GetByTrackIdAsync(Guid trackId, CancellationToken ct = default);

    /// <summary>
    /// Loads a Track's Modules ordered by <c>Order</c>, tracked, for ordering
    /// maintenance (<c>SiblingOrdering</c> in Application): the caller
    /// mutates the returned entities and saves once. The read
    /// <see cref="GetByTrackIdAsync"/> intentionally returns detached copies.
    /// </summary>
    Task<IReadOnlyList<Module>> GetByTrackIdForUpdateAsync(Guid trackId, CancellationToken ct = default);

    /// <summary>
    /// Marks a module for deletion. Callers run the #251 conflict guards
    /// first, so no dependent row may exist when this is called.
    /// </summary>
    Task RemoveAsync(Module module, CancellationToken ct = default);

    Task<Module?> GetNextModuleAsync(Guid trackId, int order, CancellationToken ct = default);
}
