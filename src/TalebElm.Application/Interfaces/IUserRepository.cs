using System;
using System.Collections.Generic;
using System.Text;

namespace TalebElm.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<User>> GetAllAsync();
        Task AddAsync(User entity);
        Task UpdateAsync(User entity);
        Task RemoveAsync(Guid id);
    }
}
