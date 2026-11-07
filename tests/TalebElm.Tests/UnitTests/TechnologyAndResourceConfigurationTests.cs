using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;
using TalebElm.Tests.Infrastructure;
using Xunit;

namespace TalebElm.Tests.UnitTests;

public class TechnologyAndResourceConfigurationTests : SqliteTestBase
{
    private static Technology NewTechnology(string slug, int order, Guid? parentId = null)
        => new()
        {
            Id = Guid.NewGuid(),
            Name = slug,
            Slug = slug,
            Order = order,
            ParentId = parentId
        };

    [Fact]
    public void Model_ShouldConfigureTechnologyParentAsSelfReferenceWithRestrictDelete()
    {
        var entityType = DbContext.Model.FindEntityType(typeof(Technology));
        var foreignKey = entityType?.GetForeignKeys()
            .FirstOrDefault(fk => fk.PrincipalEntityType.ClrType == typeof(Technology));

        Assert.NotNull(foreignKey);
        Assert.False(foreignKey.IsRequired);
        Assert.Equal(DeleteBehavior.Restrict, foreignKey.DeleteBehavior);
        Assert.Equal(nameof(Technology.ParentId), foreignKey.Properties.Single().Name);
    }

    [Fact]
    public void Model_ShouldConfigureFilteredUniqueIndexesForSlugAndOrder()
    {
        var entityType = DbContext.Model.FindEntityType(typeof(Technology));
        var indexes = entityType!.GetIndexes().Where(i => i.IsUnique).ToList();

        const string withParent = "\"ParentId\" IS NOT NULL";
        const string withoutParent = "\"ParentId\" IS NULL";

        Assert.Contains(indexes, i => i.GetFilter() == withParent
            && i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(Technology.ParentId), nameof(Technology.Slug) }));
        Assert.Contains(indexes, i => i.GetFilter() == withoutParent
            && i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(Technology.Slug) }));
        Assert.Contains(indexes, i => i.GetFilter() == withParent
            && i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(Technology.ParentId), nameof(Technology.Order) }));
        Assert.Contains(indexes, i => i.GetFilter() == withoutParent
            && i.Properties.Select(p => p.Name).SequenceEqual(new[] { nameof(Technology.Order) }));
    }

    [Fact]
    public async Task SaveTechnology_WithDuplicateSlugUnderSameParent_ThrowsDbUpdateException()
    {
        var parent = NewTechnology("backend", 1);
        DbContext.Technologies.Add(parent);
        DbContext.Technologies.Add(NewTechnology("basics", 1, parent.Id));
        await SaveChangesAsync();

        DbContext.Technologies.Add(NewTechnology("basics", 2, parent.Id));

        await Assert.ThrowsAsync<DbUpdateException>(async () => await SaveChangesAsync());
    }

    [Fact]
    public async Task SaveTechnology_WithSameSlugUnderDifferentParents_IsAccepted()
    {
        var backend = NewTechnology("backend", 1);
        var frontend = NewTechnology("frontend", 2);
        DbContext.Technologies.AddRange(backend, frontend);
        DbContext.Technologies.Add(NewTechnology("basics", 1, backend.Id));
        DbContext.Technologies.Add(NewTechnology("basics", 1, frontend.Id));

        await SaveChangesAsync();
    }

    [Fact]
    public async Task SaveTechnology_WithDuplicateRootSlug_ThrowsDbUpdateException()
    {
        DbContext.Technologies.Add(NewTechnology("dotnet", 1));
        await SaveChangesAsync();

        DbContext.Technologies.Add(NewTechnology("dotnet", 2));

        await Assert.ThrowsAsync<DbUpdateException>(async () => await SaveChangesAsync());
    }

    [Fact]
    public async Task SaveTechnology_WithDuplicateRootOrder_ThrowsDbUpdateException()
    {
        DbContext.Technologies.Add(NewTechnology("dotnet", 1));
        await SaveChangesAsync();

        DbContext.Technologies.Add(NewTechnology("java", 1));

        await Assert.ThrowsAsync<DbUpdateException>(async () => await SaveChangesAsync());
    }

    [Fact]
    public async Task DeleteTechnology_WithChildren_ThrowsDbUpdateException()
    {
        var parent = NewTechnology("backend", 1);
        DbContext.Technologies.Add(parent);
        DbContext.Technologies.Add(NewTechnology("dotnet", 1, parent.Id));
        await SaveChangesAsync();
        ClearTracker();

        var parentToDelete = await DbContext.Technologies.FindAsync(parent.Id);
        Assert.NotNull(parentToDelete);
        DbContext.Technologies.Remove(parentToDelete);

        await Assert.ThrowsAsync<DbUpdateException>(async () => await SaveChangesAsync());
    }

    [Fact]
    public void Model_ShouldConfigureResourceUrlMaxLengthAndTechnologyStatusIndex()
    {
        var entityType = DbContext.Model.FindEntityType(typeof(Resource));
        var urlProperty = entityType?.FindProperty(nameof(Resource.Url));
        var index = entityType?.GetIndexes().FirstOrDefault(i =>
            i.Properties.Select(p => p.Name)
                .SequenceEqual(new[] { nameof(Resource.TechnologyId), nameof(Resource.Status) }));

        Assert.NotNull(urlProperty);
        Assert.Equal(2000, urlProperty.GetMaxLength());
        Assert.NotNull(index);
    }
}
