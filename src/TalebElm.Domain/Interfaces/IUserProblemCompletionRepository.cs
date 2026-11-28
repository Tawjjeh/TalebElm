using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface IUserProblemCompletionRepository : IRepository<UserProblemCompletion>
{
    Task<UserProblemCompletion?> GetByUserAndProblemAsync(Guid userId, Guid moduleProblemId, CancellationToken ct = default);
    Task<int> CountForUserInModuleAsync(Guid userId, Guid moduleId, CancellationToken ct = default);
}
