using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces
{
    public interface IUserProgressRepository : IRepository<UserProgress>
    {
        Task<UserProgress?> GetByUserAndModuleAsync(Guid userId, Guid moduleId, CancellationToken ct = default);
        Task<IReadOnlyList<UserProgress>> GetByUserAndTrackAsync(Guid userId, Guid trackId, CancellationToken ct = default);

        /// <summary>Whether any learner progress row references the module.</summary>
        Task<bool> ExistsForModuleAsync(Guid moduleId, CancellationToken ct = default);

        /// <summary>Whether any learner progress row references a module of the track.</summary>
        Task<bool> ExistsForTrackAsync(Guid trackId, CancellationToken ct = default);
    }
}
