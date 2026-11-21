using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface IModuleProblemRepository : IRepository<ModuleProblem>
{
    Task<IReadOnlyList<ModuleProblem>> GetByModuleAsync(Guid moduleId, CancellationToken ct = default);
    Task<int> CountRequiredAsync(Guid moduleId, CancellationToken ct = default);
}
