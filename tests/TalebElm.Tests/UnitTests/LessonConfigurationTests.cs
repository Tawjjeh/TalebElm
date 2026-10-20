using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class LessonConfigurationTests : SqliteTestBase
{
    [Fact]
    public void Model_ShouldConfigureLessonModuleRelationshipAndRestrictDelete()
    {
        var entityType = DbContext.Model.FindEntityType(typeof(Lesson));
        var foreignKey = entityType?.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Module));

        Assert.NotNull(foreignKey);
        Assert.Equal(nameof(Lesson.ModuleId), foreignKey.Properties.Single().Name);
        Assert.Equal(nameof(Module.Id), foreignKey.PrincipalKey.Properties.Single().Name);
        Assert.True(foreignKey.IsRequired);
        Assert.False(foreignKey.IsUnique);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
    }

    [Fact]
    public async Task DeleteModule_WithAssociatedLessons_ThrowsDbUpdateExceptionDueToRestrict()
    {
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

        var moduleToDelete = await DbContext.Modules.FindAsync(module.Id);
        Assert.NotNull(moduleToDelete);

        DbContext.Modules.Remove(moduleToDelete);

        var ex = await Assert.ThrowsAsync<DbUpdateException>(() => SaveChangesAsync());
        var sqliteEx = Assert.IsType<SqliteException>(ex.InnerException);
        Assert.Equal(19, sqliteEx.SqliteErrorCode); // SQLITE_CONSTRAINT
        Assert.Equal(1811, sqliteEx.SqliteExtendedErrorCode);
    }
}