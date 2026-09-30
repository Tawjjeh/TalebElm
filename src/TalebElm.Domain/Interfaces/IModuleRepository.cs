using System;
using System.Collections.Generic;
using System.Text;
using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces
{
    public interface IModuleRepository : IRepository<Module>
    {
        Task<IReadOnlyList<Module>> GetByTrackIdAsync(Guid trackId);
    }
}
