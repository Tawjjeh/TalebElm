using System;
using System.Collections.Generic;
using System.Text;
using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<User>> GetAllAsync();
        Task AddAsync(User entity);
        Task UpdateAsync(User entity);
        Task DeleteAsync(User entity);
    }
}
