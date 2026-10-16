using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Persistence.Repositories;

public class TrackEnrollmentRepository(AppDbContext context) : ITrackEnrollmentRepository
{
    public Task AddAsync(TrackEnrollment entity)
    {
        throw new NotImplementedException();
    }

    public Task<IReadOnlyList<TrackEnrollment>> GetAllAsync()
    {
        throw new NotImplementedException();
    }

    public Task<TrackEnrollment?> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }
}
