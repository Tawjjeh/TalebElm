using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class TechnologyRepositoryTests : SqliteTestBase
{
    private readonly TechnologyRepository _repository;

    public TechnologyRepositoryTests() => _repository = new TechnologyRepository(DbContext);

    private static Technology NewTech(string slug, int order, Guid? parentId = null, TechnologyKind kind = TechnologyKind.Language)
        => new() { Name = slug, Slug = slug, Order = order, ParentId = parentId, Kind = kind };

    [Fact]
    public async Task GetByParentIdAsync_WhenParentIsNull_ReturnsRootsInOrder()
    {
        var python = NewTech("python", 2);
        var csharp = NewTech("csharp", 1);
        var child = NewTech("aspnet-core", 1, csharp.Id, TechnologyKind.Framework);

        DbContext.Technologies.AddRange(python, csharp, child);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByParentIdAsync(null);

        Assert.Equal(2, result.Count);
        Assert.Equal(csharp.Id, result[0].Id);
        Assert.Equal(python.Id, result[1].Id);
    }

    [Fact]
    public async Task GetByParentIdAsync_ReturnsOnlyChildrenOfParent()
    {
        var csharp = NewTech("csharp", 1);
        var python = NewTech("python", 2);
        var aspnet = NewTech("aspnet-core", 1, csharp.Id, TechnologyKind.Framework);
        var django = NewTech("django", 1, python.Id, TechnologyKind.Framework);

        DbContext.Technologies.AddRange(csharp, python, aspnet, django);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetByParentIdAsync(csharp.Id);

        Assert.Single(result);
        Assert.Equal(aspnet.Id, result[0].Id);
    }

    [Fact]
    public async Task GetChildCountsAsync_WhenParentIdsAreEmpty_ReturnsEmpty()
    {
        var result = await _repository.GetChildCountsAsync([]);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetChildCountsAsync_ReturnsDirectChildCountsPerParent()
    {
        var csharp = NewTech("csharp", 1);
        var python = NewTech("python", 2);
        var aspnet = NewTech("aspnet-core", 1, csharp.Id, TechnologyKind.Framework);
        var efCore = NewTech("ef-core", 2, csharp.Id, TechnologyKind.Framework);
        var django = NewTech("django", 1, python.Id, TechnologyKind.Framework);
        var dependencyInjection = NewTech("dependency-injection", 1, aspnet.Id, TechnologyKind.Topic);

        DbContext.Technologies.AddRange(csharp, python, aspnet, efCore, django, dependencyInjection);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetChildCountsAsync(
            [csharp.Id, python.Id, dependencyInjection.Id]);

        Assert.Equal(2, result[csharp.Id]);
        Assert.Equal(1, result[python.Id]);
        Assert.False(result.ContainsKey(dependencyInjection.Id));
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetBySlugPathAsync_ResolvesNestedPath()
    {
        var csharp = NewTech("csharp", 1);
        var aspnet = NewTech("aspnet-core", 1, csharp.Id, TechnologyKind.Framework);

        DbContext.Technologies.AddRange(csharp, aspnet);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetBySlugPathAsync(["csharp", "aspnet-core"]);

        Assert.NotNull(result);
        Assert.Equal(aspnet.Id, result!.Id);
    }

    [Fact]
    public async Task GetBySlugPathAsync_WhenSlugIsUnderDifferentParent_ReturnsNull()
    {
        var csharp = NewTech("csharp", 1);
        var python = NewTech("python", 2);
        var django = NewTech("django", 1, python.Id, TechnologyKind.Framework);

        DbContext.Technologies.AddRange(csharp, python, django);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetBySlugPathAsync(["csharp", "django"]);

        Assert.Null(result);
    }

    [Fact]
    public async Task GetBySlugPathAsync_WhenPathIsEmpty_ReturnsNull()
    {
        var result = await _repository.GetBySlugPathAsync([]);

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_DoesNotPersist_UntilSave()
    {
        var tech = NewTech("csharp", 1);

        await _repository.AddAsync(tech);

        await using (var before = CreateContext())
            Assert.Equal(0, before.Technologies.Count(t => t.Id == tech.Id));

        await SaveChangesAsync();

        await using (var after = CreateContext())
            Assert.Equal(1, after.Technologies.Count(t => t.Id == tech.Id));
    }

    [Fact]
    public async Task GetBySlugPathAsync_ResolvesThreeLevelPath()
    {
        var csharp = NewTech("csharp", 1);
        var aspnet = NewTech("aspnet-core", 1, csharp.Id, TechnologyKind.Framework);
        var dependencyInjection = NewTech("dependency-injection", 1, aspnet.Id, TechnologyKind.Topic);

        DbContext.Technologies.AddRange(csharp, aspnet, dependencyInjection);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetBySlugPathAsync(["csharp", "aspnet-core", "dependency-injection"]);

        Assert.NotNull(result);
        Assert.Equal(dependencyInjection.Id, result!.Id);
    }
}
