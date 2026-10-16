using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public Task<User?> GetByIdAsync(Guid id)
        => throw new NotImplementedException();

    public Task<IReadOnlyList<User>> GetAllAsync()
        => throw new NotImplementedException();

    public Task AddAsync(User entity)
        => throw new NotImplementedException();
}
