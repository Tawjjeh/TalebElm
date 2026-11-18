using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.Infrastructure;

public class LessonConfigurationTests
{
    [Fact]
    public void LessonType_ShouldBeRequiredIntegerColumnWithDefaultValueOne()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        using var context = new AppDbContext(options);

        var entityType = context.Model.FindEntityType(typeof(Lesson));

        Assert.NotNull(entityType);

        var property = entityType.FindProperty(nameof(Lesson.Type));

        Assert.NotNull(property);
        Assert.Equal(typeof(LessonType), property.ClrType);
        Assert.Equal("INTEGER", property.GetColumnType());
        Assert.False(property.IsNullable);
        Assert.Equal(LessonType.Text, (LessonType)property.GetDefaultValue()!);
    }

    [Fact]
    public void MigratedLessonTable_ShouldRejectUndefinedTypeValues()
    {
        using var connection = new SqliteConnection("Data Source=:memory:");
        connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(connection)
            .Options;

        using var context = new AppDbContext(options);
        context.Database.Migrate();

        using var command = connection.CreateCommand();
        command.CommandText =
            "SELECT sql FROM sqlite_master WHERE type = 'table' AND name = 'Lessons'";

        var schema = command.ExecuteScalar() as string;

        Assert.NotNull(schema);
        Assert.Contains("CK_Lessons_Type_Valid", schema);
        Assert.Contains("Type IN (1, 2, 3)", schema);
    }
}
