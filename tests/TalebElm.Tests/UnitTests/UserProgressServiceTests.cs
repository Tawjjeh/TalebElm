using Moq;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
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
        repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>())).ReturnsAsync(rows);
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
        repository.Setup(r => r.GetByUserAndTrackAsync(userId, trackId, It.IsAny<CancellationToken>())).ReturnsAsync(rows);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.UserProgresses).Returns(repository.Object);

        var service = new UserProgressService(unitOfWork.Object);

        var result = await service.GetByTrackAsync(userId, trackId);

        Assert.Single(result);
        Assert.True(result[0].PassedExam);
    }

    [Fact]
    public async Task GetMyProgressAsync_ForwardsCancellationTokenToRepository()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var repository = new Mock<IUserProgressRepository>();
        repository.Setup(r => r.GetAllAsync(It.IsAny<CancellationToken>()))
            .Returns((CancellationToken ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return Task.FromResult<IReadOnlyList<UserProgress>>(new List<UserProgress>());
            });
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.UserProgresses).Returns(repository.Object);

        var service = new UserProgressService(unitOfWork.Object);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.GetMyProgressAsync(Guid.NewGuid(), cts.Token));
    }

    [Fact]
    public async Task GetByTrackAsync_ForwardsCancellationTokenToRepository()
    {
        using var cts = new CancellationTokenSource();
        cts.Cancel();
        var repository = new Mock<IUserProgressRepository>();
        repository.Setup(r => r.GetByUserAndTrackAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .Returns((Guid _, Guid _, CancellationToken ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return Task.FromResult<IReadOnlyList<UserProgress>>(new List<UserProgress>());
            });
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.UserProgresses).Returns(repository.Object);

        var service = new UserProgressService(unitOfWork.Object);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(
            () => service.GetByTrackAsync(Guid.NewGuid(), Guid.NewGuid(), cts.Token));
    }

    #region AdminUnlockAsync_Method_Tests

    private readonly Guid _adminId = Guid.NewGuid();
    private readonly Guid _learnerId = Guid.NewGuid();
    private readonly Guid _moduleId = Guid.NewGuid();

    private (Mock<IUnitOfWork> UnitOfWork, Mock<IUserRepository> Users, Mock<IModuleRepository> Modules, Mock<IUserProgressRepository> Progresses) ArrangeUnlock(
        User? learner, Module? module, UserProgress? progress)
    {
        var users = new Mock<IUserRepository>();
        users.Setup(r => r.GetByIdAsync(_learnerId, It.IsAny<CancellationToken>())).ReturnsAsync(learner);
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(_moduleId, It.IsAny<CancellationToken>())).ReturnsAsync(module);
        var progresses = new Mock<IUserProgressRepository>();
        progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _moduleId, It.IsAny<CancellationToken>())).ReturnsAsync(progress);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Users).Returns(users.Object);
        unitOfWork.Setup(u => u.Modules).Returns(modules.Object);
        unitOfWork.Setup(u => u.UserProgresses).Returns(progresses.Object);
        return (unitOfWork, users, modules, progresses);
    }

    private static User StudentLearner(Guid id) => new()
    {
        Id = id, Name = "Learner", Email = "learner@test.com", PasswordHash = "hash", Role = UserRole.Student
    };

    [Fact]
    public async Task AdminUnlockAsync_WhenProgressMissing_CreatesUnlockedRow()
    {
        var (unitOfWork, _, _, progresses) = ArrangeUnlock(
            StudentLearner(_learnerId), new Module { Id = _moduleId }, progress: null);
        var service = new UserProgressService(unitOfWork.Object);

        var result = await service.AdminUnlockAsync(_adminId, new AdminUnlockModuleRequest(_learnerId, _moduleId));

        Assert.Equal(_moduleId, result.ModuleId);
        Assert.True(result.IsUnlocked);
        Assert.False(result.PassedExam);
        Assert.Equal(0, result.Score);
        progresses.Verify(r => r.AddAsync(
            It.Is<UserProgress>(p => p.UserId == _learnerId && p.ModuleId == _moduleId
                && p.IsUnlocked && !p.PassedExam && p.Score == 0 && !p.CompletedRequiredProblems),
            It.IsAny<CancellationToken>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdminUnlockAsync_WhenLocked_UnlocksLeavingScoreAndFlagsUntouched()
    {
        var existing = new UserProgress
        {
            UserId = _learnerId, ModuleId = _moduleId,
            IsUnlocked = false, PassedExam = true, Score = 85, CompletedRequiredProblems = true
        };
        var (unitOfWork, _, _, progresses) = ArrangeUnlock(
            StudentLearner(_learnerId), new Module { Id = _moduleId }, existing);
        var service = new UserProgressService(unitOfWork.Object);

        var result = await service.AdminUnlockAsync(_adminId, new AdminUnlockModuleRequest(_learnerId, _moduleId));

        Assert.True(existing.IsUnlocked);
        Assert.Equal(85, existing.Score);
        Assert.True(existing.PassedExam);
        Assert.True(existing.CompletedRequiredProblems);
        Assert.Equal(85, result.Score);
        progresses.Verify(r => r.AddAsync(It.IsAny<UserProgress>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AdminUnlockAsync_WhenAlreadyUnlocked_IsIdempotentNoOp()
    {
        var existing = new UserProgress
        {
            UserId = _learnerId, ModuleId = _moduleId,
            IsUnlocked = true, PassedExam = false, Score = 40, CompletedRequiredProblems = false
        };
        var (unitOfWork, _, _, progresses) = ArrangeUnlock(
            StudentLearner(_learnerId), new Module { Id = _moduleId }, existing);
        var service = new UserProgressService(unitOfWork.Object);

        var result = await service.AdminUnlockAsync(_adminId, new AdminUnlockModuleRequest(_learnerId, _moduleId));

        Assert.True(result.IsUnlocked);
        Assert.Equal(40, result.Score);
        Assert.False(result.PassedExam);
        progresses.Verify(r => r.AddAsync(It.IsAny<UserProgress>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdminUnlockAsync_WhenLearnerMissing_ThrowsNotFoundException()
    {
        var (unitOfWork, _, _, _) = ArrangeUnlock(
            learner: null, new Module { Id = _moduleId }, progress: null);
        var service = new UserProgressService(unitOfWork.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.AdminUnlockAsync(_adminId, new AdminUnlockModuleRequest(_learnerId, _moduleId)));
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdminUnlockAsync_WhenLearnerIsNotStudent_ThrowsNotFoundException()
    {
        var admin = new User
        {
            Id = _learnerId, Name = "Admin", Email = "admin@test.com", PasswordHash = "hash", Role = UserRole.Admin
        };
        var (unitOfWork, _, _, _) = ArrangeUnlock(
            admin, new Module { Id = _moduleId }, progress: null);
        var service = new UserProgressService(unitOfWork.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.AdminUnlockAsync(_adminId, new AdminUnlockModuleRequest(_learnerId, _moduleId)));
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdminUnlockAsync_WhenModuleMissing_ThrowsNotFoundException()
    {
        var (unitOfWork, _, _, _) = ArrangeUnlock(
            StudentLearner(_learnerId), module: null, progress: null);
        var service = new UserProgressService(unitOfWork.Object);

        await Assert.ThrowsAsync<NotFoundException>(
            () => service.AdminUnlockAsync(_adminId, new AdminUnlockModuleRequest(_learnerId, _moduleId)));
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task AdminUnlockAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var (unitOfWork, users, modules, progresses) = ArrangeUnlock(
            StudentLearner(_learnerId), new Module { Id = _moduleId }, progress: null);
        users.Setup(r => r.GetByIdAsync(_learnerId, cts.Token)).ReturnsAsync(StudentLearner(_learnerId));
        modules.Setup(r => r.GetByIdAsync(_moduleId, cts.Token)).ReturnsAsync(new Module { Id = _moduleId });
        progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _moduleId, cts.Token)).ReturnsAsync((UserProgress?)null);
        var service = new UserProgressService(unitOfWork.Object);

        var result = await service.AdminUnlockAsync(_adminId, new AdminUnlockModuleRequest(_learnerId, _moduleId), cts.Token);

        Assert.True(result.IsUnlocked);
        users.Verify(r => r.GetByIdAsync(_learnerId, cts.Token), Times.Once);
        modules.Verify(r => r.GetByIdAsync(_moduleId, cts.Token), Times.Once);
        progresses.Verify(r => r.GetByUserAndModuleAsync(_learnerId, _moduleId, cts.Token), Times.Once);
    }
    #endregion
}
