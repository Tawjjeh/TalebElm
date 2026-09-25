using System;
using System.Collections.Generic;
using System.Text;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Repositories
{
    public class TrackRepository : ITrackRepository
    {
        public Task AddAsync(Track entity)
        {
            throw new NotImplementedException();
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
}
