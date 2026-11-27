using Microsoft.JSInterop.Infrastructure;
using Moq;
using TalebElm.Api.Services;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Tests.UnitTests;

public class LessonServiceTests
{
    private static readonly Guid StudentId = Guid.Parse("11111111-1111-1111-1111-111111111111");

    [Fact]
    public async Task GetByModuleAsync_ReturnsLessonsOrderedByOrder()
    {
        var moduleId = Guid.NewGuid();
        var lessons = new List<Lesson>
        {
            new() { Id = Guid.NewGuid(), Title = "Lesson 2", Content = "Second", Order = 2, ModuleId = moduleId },
            new() { Id = Guid.NewGuid(), Title = "Lesson 1", Content = "First", Order = 1, ModuleId = moduleId }
        };

        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateModule(moduleId));

        var lessonRepository = new Mock<ILessonRepository>();
        lessonRepository.Setup(r => r.GetByModuleIdAsync(moduleId, It.IsAny<CancellationToken>())).ReturnsAsync(lessons);

        var service = CreateService(modules, lessonRepository, UnlockedProgress(moduleId));
        var result = await service.GetByModuleAsync(moduleId);

        Assert.Equal(new[] { "Lesson 1", "Lesson 2" }, result.Select(lesson => lesson.Title));
        Assert.All(result, lesson => Assert.Equal(moduleId, lesson.ModuleId));
    }

    [Fact]
    public async Task GetByModuleAsync_WhenModuleDoesNotExist_ThrowsNotFoundException()
    {
        var moduleId = Guid.NewGuid();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId, It.IsAny<CancellationToken>())).ReturnsAsync((Module?)null);

        var lessonRepository = new Mock<ILessonRepository>();
        var service = CreateService(modules, lessonRepository);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByModuleAsync(moduleId));
        lessonRepository.Verify(r => r.GetByModuleIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByModuleAsync_WhenTrackIsNotPublished_ThrowsNotFoundException()
    {
        var moduleId = Guid.NewGuid();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateModule(moduleId, TrackStatus.Draft));

        var lessonRepository = new Mock<ILessonRepository>();
        var service = CreateService(modules, lessonRepository);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByModuleAsync(moduleId));
        lessonRepository.Verify(r => r.GetByModuleIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByModuleAsync_WhenNoLessonsExist_ReturnsEmptyList()
    {
        var moduleId = Guid.NewGuid();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateModule(moduleId));

        var lessonRepository = new Mock<ILessonRepository>();
        lessonRepository.Setup(r => r.GetByModuleIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(Array.Empty<Lesson>());

        var service = CreateService(modules, lessonRepository, UnlockedProgress(moduleId));
        var result = await service.GetByModuleAsync(moduleId);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetByModuleAsync_WithoutAuthenticatedUser_ThrowsUnauthorized()
    {
        var moduleId = Guid.NewGuid();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateModule(moduleId));

        var lessonRepository = new Mock<ILessonRepository>();
        var progress = new Mock<IUserProgressRepository>();

        var service = CreateService(modules, lessonRepository, progress, user: Anonymous());

        await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetByModuleAsync(moduleId));

        progress.Verify(
            r => r.GetByUserAndModuleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByModuleAsync_WhenUserHasNoProgress_ThrowsForbidden()
    {
        var moduleId = Guid.NewGuid();
        var modules = new Mock<IModuleRepository>();

        modules.Setup(r => r.GetByIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateModule(moduleId));

        var lessonRepository = new Mock<ILessonRepository>();
        var progress = new Mock<IUserProgressRepository>();

        progress.Setup(r => r.GetByUserAndModuleAsync(StudentId, moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProgress?)null);

        var service = CreateService(modules, lessonRepository, progress);

        await Assert.ThrowsAsync<ForbiddenExcpetion>(() => service.GetByModuleAsync(moduleId));
        lessonRepository.Verify(r => r.GetByModuleIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByModuleAsync_WhenModuleIsLocked_ThrowsForbidden()
    {
        var moduleId = Guid.NewGuid();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(CreateModule(moduleId));

        var lessonRepository = new Mock<ILessonRepository>();
        var progress = new Mock<IUserProgressRepository>();
        progress.Setup(r => r.GetByUserAndModuleAsync(StudentId, moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProgress { UserId = StudentId, ModuleId = moduleId, IsUnlocked = false });

        var service = CreateService(modules, lessonRepository, progress);

        await Assert.ThrowsAsync<ForbiddenExcpetion>(() => service.GetByModuleAsync(moduleId));
        lessonRepository.Verify(r => r.GetByModuleIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenLessonExists_ReturnsDetailWithOrderedResources()
    {
        var lessonId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var lesson = new Lesson
        {
            Id = lessonId,
            Title = "C# Basics",
            Content = "Introduction to C#",
            Order = 3,
            ModuleId = moduleId,
            Module = CreateModule(moduleId)
        };

        var resources = new List<LessonResource>
        {
            new() { Id = Guid.NewGuid(), Title = "Video", Kind = LessonResourceKind.Video, Url = "https://example.com/video", Order = 2, LessonId = lessonId },
            new() { Id = Guid.NewGuid(), Title = "Docs", Kind = LessonResourceKind.Documentation, Citation = "C# spec", Order = 1, LessonId = lessonId }
        };

        var lessons = new Mock<ILessonRepository>();
        lessons.Setup(r => r.GetByIdAsync(lessonId, It.IsAny<CancellationToken>())).ReturnsAsync(lesson);

        var lessonResources = new Mock<ILessonResourceRepository>();
        lessonResources.Setup(r => r.GetByLessonIdAsync(lessonId, It.IsAny<CancellationToken>())).ReturnsAsync(resources);

        var service = CreateService(
            new Mock<IModuleRepository>(),
            lessons,
            UnlockedProgress(moduleId),
            lessonResources);

        var result = await service.GetByIdAsync(lessonId);

        Assert.Equal(lessonId, result.Id);
        Assert.Equal(moduleId, result.ModuleId);
        Assert.Equal("C# Basics", result.Title);
        Assert.Equal("Introduction to C#", result.Content);
        Assert.Equal(3, result.Order);
        Assert.Equal(new[] { "Docs", "Video" }, result.Resources.Select(resource => resource.Title));
        Assert.Equal(LessonResourceKind.Documentation, result.Resources[0].Kind);
        Assert.Equal("C# spec", result.Resources[0].Citation);
       
        Assert.Equal(LessonResourceKind.Video, result.Resources[1].Kind);

        Assert.Equal("https://example.com/video", result.Resources[1].Url);
    }

    [Fact]
    public async Task GetByIdAsync_WhenLessonDoesNotExist_ThrowsNotFoundException()
    {
        var lessonId = Guid.NewGuid();
        var lessons = new Mock<ILessonRepository>();
        lessons.Setup(r => r.GetByIdAsync(lessonId, It.IsAny<CancellationToken>())).ReturnsAsync((Lesson?)null);

        var lessonResources = new Mock<ILessonResourceRepository>();
        var service = CreateService(new Mock<IModuleRepository>(), lessons, lessonResources: lessonResources);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(lessonId));
        lessonResources.Verify(r => r.GetByLessonIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenLessonIsInDraftTrack_ThrowsNotFoundException()
    {
        var lessonId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var lessons = new Mock<ILessonRepository>();
        lessons.Setup(r => r.GetByIdAsync(lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Lesson
            {
                Id = lessonId,
                ModuleId = moduleId,
                Module = CreateModule(moduleId, TrackStatus.Draft)
            });

        var lessonResources = new Mock<ILessonResourceRepository>();
        var service = CreateService(new Mock<IModuleRepository>(), lessons, lessonResources: lessonResources);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(lessonId));
        lessonResources.Verify(r => r.GetByLessonIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WithoutAuthenticatedUser_ThrowsUnauthorized()
    {
        var lessonId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var lessons = new Mock<ILessonRepository>();
        lessons.Setup(r => r.GetByIdAsync(lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Lesson
            {
                Id = lessonId,
                ModuleId = moduleId,
                Module = CreateModule(moduleId)
            });

        var progress = new Mock<IUserProgressRepository>();
        var service = CreateService(
            new Mock<IModuleRepository>(),
            lessons,
            progress,
            user: Anonymous());

        await Assert.ThrowsAsync<UnauthorizedException>(() => service.GetByIdAsync(lessonId));
        progress.Verify(
            r => r.GetByUserAndModuleAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Fact]
    public async Task GetByIdAsync_WhenModuleIsLocked_ThrowsForbidden()
    {
        var lessonId = Guid.NewGuid();
        var moduleId = Guid.NewGuid();
        var lessons = new Mock<ILessonRepository>();
        lessons.Setup(r => r.GetByIdAsync(lessonId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Lesson
            {
                Id = lessonId,
                ModuleId = moduleId,
                Module = CreateModule(moduleId)
            });

        var progress = new Mock<IUserProgressRepository>();
        progress.Setup(r => r.GetByUserAndModuleAsync(StudentId, moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProgress { UserId = StudentId, ModuleId = moduleId, IsUnlocked = false });

        var lessonResources = new Mock<ILessonResourceRepository>();
        var service = CreateService(new Mock<IModuleRepository>(), lessons, progress, lessonResources);

        await Assert.ThrowsAsync<ForbiddenExcpetion>(() => service.GetByIdAsync(lessonId));
        lessonResources.Verify(r => r.GetByLessonIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    private static LessonService CreateService(
        Mock<IModuleRepository> modules,
        Mock<ILessonRepository> lessons,
        Mock<IUserProgressRepository>? progresses = null,
        Mock<ILessonResourceRepository>? lessonResources = null,
        ICurrentUserService? user = null)
        => new(CreateUnitOfWork(modules, lessons, progresses, lessonResources).Object,
            user ?? SignedInAs(StudentId));

    //. Avoid writing the same lines 
    private static Mock<IUnitOfWork> CreateUnitOfWork(
        Mock<IModuleRepository> modules,
        Mock<ILessonRepository> lessons,
        Mock<IUserProgressRepository>? progresses = null,
        Mock<ILessonResourceRepository>? lessonResources = null)
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Modules).Returns(modules.Object);
        unitOfWork.Setup(u => u.Lessons).Returns(lessons.Object);
        unitOfWork.Setup(u => u.UserProgresses).Returns((progresses ?? new Mock<IUserProgressRepository>()).Object);
        unitOfWork.Setup(u => u.LessonResources).Returns((lessonResources ?? new Mock<ILessonResourceRepository>()).Object);
        return unitOfWork;
    }

    private static Mock<IUserProgressRepository> UnlockedProgress(Guid moduleId)
    {
        var progresses = new Mock<IUserProgressRepository>();
        progresses.Setup(r => r.GetByUserAndModuleAsync(StudentId, moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProgress { UserId = StudentId, ModuleId = moduleId, IsUnlocked = true });
        return progresses;
    }

    private static Module CreateModule(Guid moduleId, TrackStatus status = TrackStatus.Published)
        => new()
        {
            Id = moduleId,
            Title = "Module",
            Summary = "Summary",
            Order = 1,
            TrackId = Guid.NewGuid(),
            Track = new Track { Name = "Track", Description = "Track", Status = status }
        };

    //. for only Student or admin 
    private static ICurrentUserService SignedInAs(Guid userId) => new TestAuthenticationScheme(userId.ToString());

    private static ICurrentUserService Anonymous() => new TestAuthenticationScheme(null);

    //. This was creating for the anonymous user or the user that didn't login on server
    //. And will be private to be called 
    private sealed class TestAuthenticationScheme(string? userId) : ICurrentUserService
    {
        public string? UserId() => userId;
    }
}
