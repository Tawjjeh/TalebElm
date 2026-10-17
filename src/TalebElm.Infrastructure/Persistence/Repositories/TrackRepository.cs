using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Repositories;

public class TrackRepository(AppDbContext context) : ITrackRepository
{
    public async Task AddAsync(Track entity)
    {
        await context.Tracks.AddAsync(entity);
    }

    public async Task<IReadOnlyList<Track>> GetAllAsync()
    {
        return await context.Tracks.AsNoTracking().ToListAsync();
    }

    public async Task<Track?> GetByIdAsync(Guid id)
    {
        return await context.Tracks.FirstOrDefaultAsync(t => t.Id == id);
    }
}
