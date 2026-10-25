using Moq;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Tests.UnitTests;

public class UserProgressServiceTests
{
    [Fact]
    public async Task GetMyProgressAsync_FiltersByUser()
    {
        var userId = Guid.NewGuid();
        var otherId = Guid.NewGuid();
        var rows = new List<UserProgress>
        {
            new() { UserId = userId, ModuleId = Guid.NewGuid(), IsUnlocked = true, PassedExam = false, Score = 0 },
            new() { UserId = otherId, ModuleId = Guid.NewGuid(), IsUnlocked = true, PassedExam = true, Score = 10 }
        };
        var repository = new Mock<IUserProgressRepository>();
        repository.Setup(r => r.GetAllAsync()).ReturnsAsync(rows);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.UserProgresses).Returns(repository.Object);

        var service = new UserProgressService(unitOfWork.Object);

        var result = await service.GetMyProgressAsync(userId);

        Assert.Single(result);
        Assert.Equal(rows[0].ModuleId, result[0].ModuleId);
    }

    [Fact]
    public async Task GetByTrackAsync_ReturnsModuleProgressInOrder()
    {
        var userId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var rows = new List<UserProgress>
        {
            new() { UserId = userId, ModuleId = Guid.NewGuid(), IsUnlocked = true, PassedExam = true, Score = 70 }
        };
        var repository = new Mock<IUserProgressRepository>();
        repository.Setup(r => r.GetByUserAndTrackAsync(userId, trackId)).ReturnsAsync(rows);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.UserProgresses).Returns(repository.Object);

        var service = new UserProgressService(unitOfWork.Object);

        var result = await service.GetByTrackAsync(userId, trackId);

        Assert.Single(result);
        Assert.True(result[0].PassedExam);
    }
}
