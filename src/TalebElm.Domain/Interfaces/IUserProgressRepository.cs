using System;
using System.Collections.Generic;
using System.Text;
using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces
{
    public interface IUserProgressRepository : IRepository<UserProgress>
    {
        Task<UserProgress?> GetByUserAndModuleAsync(Guid userId, Guid moduleId);
        Task<IReadOnlyList<UserProgress>> GetByUserAndTrackAsync(Guid userId, Guid trackId);
    }
}
