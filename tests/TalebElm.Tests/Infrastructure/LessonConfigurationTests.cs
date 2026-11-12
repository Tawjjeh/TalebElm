
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
        Assert.Equal(1,(int)(LessonType) property.GetDefaultValue());
    }
}
