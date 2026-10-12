
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;

namespace TalebElm.Application.Services.Concrete;

public class UserService : IUserService
{
    public Task<IReadOnlyList<UserResponse>> GetAllAsync()
        => throw new NotImplementedException();
    public Task<UserResponse> CreateAsync(CreateUserRequest request)
        => throw new NotImplementedException();


}
