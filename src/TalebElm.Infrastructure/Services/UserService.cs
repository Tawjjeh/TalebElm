
using TalebElm.Application.DTOs;
using TalebElm.Application.Services;

namespace TalebElm.Infrastructure.Services;

public class UserService : IUserService
{
    public Task<IReadOnlyList<UserResponse>> GetAllAsync()
        => throw new NotImplementedException();
    public Task<UserResponse> CreateAsync(CreateUserRequest request)
        => throw new NotImplementedException();

   
}
