using Moq;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Tests.UnitTests;

public class UserServiceTests
{
    [Fact]
    public async Task GetAllAsync_WhenUsersExist_ReturnsUserResponses()
    {
        var users = new List<User>
        {
            new() { Id = Guid.NewGuid(), Name = "Ada", Email = "ada@test.com" },
            new() { Id = Guid.NewGuid(), Name = "Bob", Email = "bob@test.com" }
        };

        var unitOfWork = new Mock<IUnitOfWork>();
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(repository => repository.GetAllAsync()).ReturnsAsync(users);
        unitOfWork.Setup(unit => unit.Users).Returns(userRepository.Object);

        var service = new UserService(unitOfWork.Object);

        var result = await service.GetAllAsync();

        Assert.Equal(2, result.Count);
        Assert.Equal("Ada", result[0].Name);
        Assert.Equal("bob@test.com", result[1].Email);
    }

    [Fact]
    public async Task CreateAsync_StoresUserAndReturnsResponse()
    {
        var request = new CreateUserRequest("Charlie", "charlie@test.com");

        var unitOfWork = new Mock<IUnitOfWork>();
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(repository => repository.AddAsync(It.IsAny<User>()))
            .Returns(Task.CompletedTask);
        unitOfWork.Setup(unit => unit.Users).Returns(userRepository.Object);
        unitOfWork.Setup(unit => unit.SaveChangesAsync()).ReturnsAsync(1);

        var service = new UserService(unitOfWork.Object);

        var result = await service.CreateAsync(request);

        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Email, result.Email);
        userRepository.Verify(repository => repository.AddAsync(It.IsAny<User>()), Times.Once);
        unitOfWork.Verify(unit => unit.SaveChangesAsync(), Times.Once);
    }
    [Fact]
    public async Task GetAllAsync_ForwardsCancellationTokenToRepository()
    {
        using var cts = new CancellationTokenSource();
        var unitOfWork = new Mock<IUnitOfWork>();
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.GetAllAsync(cts.Token)).ReturnsAsync([]);
        unitOfWork.Setup(u => u.Users).Returns(userRepository.Object);

        var service = new UserService(unitOfWork.Object);

        await service.GetAllAsync(cts.Token);

        userRepository.Verify(r => r.GetAllAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ForwardsCancellationTokenToRepositoryAndUnitOfWork()
    {
        using var cts = new CancellationTokenSource();
        var request = new CreateUserRequest("Charlie", "charlie@test.com");

        var unitOfWork = new Mock<IUnitOfWork>();
        var userRepository = new Mock<IUserRepository>();
        userRepository.Setup(r => r.AddAsync(It.IsAny<User>(), cts.Token)).Returns(Task.CompletedTask);
        unitOfWork.Setup(u => u.Users).Returns(userRepository.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync(cts.Token)).ReturnsAsync(1);

        var service = new UserService(unitOfWork.Object);

        await service.CreateAsync(request, cts.Token);

        userRepository.Verify(r => r.AddAsync(It.IsAny<User>(), cts.Token), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }
}
