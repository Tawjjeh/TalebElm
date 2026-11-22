using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Infrastructure.Persistence.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class ResourceRepositoryTests : SqliteTestBase
{
    private readonly ResourceRepository _repository;

    public ResourceRepositoryTests() => _repository = new ResourceRepository(DbContext);

    private static Resource NewResource(
        string title,
        ResourceStatus status = ResourceStatus.Approved,
        string language = "en",
        ResourceLevel level = ResourceLevel.Beginner,
        bool isPaid = false,
        ResourceKind kind = ResourceKind.Article,
        Guid? technologyId = null)
        => new()
        {
            Title = title,
            Url = "https://example.com/" + title.Replace(' ', '-'),
            Language = language,
            Level = level,
            IsPaid = isPaid,
            Kind = kind,
            Status = status,
            TechnologyId = technologyId
        };

    [Fact]
    public async Task GetApprovedPagedAsync_ReturnsOnlyApproved()
    {
        DbContext.Resources.AddRange(
            NewResource("Approved one"),
            NewResource("Pending", ResourceStatus.PendingApproval),
            NewResource("Broken", ResourceStatus.Broken),
            NewResource("Rejected", ResourceStatus.Rejected));
        await SaveChangesAsync();
        ClearTracker();

        var (items, total) = await _repository.GetApprovedPagedAsync(null, null, null, null, null, null, 1, 20);

        Assert.Single(items);
        Assert.Equal("Approved one", items[0].Title);
        Assert.Equal(1, total);
    }

    [Fact]
    public async Task GetApprovedPagedAsync_FiltersByTechnologyLanguageLevelPaidAndKind()
    {
        var tech = new Technology { Name = "C#", Slug = "csharp", Order = 1, Kind = TechnologyKind.Language };
        DbContext.Technologies.Add(tech);
        await SaveChangesAsync();

        var match = NewResource("Match", language: "ar", level: ResourceLevel.Advanced, isPaid: true, kind: ResourceKind.Book, technologyId: tech.Id);
        DbContext.Resources.AddRange(
            match,
            NewResource("Other language", language: "en", level: ResourceLevel.Advanced, isPaid: true, kind: ResourceKind.Book, technologyId: tech.Id),
            NewResource("Other level", language: "ar", level: ResourceLevel.Beginner, isPaid: true, kind: ResourceKind.Book, technologyId: tech.Id),
            NewResource("Free", language: "ar", level: ResourceLevel.Advanced, isPaid: false, kind: ResourceKind.Book, technologyId: tech.Id),
            NewResource("Other kind", language: "ar", level: ResourceLevel.Advanced, isPaid: true, kind: ResourceKind.Video, technologyId: tech.Id),
            NewResource("No technology", language: "ar", level: ResourceLevel.Advanced, isPaid: true, kind: ResourceKind.Book));
        await SaveChangesAsync();
        ClearTracker();

        var (items, total) = await _repository.GetApprovedPagedAsync(
            tech.Id, null, "ar", ResourceLevel.Advanced, true, ResourceKind.Book, 1, 20);

        Assert.Single(items);
        Assert.Equal(match.Id, items[0].Id);
        Assert.Equal(1, total);
    }

    [Fact]
    public async Task GetApprovedPagedAsync_FiltersByTitleKeyword()
    {
        DbContext.Resources.AddRange(NewResource("Dependency Injection guide"), NewResource("LINQ basics"));
        await SaveChangesAsync();
        ClearTracker();

        var (items, total) = await _repository.GetApprovedPagedAsync(null, "Injection", null, null, null, null, 1, 20);

        Assert.Single(items);
        Assert.Equal("Dependency Injection guide", items[0].Title);
        Assert.Equal(1, total);
    }

    [Fact]
    public async Task GetApprovedPagedAsync_PagesResults_AndReportsTotalCount()
    {
        DbContext.Resources.AddRange(
            NewResource("A"), NewResource("B"), NewResource("C"), NewResource("D"), NewResource("E"));
        await SaveChangesAsync();
        ClearTracker();

        var (items, total) = await _repository.GetApprovedPagedAsync(null, null, null, null, null, null, 2, 2);

        Assert.Equal(5, total);
        Assert.Equal(2, items.Count);
        Assert.Equal("C", items[0].Title);
        Assert.Equal("D", items[1].Title);
    }

    [Fact]
    public async Task GetApprovedPagedAsync_WhenPageOverflows_ReturnsEmptyPage()
    {
        DbContext.Resources.AddRange(NewResource("A"), NewResource("B"));
        await SaveChangesAsync();
        ClearTracker();

        var (items, total) = await _repository.GetApprovedPagedAsync(
            null, null, null, null, null, null, int.MaxValue, 20);

        Assert.Empty(items);
        Assert.Equal(2, total);
    }

    [Fact]
    public async Task GetApprovedPagedAsync_WhenPageIsPastTheEnd_ReturnsEmptyPage()
    {
        DbContext.Resources.AddRange(NewResource("A"), NewResource("B"));
        await SaveChangesAsync();
        ClearTracker();

        var (items, total) = await _repository.GetApprovedPagedAsync(null, null, null, null, null, null, 5, 20);

        Assert.Empty(items);
        Assert.Equal(2, total);
    }

    [Fact]
    public async Task GetApprovedResourceCountsAsync_WhenTechnologyIdsAreEmpty_ReturnsEmpty()
    {
        var result = await _repository.GetApprovedResourceCountsAsync([]);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetApprovedResourceCountsAsync_ReturnsOnlyApprovedDirectCounts()
    {
        var csharp = new Technology { Name = "C#", Slug = "csharp", Order = 1, Kind = TechnologyKind.Language };
        var python = new Technology { Name = "Python", Slug = "python", Order = 2, Kind = TechnologyKind.Language };
        DbContext.Technologies.AddRange(csharp, python);
        await SaveChangesAsync();

        DbContext.Resources.AddRange(
            NewResource("C# one", technologyId: csharp.Id),
            NewResource("C# two", technologyId: csharp.Id),
            NewResource("C# pending", ResourceStatus.PendingApproval, technologyId: csharp.Id),
            NewResource("C# rejected", ResourceStatus.Rejected, technologyId: csharp.Id),
            NewResource("C# broken", ResourceStatus.Broken, technologyId: csharp.Id),
            NewResource("Python one", technologyId: python.Id),
            NewResource("No technology"));
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetApprovedResourceCountsAsync(
            [csharp.Id, python.Id, Guid.NewGuid()]);

        Assert.Equal(2, result[csharp.Id]);
        Assert.Equal(1, result[python.Id]);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task GetForHealthCheckAsync_ReturnsOnlyApproved()
    {
        DbContext.Resources.AddRange(
            NewResource("Approved"),
            NewResource("Broken", ResourceStatus.Broken),
            NewResource("Pending", ResourceStatus.PendingApproval));
        await SaveChangesAsync();
        ClearTracker();

        var result = await _repository.GetForHealthCheckAsync();

        Assert.Single(result);
        Assert.Equal("Approved", result[0].Title);
    }

    [Fact]
    public async Task GetByIdAsync_WhenResourceIsMissing_ReturnsNull()
    {
        var result = await _repository.GetByIdAsync(Guid.NewGuid());

        Assert.Null(result);
    }

    [Fact]
    public async Task AddAsync_DoesNotPersist_UntilSave()
    {
        var resource = NewResource("Docs");

        await _repository.AddAsync(resource);

        await using (var before = CreateContext())
            Assert.Equal(0, before.Resources.Count(r => r.Id == resource.Id));

        await SaveChangesAsync();

        await using (var after = CreateContext())
            Assert.Equal(1, after.Resources.Count(r => r.Id == resource.Id));
    }
}
