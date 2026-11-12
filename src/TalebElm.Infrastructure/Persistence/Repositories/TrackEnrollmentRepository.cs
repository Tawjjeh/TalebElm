using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class TrackEnrollmentRepository(AppDbContext context) : ITrackEnrollmentRepository
{
    public async Task AddAsync(TrackEnrollment entity, CancellationToken ct = default)
    {
        await context.TrackEnrollments.AddAsync(entity, ct);
    }

    public async Task<IReadOnlyList<TrackEnrollment>> GetAllAsync(CancellationToken ct = default)
    {
        return await context.TrackEnrollments.ToListAsync(ct);
    }

    public async Task<TrackEnrollment?> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        return await context.TrackEnrollments
            .FindAsync([id], ct);
    }
}
