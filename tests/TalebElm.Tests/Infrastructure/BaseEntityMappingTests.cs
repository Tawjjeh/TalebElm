
using TalebElm.Domain.Entities;

namespace TalebElm.Tests.Infrastructure;

public class BaseEntityMappingTests : SqliteTestBase
{
    [Theory]
    [InlineData(typeof(Exam))]
    [InlineData(typeof(Track))]
    public void BaseEntity_ShouldHaveCorrectMapping(Type entityClrType)
    {
        var entityType = DbContext.Model.FindEntityType(entityClrType);

        Assert.NotNull(entityType);

        // Id must be the primary key.
        var primaryKey = entityType.FindPrimaryKey();

        Assert.NotNull(primaryKey);
        Assert.Single(primaryKey.Properties);
        Assert.Equal(
            nameof(BaseEntity.Id),
            primaryKey.Properties[0].Name);

        // Id must be a Guid.
        Assert.Equal(
            typeof(Guid),
            entityType.FindProperty(nameof(BaseEntity.Id))!.ClrType);

        // CreatedAt must be required and use DateTimeOffset.
        var createdAt = entityType.FindProperty(
            nameof(BaseEntity.CreatedAt));

        Assert.NotNull(createdAt);
        Assert.Equal(typeof(DateTimeOffset), createdAt.ClrType);
        Assert.False(createdAt.IsNullable);
    }
}
