using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Repositories;

public class UserProgressRepository : IUserProgressRepository
{
    public Task<UserProgress?> GetByIdAsync(Guid id) => throw new NotImplementedException();

    public Task<IReadOnlyList<UserProgress>> GetAllAsync() => throw new NotImplementedException();

    public Task AddAsync(UserProgress entity) => throw new NotImplementedException();
}
