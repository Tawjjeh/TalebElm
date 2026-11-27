
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface IModuleService
{
    Task<IReadOnlyList<ModuleResponse>> GetByTrackAsync(Guid trackId, CancellationToken ct = default);

    Task<ModuleResponse> GetByIdAsync(Guid id,CancellationToken ct = default);

    /// <summary>
    /// Creates a module in a track at <c>request.Order</c>, shifting siblings
    /// with <c>SiblingOrdering.PrepareInsert</c> so <c>Order</c> stays
    /// contiguous. The route's track id is authoritative over
    /// <c>request.TrackId</c>.
    /// </summary>
    Task<ModuleResponse> CreateAsync(CreateModuleRequest request, CancellationToken ct = default);

    /// <summary>
    /// Updates a module's title, summary, and order within its track (an
    /// <c>Order</c> change moves the module via <c>SiblingOrdering.MoveAsync</c>).
    /// Moving a module to another track is not supported.
    /// </summary>
    Task<ModuleResponse> UpdateAsync(Guid id, UpdateModuleRequest request, CancellationToken ct = default);

    /// <summary>
    /// Deletes a module and compacts the remaining siblings. Throws a conflict
    /// when the module still has Lessons, an Exam, or learner progress.
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}
