using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);

}
