using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class LessonConfigurationTests : SqliteTestBase
{
    [Fact]
    public void Model_ShouldConfigureLessonModuleRelationshipAndRestrictDelete()
    {
        // Act
        var entityType = DbContext.Model.FindEntityType(typeof(Lesson));
        var foreignKey = entityType?.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Module));

        // Assert
        Assert.NotNull(foreignKey);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        Assert.Equal(nameof(Lesson.ModuleId), foreignKey.Properties.Single().Name);
    }

    [Fact]
    public async Task DeleteModule_WithAssociatedLessons_ThrowsDbUpdateExceptionDueToRestrict()
    {
        // Arrange
        var track = new Track { Id = Guid.NewGuid(), Name = "Backend", Description = "Backend track" };
        var module = new Module { Id = Guid.NewGuid(), Title = "C# Fundamentals", Order = 1, TrackId = track.Id };
        var lesson = new Lesson 
        { 
            Id = Guid.NewGuid(), 
            Title = "Variables", 
            Content = "Content", 
            Order = 1, 
            ModuleId = module.Id 
        };

        DbContext.Tracks.Add(track);
        DbContext.Modules.Add(module);
        DbContext.Lessons.Add(lesson);
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