
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync();
    Task<UserResponse> CreateAsync(CreateUserRequest request);
}
