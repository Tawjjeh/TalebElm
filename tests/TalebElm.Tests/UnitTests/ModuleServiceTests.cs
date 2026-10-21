using Moq;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Tests.UnitTests;

public class ModuleServiceTests
{
    [Fact]
    public async Task GetByTrackAsync_ReturnsModulesOrderedByOrder()
    {
        var trackId = Guid.NewGuid();
        var modules = new List<Module>
        {
            new() { Id = Guid.NewGuid(), Title = "Module 2", Summary = "Second", Order = 2, TrackId = trackId },
            new() { Id = Guid.NewGuid(), Title = "Module 1", Summary = "First", Order = 1, TrackId = trackId }
        };

        var tracks = new Mock<ITrackRepository>();
        tracks.Setup(r => r.GetByIdAsync(trackId))
            .ReturnsAsync(new Track { Id = trackId, Name = "Backend" });

        var moduleRepository = new Mock<IModuleRepository>();
        moduleRepository.Setup(r => r.GetByTrackIdAsync(trackId))
            .ReturnsAsync(modules);

        var service = new ModuleService(CreateUnitOfWork(tracks, moduleRepository).Object);
        var result = await service.GetByTrackAsync(trackId);

        Assert.Equal(new[] { "Module 1", "Module 2" }, result.Select(module => module.Title));
        Assert.Equal(new[] { 1, 2 }, result.Select(module => module.Order));
    }

    [Fact]
    public async Task GetByTrackAsync_WhenTrackDoesNotExist_ThrowsNotFoundException()
    {
        var trackId = Guid.NewGuid();
        var tracks = new Mock<ITrackRepository>();
        tracks.Setup(r => r.GetByIdAsync(trackId)).ReturnsAsync((Track?)null);

        var moduleRepository = new Mock<IModuleRepository>();
        var service = new ModuleService(CreateUnitOfWork(tracks, moduleRepository).Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByTrackAsync(trackId));
        moduleRepository.Verify(r => r.GetByTrackIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenModuleExists_ReturnsModuleResponse()
    {
        var moduleId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var module = new Module
        {
            Id = moduleId,
            Title = "C# Fundamentals",
            Summary = "Introduction to C#",
            Order = 1,
            TrackId = trackId
        };

        var moduleRepository = new Mock<IModuleRepository>();
        moduleRepository.Setup(r => r.GetByIdAsync(moduleId)).ReturnsAsync(module);

        var service = new ModuleService(CreateUnitOfWork(new Mock<ITrackRepository>(), moduleRepository).Object);
        var result = await service.GetByIdAsync(moduleId);

        Assert.Equal(moduleId, result.Id);
        Assert.Equal("C# Fundamentals", result.Title);
        Assert.Equal("Introduction to C#", result.Summary);
        Assert.Equal(1, result.Order);
        Assert.Equal(trackId, result.TrackId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenModuleDoesNotExist_ThrowsNotFoundException()
    {
        var moduleId = Guid.NewGuid();
        var moduleRepository = new Mock<IModuleRepository>();
        moduleRepository.Setup(r => r.GetByIdAsync(moduleId)).ReturnsAsync((Module?)null);

        var service = new ModuleService(CreateUnitOfWork(new Mock<ITrackRepository>(), moduleRepository).Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(moduleId));
    }

    private static Mock<IUnitOfWork> CreateUnitOfWork(
        Mock<ITrackRepository> tracks,
        Mock<IModuleRepository> modules)
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Tracks).Returns(tracks.Object);
        unitOfWork.Setup(u => u.Modules).Returns(modules.Object);
        return unitOfWork;
    }
}
