using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Application.Services.Concrete;

public class UserService : IUserService
{
    public Task<IReadOnlyList<UserResponse>> GetAllAsync()
        => throw new NotImplementedException("Listing users is not implemented yet.");
    public Task<UserResponse> CreateAsync(CreateUserRequest request)
        => throw new NotImplementedException("Creating a user is not implemented yet.");
}
