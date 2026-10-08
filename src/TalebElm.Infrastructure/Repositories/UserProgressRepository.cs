using System;
using System.Security.Cryptography;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Repositories;

public class UserProgressRepository(AppDbContext context) : IUserProgressRepository
{
    public async Task AddAsync(UserProgress entity)
    {
        await context.UserProgresses.AddAsync(entity);
    }

    public async Task<IReadOnlyList<UserProgress>> GetAllAsync()
    {
        return await context.UserProgresses.ToListAsync();
    }

    public async Task<UserProgress?> GetByIdAsync(Guid id)
    {
        return await context.UserProgresses.FirstOrDefaultAsync(up => up.Id == id);
    }

    public async Task<UserProgress?> GetByUserAndModuleAsync(Guid userId, Guid moduleId)
    {
        return await context.UserProgresses.FirstOrDefaultAsync(up => up.UserId == userId && up.ModuleId == moduleId);
    }

    public async Task<IReadOnlyList<UserProgress>> GetByUserAndTrackAsync(Guid userId, Guid trackId)
    {
        var r = await (from up in context.UserProgresses
                       join m in context.Modules on up.ModuleId equals m.Id
                       where m.TrackId == trackId && up.UserId == userId
                       orderby m.Order
                       select up)
                    .ToListAsync();
        return r;
    }
}
