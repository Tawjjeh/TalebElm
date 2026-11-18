using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class UserExternalAccountRepository(AppDbContext context) : IUserExternalAccountRepository
{
    public async Task<UserExternalAccount?> GetByIdAsync(Guid id, CancellationToken ct = default)
        => await context.UserExternalAccounts.FirstOrDefaultAsync(u => u.Id == id, ct);

    public async Task<IReadOnlyList<UserExternalAccount>> GetAllAsync(CancellationToken ct = default)
        => await context.UserExternalAccounts.AsNoTracking().ToListAsync(ct);

    public async Task AddAsync(UserExternalAccount entity, CancellationToken ct = default)
        => await context.UserExternalAccounts.AddAsync(entity, ct);

    public async Task<UserExternalAccount?> GetByUserAndPlatformAsync(Guid userId, ExternalPlatform platform, CancellationToken ct = default)
    {
        return await context.UserExternalAccounts
            .FirstOrDefaultAsync(u =>u.UserId == userId && u.Platform == platform, ct);
    }

    public async Task<UserExternalAccount?> GetByPlatformAndHandleAsync(ExternalPlatform platform, string handle, CancellationToken ct = default)
    {
        return await context.UserExternalAccounts
            .FirstOrDefaultAsync(a => a.Platform == platform && a.Handle == handle, ct);
    }
}
