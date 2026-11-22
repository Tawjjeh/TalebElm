
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface IUserService
{
    Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken ct = default);
    Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct = default);
}
