using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.UnitTests;

public class AppDbContextModelTests
{
    [Fact]
    public void UserProgressConfiguration_ShouldBeApplied()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        using var context = new AppDbContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(UserProgress));

        // Assert
        Assert.NotNull(entityType);

        var primaryKey = entityType.FindPrimaryKey();

        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal(nameof(UserProgress.Id), primaryKey.Properties[0].Name);

        var index = entityType
            .GetIndexes()
            .Single(x => x.Properties.Select(p => p.Name)
                .SequenceEqual(new[]
                {
                    nameof(UserProgress.UserId),
                    nameof(UserProgress.ModuleId)
                }));

        Assert.True(index.IsUnique);
    }
}