using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class ResourceHubReadServiceTests : SqliteTestBase
{
    private readonly ResourceHubReadService _sut;

    public ResourceHubReadServiceTests()
        => _sut = new ResourceHubReadService(new UnitOfWork(DbContext));

    private static ResourceFilterParameters EmptyFilters
        => new(null, null, null, null, null);

    private Technology AddTechnology(
        string slug,
        int order,
        Guid? parentId = null,
        TechnologyKind kind = TechnologyKind.Language)
    {
        var technology = new Technology
        {
            Name = slug,
            Slug = slug,
            Order = order,
            ParentId = parentId,
            Kind = kind
        };

        DbContext.Technologies.Add(technology);
        return technology;
    }

    private Resource AddResource(
        string title,
        Guid? technologyId = null,
        ResourceStatus status = ResourceStatus.Approved,
        string language = "en",
        ResourceLevel level = ResourceLevel.Beginner,
        bool isPaid = false,
        ResourceKind kind = ResourceKind.Article)
    {
        var resource = new Resource
        {
            Title = title,
            TechnologyId = technologyId,
            Status = status,
            Language = language,
            Level = level,
            IsPaid = isPaid,
            Kind = kind,
            Url = $"https://example.com/{Guid.NewGuid():N}"
        };

        DbContext.Resources.Add(resource);
        return resource;
    }

    [Fact]
    public async Task GetTechnologiesAsync_WhenNoTechnologiesExist_ReturnsEmpty()
    {
        var result = await _sut.GetTechnologiesAsync(null);

        Assert.Empty(result);
    }

    [Fact]
    public async Task GetTechnologiesAsync_WhenParentIsNull_ReturnsRootsInOrder()
    {
        var python = AddTechnology("python", 2);
        var csharp = AddTechnology("csharp", 1);
        AddTechnology("aspnet-core", 1, csharp.Id, TechnologyKind.Framework);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _sut.GetTechnologiesAsync(null);

        Assert.Equal(2, result.Count);
        Assert.Equal(csharp.Id, result[0].Id);
        Assert.Equal(1, result[0].ChildCount);
        Assert.Equal(0, result[0].ResourceCount);
        Assert.Equal(python.Id, result[1].Id);
        Assert.Equal(0, result[1].ChildCount);
        Assert.Equal(0, result[1].ResourceCount);
    }

    [Fact]
    public async Task GetTechnologiesAsync_ReturnsDirectChildAndApprovedResourceCounts()
    {
        var csharp = AddTechnology("csharp", 1);
        var aspnet = AddTechnology("aspnet-core", 1, csharp.Id, TechnologyKind.Framework);
        var dependencyInjection = AddTechnology("dependency-injection", 1, aspnet.Id, TechnologyKind.Topic);
        AddResource("C# guide", csharp.Id);
        AddResource("Pending C# guide", csharp.Id, ResourceStatus.PendingApproval);
        AddResource("ASP.NET Core guide", aspnet.Id);
        AddResource("DI guide", dependencyInjection.Id);
        AddResource("Rejected DI guide", dependencyInjection.Id, ResourceStatus.Rejected);
        AddResource("Broken DI guide", dependencyInjection.Id, ResourceStatus.Broken);
        await SaveChangesAsync();
        ClearTracker();

        var roots = await _sut.GetTechnologiesAsync(null);

        var root = Assert.Single(roots);
        Assert.Equal(1, root.ChildCount);
        Assert.Equal(1, root.ResourceCount);

        var frameworks = await _sut.GetTechnologiesAsync(csharp.Id);

        var framework = Assert.Single(frameworks);
        Assert.Equal(1, framework.ChildCount);
        Assert.Equal(1, framework.ResourceCount);
    }

    [Fact]
    public async Task GetTechnologiesAsync_WithParentId_ReturnsOnlyDirectChildren()
    {
        var csharp = AddTechnology("csharp", 1);
        var python = AddTechnology("python", 2);
        var aspnet = AddTechnology("aspnet-core", 1, csharp.Id, TechnologyKind.Framework);
        AddTechnology("django", 1, python.Id, TechnologyKind.Framework);
        AddTechnology("dependency-injection", 1, aspnet.Id, TechnologyKind.Topic);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _sut.GetTechnologiesAsync(csharp.Id);

        var child = Assert.Single(result);
        Assert.Equal(aspnet.Id, child.Id);
    }

    [Fact]
    public async Task GetTechnologyBySlugPathAsync_ResolvesNestedPathAndReturnsChildrenAndResources()
    {
        var csharp = AddTechnology("csharp", 1);
        var aspnet = AddTechnology("aspnet-core", 1, csharp.Id, TechnologyKind.Framework);
        var dependencyInjection = AddTechnology("dependency-injection", 1, aspnet.Id, TechnologyKind.Topic);
        AddResource("ASP.NET Core docs", aspnet.Id);
        AddResource("DI guide", dependencyInjection.Id);
        AddResource("Pending DI guide", dependencyInjection.Id, ResourceStatus.PendingApproval);
        await SaveChangesAsync();
        ClearTracker();

        var detail = await _sut.GetTechnologyBySlugPathAsync(
            [" CSharp ", "ASPNET-CORE"], EmptyFilters);

        Assert.Equal(aspnet.Id, detail.Id);
        Assert.Single(detail.Children);
        Assert.Equal(dependencyInjection.Id, detail.Children[0].Id);
        Assert.Equal(0, detail.Children[0].ChildCount);
        Assert.Equal(1, detail.Children[0].ResourceCount);

        var topicDetail = await _sut.GetTechnologyBySlugPathAsync(
            ["csharp", "aspnet-core", "dependency-injection"], EmptyFilters);

        Assert.Equal(dependencyInjection.Id, topicDetail.Id);
        var resource = Assert.Single(topicDetail.Resources.Items);
        Assert.Equal("DI guide", resource.Title);
        Assert.Equal(1, topicDetail.Resources.TotalCount);
    }

    [Fact]
    public async Task GetTechnologyBySlugPathAsync_WhenSlugIsUnderDifferentParent_ThrowsNotFound()
    {
        var csharp = AddTechnology("csharp", 1);
        var python = AddTechnology("python", 2);
        AddTechnology("django", 1, python.Id, TechnologyKind.Framework);
        await SaveChangesAsync();
        ClearTracker();

        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.GetTechnologyBySlugPathAsync(["csharp", "django"], EmptyFilters));
    }

    [Fact]
    public async Task GetTechnologyBySlugPathAsync_WhenPathExceedsThreeSegments_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.GetTechnologyBySlugPathAsync(["a", "b", "c", "d"], EmptyFilters));
    }

    [Fact]
    public async Task GetTechnologyBySlugPathAsync_WhenPathIsEmpty_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.GetTechnologyBySlugPathAsync([], EmptyFilters));
    }

    [Fact]
    public async Task GetTechnologyBySlugPathAsync_WhenPathContainsBlankSegment_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.GetTechnologyBySlugPathAsync(["csharp", "   "], EmptyFilters));
    }

    [Fact]
    public async Task GetResourcesAsync_ReturnsOnlyApprovedResources()
    {
        AddResource("Approved");
        AddResource("Pending", status: ResourceStatus.PendingApproval);
        AddResource("Rejected", status: ResourceStatus.Rejected);
        AddResource("Broken", status: ResourceStatus.Broken);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _sut.GetResourcesAsync(null, EmptyFilters);

        var resource = Assert.Single(result.Items);
        Assert.Equal("Approved", resource.Title);
        Assert.Equal(1, result.TotalCount);

    }

    [Theory]
    [InlineData("csharp")]
    [InlineData("  csharp")]
    [InlineData("csharp  ")]
    [InlineData(" csharp ")]
    public async Task GetResourcesAsync_IgnoresSurroundingWhitespace(string query)
    {
        var match = AddResource("csharp");
        AddResource("SQL basics");
        await SaveChangesAsync();
        ClearTracker();

        var filters = new ResourceFilterParameters(query, null, null, null, null);
        var result = await _sut.GetResourcesAsync(null, filters);

        var resource = Assert.Single(result.Items);
        Assert.Equal(match.Id, resource.Id);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetResourcesAsync_CombinesTopicKeywordLanguageLevelPaidAndKindFilters()
    {
        var technology = AddTechnology("csharp", 1);
        var match = AddResource(
            "Dependency Injection guide",
            technology.Id,
            language: "ar",
            level: ResourceLevel.Advanced,
            isPaid: true,
            kind: ResourceKind.Book);
        AddResource("LINQ basics", technology.Id);
        AddResource(
            "Patterns",
            technology.Id,
            language: "en",
            level: ResourceLevel.Advanced,
            isPaid: true,
            kind: ResourceKind.Book);
        AddResource(
            "Free guide",
            technology.Id,
            language: "ar",
            level: ResourceLevel.Advanced,
            kind: ResourceKind.Book);
        AddResource(
            "Video guide",
            technology.Id,
            language: "ar",
            level: ResourceLevel.Advanced,
            isPaid: true,
            kind: ResourceKind.Video);
        AddResource(
            "Dependency Injection hidden",
            technology.Id,
            ResourceStatus.PendingApproval,
            "ar",
            ResourceLevel.Advanced,
            true,
            ResourceKind.Book);
        await SaveChangesAsync();
        ClearTracker();

        var filters = new ResourceFilterParameters(
            "Injection",
            "ar",
            (int)ResourceLevel.Advanced,
            true,
            (int)ResourceKind.Book);

        var result = await _sut.GetResourcesAsync(technology.Id, filters);

        var resource = Assert.Single(result.Items);
        Assert.Equal(match.Id, resource.Id);
        Assert.Equal(1, result.TotalCount);
    }

    [Fact]
    public async Task GetResourcesAsync_PagesResultsAndReportsTotalPages()
    {
        foreach (var title in new[] { "A", "B", "C", "D", "E" })
            AddResource(title);
        AddResource("Pending", status: ResourceStatus.PendingApproval);
        await SaveChangesAsync();
        ClearTracker();

        var result = await _sut.GetResourcesAsync(
            null,
            new ResourceFilterParameters(null, null, null, null, null, 2, 2));

        Assert.Equal(5, result.TotalCount);
        Assert.Equal(3, result.TotalPages);
        Assert.Equal(2, result.Items.Count);
        Assert.Equal("C", result.Items[0].Title);
        Assert.Equal("D", result.Items[1].Title);
    }

    [Fact]
    public async Task GetResourcesAsync_WhenPageIsPastTheEnd_ReturnsEmptyPage()
    {
        AddResource("A");
        AddResource("B");
        await SaveChangesAsync();
        ClearTracker();

        var result = await _sut.GetResourcesAsync(
            null,
            new ResourceFilterParameters(null, null, null, null, null, 5, 20));

        Assert.Empty(result.Items);
        Assert.Equal(2, result.TotalCount);
        Assert.Equal(1, result.TotalPages);
    }

    [Fact]
    public async Task GetResourcesAsync_WhenTechnologyDoesNotExist_ThrowsNotFound()
    {
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.GetResourcesAsync(Guid.NewGuid(), EmptyFilters));
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GetResourcesAsync_WhenPagingIsInvalid_ThrowsValidationException(
        int pageNumber,
        int pageSize)
    {
        var filters = new ResourceFilterParameters(
            null, null, null, null, null, pageNumber, pageSize);

        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.GetResourcesAsync(null, filters));
    }
    [Theory]
    [InlineData(999, null)]
    [InlineData(-1, null)]
    [InlineData(null, 999)]
    [InlineData(null, -1)]
    public async Task GetResourcesAsync_WhenLevelOrKindIsOutOfRange_ThrowsValidationException(
        int? level,
        int? kind)
    {
        var filters = new ResourceFilterParameters(null, null, level, null, kind);

        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.GetResourcesAsync(null, filters));
    }

    [Fact]
    public async Task GetResourcesAsync_WhenFiltersAreNull_ThrowsValidationException()
    {
        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.GetResourcesAsync(null, null!));
    }
}
