using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Tests.Infrastructure;
using Xunit;

namespace TalebElm.Tests.UnitTests;

public class ExamAndUserProgressConfigurationTests : SqliteTestBase
{
    [Fact]
    public void Model_ShouldConfigureExamModuleOneToOneAndRestrictDelete()
    {
        // Act
        var entityType = DbContext.Model.FindEntityType(typeof(Exam));
        var foreignKey = entityType?.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Module));

        // Assert
        Assert.NotNull(foreignKey);
        Assert.True(foreignKey.IsUnique);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        Assert.Equal(nameof(Exam.ModuleId), foreignKey.Properties.Single().Name);
    }

    [Fact]
    public void Model_ShouldConfigureUserProgressForeignKeysAndRestrictDelete()
    {
        // Act
        var entityType = DbContext.Model.FindEntityType(typeof(UserProgress));
        
        var userFk = entityType?.GetForeignKeys()
            .FirstOrDefault(fk => fk.Properties.Any(p => p.Name == nameof(UserProgress.UserId)));

        var moduleFk = entityType?.GetForeignKeys()
            .FirstOrDefault(fk => fk.Properties.Any(p => p.Name == nameof(UserProgress.ModuleId)));

        // Assert
        Assert.NotNull(userFk);
        Assert.True(userFk.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, userFk.DeleteBehavior);

        Assert.NotNull(moduleFk);
        Assert.True(moduleFk.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, moduleFk.DeleteBehavior);
    }

    [Fact]
    public async Task DeleteModule_WithAssociatedExam_ThrowsDbUpdateException()
    {
        // Arrange
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Desc" };
        var module = new Module { Id = Guid.NewGuid(), Title = "C# Advanced", Order = 1, TrackId = track.Id };
        var exam = new Exam { Id = Guid.NewGuid(), Title = "C# Exam", PassThreshold = 70, ModuleId = module.Id };

        DbContext.Tracks.Add(track);
        DbContext.Modules.Add(module);
        DbContext.Exams.Add(exam);
        await SaveChangesAsync();
        ClearTracker();

        // Act
        var moduleToDelete = await DbContext.Modules.FindAsync(module.Id);
        Assert.NotNull(moduleToDelete);
        DbContext.Modules.Remove(moduleToDelete);

        // Assert
        await Assert.ThrowsAsync<DbUpdateException>(async () => await SaveChangesAsync());
    }
}