using Moq;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Tests.UnitTests;

public class LessonServiceTests
{
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
        modules.Setup(r => r.GetByIdAsync(moduleId))
            .ReturnsAsync(new Module { Id = moduleId, Title = "Module", Order = 1, TrackId = Guid.NewGuid() });

        var lessonRepository = new Mock<ILessonRepository>();
        lessonRepository.Setup(r => r.GetByModuleIdAsync(moduleId)).ReturnsAsync(lessons);

        var service = new LessonService(CreateUnitOfWork(modules, lessonRepository).Object);
        var result = await service.GetByModuleAsync(moduleId);

        Assert.Equal(new[] { "Lesson 1", "Lesson 2" }, result.Select(lesson => lesson.Title));
        Assert.All(result, lesson => Assert.Equal(moduleId, lesson.ModuleId));
    }

    [Fact]
    public async Task GetByModuleAsync_WhenModuleDoesNotExist_ThrowsNotFoundException()
    {
        var moduleId = Guid.NewGuid();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId)).ReturnsAsync((Module?)null);

        var lessonRepository = new Mock<ILessonRepository>();
        var service = new LessonService(CreateUnitOfWork(modules, lessonRepository).Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByModuleAsync(moduleId));
        lessonRepository.Verify(r => r.GetByModuleIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    [Fact]
    public async Task GetByModuleAsync_WhenNoLessonsExist_ReturnsEmptyList()
    {
        var moduleId = Guid.NewGuid();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId))
            .ReturnsAsync(new Module { Id = moduleId, Title = "Module", Order = 1, TrackId = Guid.NewGuid() });

        var lessonRepository = new Mock<ILessonRepository>();
        lessonRepository.Setup(r => r.GetByModuleIdAsync(moduleId))
            .ReturnsAsync(Array.Empty<Lesson>());

        var service = new LessonService(CreateUnitOfWork(modules, lessonRepository).Object);
        var result = await service.GetByModuleAsync(moduleId);

        Assert.Empty(result);
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
            ModuleId = moduleId
        };

        var resources = new List<LessonResource>
        {
            new() { Id = Guid.NewGuid(), Title = "Video", Kind = LessonResourceKind.Video, Url = "https://example.com/video", Order = 2, LessonId = lessonId },
            new() { Id = Guid.NewGuid(), Title = "Docs", Kind = LessonResourceKind.Documentation, Citation = "C# spec", Order = 1, LessonId = lessonId }
        };

        var lessons = new Mock<ILessonRepository>();
        lessons.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync(lesson);

        var lessonResources = new Mock<ILessonResourceRepository>();
        lessonResources.Setup(r => r.GetByLessonIdAsync(lessonId)).ReturnsAsync(resources);

        var service = new LessonService(CreateUnitOfWork(
            new Mock<IModuleRepository>(),
            lessons,
            lessonResources).Object);

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
        lessons.Setup(r => r.GetByIdAsync(lessonId)).ReturnsAsync((Lesson?)null);

        var lessonResources = new Mock<ILessonResourceRepository>();
        var service = new LessonService(CreateUnitOfWork(
            new Mock<IModuleRepository>(),
            lessons,
            lessonResources).Object);

        await Assert.ThrowsAsync<NotFoundException>(() => service.GetByIdAsync(lessonId));
        lessonResources.Verify(r => r.GetByLessonIdAsync(It.IsAny<Guid>()), Times.Never);
    }

    private static Mock<IUnitOfWork> CreateUnitOfWork(
        Mock<IModuleRepository> modules,
        Mock<ILessonRepository> lessons,
        Mock<ILessonResourceRepository>? lessonResources = null)
    {
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.Setup(u => u.Modules).Returns(modules.Object);
        unitOfWork.Setup(u => u.Lessons).Returns(lessons.Object);
        unitOfWork.Setup(u => u.LessonResources).Returns((lessonResources ?? new Mock<ILessonResourceRepository>()).Object);
        return unitOfWork;
    }
}
