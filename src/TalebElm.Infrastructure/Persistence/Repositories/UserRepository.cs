using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;


namespace TalebElm.Infrastructure.Persistence.Repositories;

public class UserRepository(AppDbContext context) : IUserRepository
{
    public async Task<User?> GetByIdAsync(Guid id)
        => await context.Users.FirstOrDefaultAsync(u => u.Id == id);

    public async Task<IReadOnlyList<User>> GetAllAsync()
        => await context.Users.ToListAsync();

    public async Task AddAsync(User entity)
        => await context.Users.AddAsync(entity);
}
