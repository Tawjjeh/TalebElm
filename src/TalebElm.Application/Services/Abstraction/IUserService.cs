
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync();
    Task<UserResponse> CreateAsync(CreateUserRequest request);
}
