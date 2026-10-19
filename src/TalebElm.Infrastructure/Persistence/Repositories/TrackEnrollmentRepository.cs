using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class TrackEnrollmentRepository(AppDbContext context) : ITrackEnrollmentRepository
{
    public async Task AddAsync(TrackEnrollment entity)
    {
        await context.TrackEnrollments.AddAsync(entity);
    }

    public async Task<IReadOnlyList<TrackEnrollment>> GetAllAsync()
    {
        return await context.TrackEnrollments.ToListAsync();
    }

    public async Task<TrackEnrollment?> GetByIdAsync(Guid id)
    {
        return await context.TrackEnrollments
            .FindAsync(id);
    }
}
