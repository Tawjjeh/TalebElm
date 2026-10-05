using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Repositories;

public class TrackRepository(AppDbContext context) : ITrackRepository
{
    public async Task AddAsync(Track entity)
    {
        await context.Set<Track>().AddAsync(entity);
    }

    public Task<IReadOnlyList<Track>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<Track?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
