using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public class UserService : IUserService
{
    private readonly IUnitOfWork _unitOfWork;

    public UserService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<UserResponse>> GetAllAsync(CancellationToken ct = default)
    {
        var users = await _unitOfWork.Users.GetAllAsync(ct);
        return users
            .Select(user => new UserResponse(user.Id, user.Name, user.Email))
            .ToList();
    }

    public async Task<UserResponse> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
    {
        var user = new User
        {
            Name = request.Name,
            Email = request.Email,
            JoinedAt = DateTime.UtcNow
        };

        await _unitOfWork.Users.AddAsync(user,ct);
        await _unitOfWork.SaveChangesAsync(ct);

        return new UserResponse(user.Id, user.Name, user.Email);
    }
}
