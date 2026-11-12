using Moq;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Tests.UnitTests;

public class TrackEnrollmentServiceTests
{
    private static Track Track(Guid trackId) => new() { Id = trackId, Name = "T", Description = "d" };

    [Fact]
    public async Task EnrollAsync_FirstModuleOnly()
    {
        var userId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var module = new Module { Id = Guid.NewGuid(), TrackId = trackId, Title = "M1", Order = 1 };

        var unitOfWork = new Mock<IUnitOfWork>();
        var users = new Mock<IUserRepository>();
        var tracks = new Mock<ITrackRepository>();
        var enrollments = new Mock<ITrackEnrollmentRepository>();
        var userProgress = new Mock<IUserProgressRepository>();
        var modules = new Mock<IModuleRepository>();

        users.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId, Name = "u", Email = "u@t.com", PasswordHash = "hash" });
        tracks.Setup(r => r.GetByIdAsync(trackId)).ReturnsAsync(Track(trackId));
        enrollments.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<TrackEnrollment>());
        modules.Setup(r => r.GetByTrackIdAsync(trackId)).ReturnsAsync(new[] { module });

        unitOfWork.Setup(u => u.Users).Returns(users.Object);
        unitOfWork.Setup(u => u.Tracks).Returns(tracks.Object);
        unitOfWork.Setup(u => u.TrackEnrollments).Returns(enrollments.Object);
        unitOfWork.Setup(u => u.Modules).Returns(modules.Object);
        unitOfWork.Setup(u => u.UserProgresses).Returns(userProgress.Object);
        unitOfWork.Setup(u => u.SaveChangesAsync()).ReturnsAsync(1);

        var service = new TrackEnrollmentService(unitOfWork.Object);

        var result = await service.EnrollAsync(userId, trackId);

        Assert.Equal(trackId, result.TrackId);
        userProgress.Verify(r => r.AddAsync(It.IsAny<UserProgress>()), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task EnrollAsync_Duplicate_ThrowsConflict()
    {
        var userId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var unitOfWork = new Mock<IUnitOfWork>();
        var users = new Mock<IUserRepository>();
        var tracks = new Mock<ITrackRepository>();
        var enrollments = new Mock<ITrackEnrollmentRepository>();

        users.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId, Name = "u", Email = "u@t.com", PasswordHash = "hash" });
        tracks.Setup(r => r.GetByIdAsync(trackId)).ReturnsAsync(Track(trackId));
        enrollments.Setup(r => r.GetAllAsync()).ReturnsAsync(new[] { new TrackEnrollment { UserId = userId, TrackId = trackId } });

        unitOfWork.Setup(u => u.Users).Returns(users.Object);
        unitOfWork.Setup(u => u.Tracks).Returns(tracks.Object);
        unitOfWork.Setup(u => u.TrackEnrollments).Returns(enrollments.Object);

        var service = new TrackEnrollmentService(unitOfWork.Object);

        await Assert.ThrowsAsync<ConflictException>(() => service.EnrollAsync(userId, trackId));
    }

    [Fact]
    public async Task EnrollAsync_EmptyTrack_ThrowsValidation()
    {
        var userId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var unitOfWork = new Mock<IUnitOfWork>();
        var users = new Mock<IUserRepository>();
        var tracks = new Mock<ITrackRepository>();
        var enrollments = new Mock<ITrackEnrollmentRepository>();
        var modules = new Mock<IModuleRepository>();

        users.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(new User { Id = userId, Name = "u", Email = "u@t.com", PasswordHash = "hash" });
        tracks.Setup(r => r.GetByIdAsync(trackId)).ReturnsAsync(Track(trackId));
        enrollments.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<TrackEnrollment>());
        modules.Setup(r => r.GetByTrackIdAsync(trackId)).ReturnsAsync(Array.Empty<Module>());

        unitOfWork.Setup(u => u.Users).Returns(users.Object);
        unitOfWork.Setup(u => u.Tracks).Returns(tracks.Object);
        unitOfWork.Setup(u => u.TrackEnrollments).Returns(enrollments.Object);
        unitOfWork.Setup(u => u.Modules).Returns(modules.Object);

        var service = new TrackEnrollmentService(unitOfWork.Object);

        await Assert.ThrowsAsync<ValidationException>(() => service.EnrollAsync(userId, trackId));
    }

    [Fact]
    public async Task EnrollAsync_MissingUserOrTrack_ThrowsNotFound()
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        var users = new Mock<IUserRepository>();
        var tracks = new Mock<ITrackRepository>();

        users.Setup(r => r.GetByIdAsync(Guid.NewGuid())).ReturnsAsync((User?)null);
        tracks.Setup(r => r.GetByIdAsync(Guid.NewGuid())).ReturnsAsync((Track?)null);

        unitOfWork.Setup(u => u.Users).Returns(users.Object);
        unitOfWork.Setup(u => u.Tracks).Returns(tracks.Object);

        var service = new TrackEnrollmentService(unitOfWork.Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.EnrollAsync(Guid.NewGuid(), Guid.NewGuid()));
    }

    [Fact]
    public async Task EnrollAsync_CancelledToken_StopsBeforeWrites()
    {
        var userId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        using var cts = new CancellationTokenSource();
        cts.Cancel();

        var unitOfWork = new Mock<IUnitOfWork>();
        var users = new Mock<IUserRepository>();
        var enrollments = new Mock<ITrackEnrollmentRepository>();
        var userProgress = new Mock<IUserProgressRepository>();

        // The user read observes the token and throws when it is already cancelled.
        users.Setup(r => r.GetByIdAsync(userId, It.IsAny<CancellationToken>()))
            .Returns((Guid _, CancellationToken ct) =>
            {
                ct.ThrowIfCancellationRequested();
                return Task.FromResult<User?>(new User { Id = userId, Name = "u", Email = "u@t.com", PasswordHash = "hash" });
            });

        unitOfWork.Setup(u => u.Users).Returns(users.Object);
        unitOfWork.Setup(u => u.TrackEnrollments).Returns(enrollments.Object);
        unitOfWork.Setup(u => u.UserProgresses).Returns(userProgress.Object);

        var service = new TrackEnrollmentService(unitOfWork.Object);

        await Assert.ThrowsAnyAsync<OperationCanceledException>(() => service.EnrollAsync(userId, trackId, cts.Token));

        enrollments.Verify(r => r.AddAsync(It.IsAny<TrackEnrollment>(), It.IsAny<CancellationToken>()), Times.Never);
        userProgress.Verify(r => r.AddAsync(It.IsAny<UserProgress>(), It.IsAny<CancellationToken>()), Times.Never);
        unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task EnrollAsync_ForwardsTokenToEveryAsyncCall()
    {
        var userId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var module = new Module { Id = Guid.NewGuid(), TrackId = trackId, Title = "M1", Order = 1 };
        using var cts = new CancellationTokenSource();
        var ct = cts.Token;

        var unitOfWork = new Mock<IUnitOfWork>();
        var users = new Mock<IUserRepository>();
        var tracks = new Mock<ITrackRepository>();
        var enrollments = new Mock<ITrackEnrollmentRepository>();
        var userProgress = new Mock<IUserProgressRepository>();
        var modules = new Mock<IModuleRepository>();

        users.Setup(r => r.GetByIdAsync(userId, ct)).ReturnsAsync(new User { Id = userId, Name = "u", Email = "u@t.com", PasswordHash = "hash" });
        tracks.Setup(r => r.GetByIdAsync(trackId, ct)).ReturnsAsync(Track(trackId));
        enrollments.Setup(r => r.GetAllAsync(ct)).ReturnsAsync(Array.Empty<TrackEnrollment>());
        modules.Setup(r => r.GetByTrackIdAsync(trackId, ct)).ReturnsAsync(new[] { module });
        unitOfWork.Setup(u => u.SaveChangesAsync(ct)).ReturnsAsync(1);

        unitOfWork.Setup(u => u.Users).Returns(users.Object);
        unitOfWork.Setup(u => u.Tracks).Returns(tracks.Object);
        unitOfWork.Setup(u => u.TrackEnrollments).Returns(enrollments.Object);
        unitOfWork.Setup(u => u.Modules).Returns(modules.Object);
        unitOfWork.Setup(u => u.UserProgresses).Returns(userProgress.Object);

        var service = new TrackEnrollmentService(unitOfWork.Object);

        await service.EnrollAsync(userId, trackId, ct);

        users.Verify(r => r.GetByIdAsync(userId, ct), Times.Once);
        tracks.Verify(r => r.GetByIdAsync(trackId, ct), Times.Once);
        enrollments.Verify(r => r.GetAllAsync(ct), Times.Once);
        modules.Verify(r => r.GetByTrackIdAsync(trackId, ct), Times.Once);
        enrollments.Verify(r => r.AddAsync(It.IsAny<TrackEnrollment>(), ct), Times.Once);
        userProgress.Verify(r => r.AddAsync(It.IsAny<UserProgress>(), ct), Times.Once);
        unitOfWork.Verify(u => u.SaveChangesAsync(ct), Times.Once);
    }
}
