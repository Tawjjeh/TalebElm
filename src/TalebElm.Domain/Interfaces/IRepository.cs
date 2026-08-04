namespace TalebElm.Domain.Interfaces;

using TalebElm.Domain.Entities;

public interface IRepository<T> where T : BaseEntity
{
    Task<T?> GetByIdAsync(Guid id);
    Task<IReadOnlyList<T>> GetAllAsync();
    Task AddAsync(T entity);
}
