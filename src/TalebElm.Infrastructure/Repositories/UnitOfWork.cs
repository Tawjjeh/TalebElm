using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure.Repositories;

public class UnitOfWork(
    AppDbContext context,
    IUserRepository userRepository,
    ITrackRepository trackRepository,
    IModuleRepository moduleRepository)
    : IUnitOfWork
{
    private readonly AppDbContext _context = context;
    
    public IUserRepository Users { get; } = userRepository;

    public ITrackRepository Tracks { get; } = trackRepository;

    public IModuleRepository Modules { get; } = moduleRepository;

    public async Task<int> SaveChangesAsync()
    {
       return await _context.SaveChangesAsync();
    }
}
