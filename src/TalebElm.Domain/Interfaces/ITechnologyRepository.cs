using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface ITechnologyRepository : IRepository<Technology>
{
    Task<IReadOnlyList<Technology>> GetByParentIdAsync(Guid? parentId, CancellationToken ct = default);

    Task<Technology?> GetBySlugPathAsync(IReadOnlyList<string> slugs, CancellationToken ct = default);
}
