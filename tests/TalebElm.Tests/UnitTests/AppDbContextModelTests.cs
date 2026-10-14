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

    [Fact]
    public void UserConfiguration_ShouldBeApplied()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite("Data Source=:memory:")
            .Options;

        using var context = new AppDbContext(options);

        // Act
        var entityType = context.Model.FindEntityType(typeof(User));

        // Assert
        Assert.NotNull(entityType);

        Assert.False(entityType.FindProperty(nameof(User.Name))!.IsNullable);
        Assert.Equal(100, entityType.FindProperty(nameof(User.Name))!.GetMaxLength());
        Assert.False(entityType.FindProperty(nameof(User.Email))!.IsNullable);
        Assert.Equal(256, entityType.FindProperty(nameof(User.Email))!.GetMaxLength());
        Assert.False(entityType.FindProperty(nameof(User.PasswordHash))!.IsNullable);

        // Note: the NOCASE email collation is verified behaviorally by
        // UserUniqueIndexTests.SaveChangesAsync_CaseVariantEmail_ThrowsUniqueViolation.

        var emailIndex = Assert.Single(
            entityType.GetIndexes(),
            x => x.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(User.Email) }));
        Assert.True(emailIndex.IsUnique);

        // Note: the CK_Users_*_NotEmpty check constraints are verified behaviorally by
        // UserUniqueIndexTests.SaveChangesAsync_EmptyRequiredField_ThrowsDbUpdateException.
    }
}