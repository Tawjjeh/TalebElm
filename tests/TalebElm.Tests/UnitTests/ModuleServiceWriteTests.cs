using FluentValidation;
using FluentValidation.Results;
using Moq;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Concrete;
using TalebElm.Application.Validators;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;
using ValidationException = TalebElm.Domain.Exceptions.ValidationException;

namespace TalebElm.Tests.UnitTests;

public class ModuleServiceWriteTests
{
    private static Mock<IUnitOfWork> TransactionalUnitOfWork()
    {
        var uow = new Mock<IUnitOfWork>();
        uow.Setup(u => u.ExecuteInTransactionAsync(
                It.IsAny<Func<CancellationToken, Task>>(),
                It.IsAny<CancellationToken>()))
            .Returns<Func<CancellationToken, Task>, CancellationToken>(
                (action, ct) => action(ct));
        return uow;
    }

    private static Mock<IValidator<CreateModuleRequest>> ValidCreateValidator()
    {
        var validator = new Mock<IValidator<CreateModuleRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<CreateModuleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return validator;
    }

    private static Mock<IValidator<UpdateModuleRequest>> ValidUpdateValidator()
    {
        var validator = new Mock<IValidator<UpdateModuleRequest>>();
        validator.Setup(v => v.ValidateAsync(It.IsAny<UpdateModuleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());
        return validator;
    }

    private static ModuleService CreateService(
        Mock<IUnitOfWork> uow,
        Mock<IValidator<CreateModuleRequest>>? createValidator = null,
        Mock<IValidator<UpdateModuleRequest>>? updateValidator = null)
        => new(uow.Object,
            (createValidator ?? ValidCreateValidator()).Object,
            (updateValidator ?? ValidUpdateValidator()).Object);

    [Fact]
    public async Task CreateAsync_InsertsAtOrderAndShiftsSiblings()
    {
        var trackId = Guid.NewGuid();
        var siblings = new List<Module>
        {
            new() { Id = Guid.NewGuid(), Title = "M1", Order = 1, TrackId = trackId },
            new() { Id = Guid.NewGuid(), Title = "M2", Order = 2, TrackId = trackId }
        };

        var uow = TransactionalUnitOfWork();
        uow.Setup(u => u.Tracks.GetByIdAsync(trackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Track { Id = trackId, Name = "T" });
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByTrackIdForUpdateAsync(trackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(siblings);
        uow.Setup(u => u.Modules).Returns(modules.Object);

        var service = CreateService(uow);

        var result = await service.CreateAsync(
            new CreateModuleRequest("New", "Summary", 2, trackId));

        Assert.Equal(2, result.Order);
        // The new module is not in the siblings list; only M2 moves (2 -> 3).
        Assert.Equal([1, 3], siblings.Select(m => m.Order).OrderBy(o => o));
        Assert.Equal("M2", siblings.Single(m => m.Order == 3).Title);
        modules.Verify(r => r.AddAsync(It.IsAny<Module>(), It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenTrackDoesNotExist_ThrowsNotFound()
    {
        var uow = TransactionalUnitOfWork();
        uow.Setup(u => u.Tracks.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Track?)null);

        var service = CreateService(uow);

        await Assert.ThrowsAsync<NotFoundException>(() => service.CreateAsync(
            new CreateModuleRequest("New", "Summary", 1, Guid.NewGuid())));

        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestInvalid_ThrowsFluentValidation()
    {
        var createValidator = new Mock<IValidator<CreateModuleRequest>>();
        createValidator.Setup(v => v.ValidateAsync(It.IsAny<CreateModuleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult([new ValidationFailure("Title", "Module.Title.Required")]));

        var service = CreateService(TransactionalUnitOfWork(), createValidator);

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(() => service.CreateAsync(
            new CreateModuleRequest("", "Summary", 1, Guid.NewGuid())));
    }

    [Fact]
    public async Task CreateAsync_OrderBeyondAppend_ThrowsValidation()
    {
        var trackId = Guid.NewGuid();
        var siblings = new List<Module>
        {
            new() { Id = Guid.NewGuid(), Title = "M1", Order = 1, TrackId = trackId }
        };

        var uow = TransactionalUnitOfWork();
        uow.Setup(u => u.Tracks.GetByIdAsync(trackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Track { Id = trackId, Name = "T" });
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByTrackIdForUpdateAsync(trackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(siblings);
        uow.Setup(u => u.Modules).Returns(modules.Object);

        var service = CreateService(uow);

        await Assert.ThrowsAsync<ValidationException>(() => service.CreateAsync(
            new CreateModuleRequest("New", "Summary", 3, trackId)));
    }

    [Fact]
    public async Task UpdateAsync_SameOrder_UpdatesFieldsWithoutMoving()
    {
        var moduleId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var module = new Module { Id = moduleId, Title = "Old", Summary = "Old", Order = 2, TrackId = trackId };

        var uow = TransactionalUnitOfWork();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(module);
        uow.Setup(u => u.Modules).Returns(modules.Object);

        var service = CreateService(uow);

        var result = await service.UpdateAsync(moduleId,
            new UpdateModuleRequest("New", "New summary", 2, trackId));

        Assert.Equal("New", result.Title);
        Assert.Equal("New summary", result.Summary);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.ExecuteInTransactionAsync(It.IsAny<Func<CancellationToken, Task>>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_OrderChange_MovesModuleWithinTrack()
    {
        var trackId = Guid.NewGuid();
        var modules = new List<Module>
        {
            new() { Id = Guid.NewGuid(), Title = "M1", Order = 1, TrackId = trackId },
            new() { Id = Guid.NewGuid(), Title = "M2", Order = 2, TrackId = trackId },
            new() { Id = Guid.NewGuid(), Title = "M3", Order = 3, TrackId = trackId }
        };
        var moving = modules[0];

        var uow = TransactionalUnitOfWork();
        var moduleRepository = new Mock<IModuleRepository>();
        moduleRepository.Setup(r => r.GetByIdAsync(moving.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(moving);
        moduleRepository.Setup(r => r.GetByTrackIdForUpdateAsync(trackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(modules);
        uow.Setup(u => u.Modules).Returns(moduleRepository.Object);

        var service = CreateService(uow);

        var result = await service.UpdateAsync(moving.Id,
            new UpdateModuleRequest("M1 renamed", "Summary", 3, trackId));

        Assert.Equal(3, result.Order);
        Assert.Equal("M1 renamed", result.Title);
        Assert.Equal([1, 2, 3], modules.Select(m => m.Order).OrderBy(o => o));
        Assert.Equal("M2", modules.Single(m => m.Order == 1).Title);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Exactly(2));
    }

    [Fact]
    public async Task UpdateAsync_TrackChange_ThrowsValidation()
    {
        var moduleId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var module = new Module { Id = moduleId, Title = "M", Summary = "S", Order = 1, TrackId = trackId };

        var uow = TransactionalUnitOfWork();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(module);
        uow.Setup(u => u.Modules).Returns(modules.Object);

        var service = CreateService(uow);

        await Assert.ThrowsAsync<ValidationException>(() => service.UpdateAsync(moduleId,
            new UpdateModuleRequest("M", "S", 1, Guid.NewGuid())));

        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task UpdateAsync_WhenModuleDoesNotExist_ThrowsNotFound()
    {
        var uow = TransactionalUnitOfWork();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Module?)null);
        uow.Setup(u => u.Modules).Returns(modules.Object);

        var service = CreateService(uow);

        await Assert.ThrowsAsync<NotFoundException>(() => service.UpdateAsync(Guid.NewGuid(),
            new UpdateModuleRequest("M", "S", 1, Guid.NewGuid())));
    }

    [Fact]
    public async Task DeleteAsync_CompactsRemainingSiblings()
    {
        var trackId = Guid.NewGuid();
        var modules = new List<Module>
        {
            new() { Id = Guid.NewGuid(), Title = "M1", Order = 1, TrackId = trackId },
            new() { Id = Guid.NewGuid(), Title = "M2", Order = 2, TrackId = trackId },
            new() { Id = Guid.NewGuid(), Title = "M3", Order = 3, TrackId = trackId }
        };
        var deleted = modules[1];

        var uow = TransactionalUnitOfWork();
        var moduleRepository = new Mock<IModuleRepository>();
        moduleRepository.Setup(r => r.GetByIdAsync(deleted.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(deleted);
        moduleRepository.Setup(r => r.GetByTrackIdForUpdateAsync(trackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(modules);
        uow.Setup(u => u.Modules).Returns(moduleRepository.Object);
        var lessons = new Mock<ILessonRepository>();
        lessons.Setup(r => r.GetByModuleIdAsync(deleted.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        uow.Setup(u => u.Lessons).Returns(lessons.Object);
        var exams = new Mock<IExamRepository>();
        exams.Setup(r => r.GetByModuleIdAsync(deleted.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exam?)null);
        uow.Setup(u => u.Exams).Returns(exams.Object);
        var progress = new Mock<IUserProgressRepository>();
        progress.Setup(r => r.ExistsForModuleAsync(deleted.Id, It.IsAny<CancellationToken>()))
            .ReturnsAsync(false);
        uow.Setup(u => u.UserProgresses).Returns(progress.Object);

        var service = CreateService(uow);

        await service.DeleteAsync(deleted.Id);

        Assert.Equal([1, 2], modules.Where(m => m.Id != deleted.Id).Select(m => m.Order).OrderBy(o => o));
        moduleRepository.Verify(r => r.RemoveAsync(deleted, It.IsAny<CancellationToken>()), Times.Once);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_WhenModuleDoesNotExist_ThrowsNotFound()
    {
        var uow = TransactionalUnitOfWork();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Module?)null);
        uow.Setup(u => u.Modules).Returns(modules.Object);

        var service = CreateService(uow);

        await Assert.ThrowsAsync<NotFoundException>(() => service.DeleteAsync(Guid.NewGuid()));
    }

    [Fact]
    public async Task DeleteAsync_WithLessons_ThrowsConflictWithoutRemoving()
    {
        var moduleId = Guid.NewGuid();
        var module = new Module { Id = moduleId, Title = "M", Order = 1, TrackId = Guid.NewGuid() };

        var uow = TransactionalUnitOfWork();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(module);
        uow.Setup(u => u.Modules).Returns(modules.Object);
        var lessons = new Mock<ILessonRepository>();
        lessons.Setup(r => r.GetByModuleIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([new Lesson { Id = Guid.NewGuid(), ModuleId = moduleId, Title = "L", Order = 1 }]);
        uow.Setup(u => u.Lessons).Returns(lessons.Object);

        var service = CreateService(uow);

        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(moduleId));

        modules.Verify(r => r.RemoveAsync(It.IsAny<Module>(), It.IsAny<CancellationToken>()), Times.Never);
        uow.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task DeleteAsync_WithExam_ThrowsConflict()
    {
        var moduleId = Guid.NewGuid();
        var module = new Module { Id = moduleId, Title = "M", Order = 1, TrackId = Guid.NewGuid() };

        var uow = TransactionalUnitOfWork();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(module);
        uow.Setup(u => u.Modules).Returns(modules.Object);
        var lessons = new Mock<ILessonRepository>();
        lessons.Setup(r => r.GetByModuleIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        uow.Setup(u => u.Lessons).Returns(lessons.Object);
        var exams = new Mock<IExamRepository>();
        exams.Setup(r => r.GetByModuleIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new Exam { Id = Guid.NewGuid(), ModuleId = moduleId, Title = "E", PassThreshold = 60 });
        uow.Setup(u => u.Exams).Returns(exams.Object);

        var service = CreateService(uow);

        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(moduleId));
    }

    [Fact]
    public async Task DeleteAsync_WithProgress_ThrowsConflict()
    {
        var moduleId = Guid.NewGuid();
        var module = new Module { Id = moduleId, Title = "M", Order = 1, TrackId = Guid.NewGuid() };

        var uow = TransactionalUnitOfWork();
        var modules = new Mock<IModuleRepository>();
        modules.Setup(r => r.GetByIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(module);
        uow.Setup(u => u.Modules).Returns(modules.Object);
        var lessons = new Mock<ILessonRepository>();
        lessons.Setup(r => r.GetByModuleIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync([]);
        uow.Setup(u => u.Lessons).Returns(lessons.Object);
        var exams = new Mock<IExamRepository>();
        exams.Setup(r => r.GetByModuleIdAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Exam?)null);
        uow.Setup(u => u.Exams).Returns(exams.Object);
        var progress = new Mock<IUserProgressRepository>();
        progress.Setup(r => r.ExistsForModuleAsync(moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(true);
        uow.Setup(u => u.UserProgresses).Returns(progress.Object);

        var service = CreateService(uow);

        await Assert.ThrowsAsync<ConflictException>(() => service.DeleteAsync(moduleId));
    }
}
