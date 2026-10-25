using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class ResourceEntityTests
{
    [Fact]
    public void NewResource_HasExpectedDefaults()
    {
        var resource = new Resource();

        Assert.NotEqual(Guid.Empty, resource.Id);
        Assert.True(resource.CreatedAt <= DateTimeOffset.UtcNow);
        Assert.Null(resource.TechnologyId);
        Assert.Equal(string.Empty, resource.Title);
        Assert.Null(resource.Description);
        Assert.Equal(default, resource.Kind);
        Assert.Null(resource.Url);
        Assert.Null(resource.Citation);
        Assert.Equal(string.Empty, resource.Language);
        Assert.Equal(default, resource.Level);
        Assert.False(resource.IsPaid);
        Assert.Equal(ResourceStatus.Approved, resource.Status);
        Assert.Equal(0, resource.ConsecutiveFailureCount);
        Assert.Null(resource.LastCheckedAt);
        Assert.Null(resource.CreatedByUserId);
        Assert.Null(resource.Technology);
        Assert.Null(resource.CreatedByUser);
    }

    [Fact]
    public void Resource_CanAssignPropertiesAndLinkRelationships()
    {
        var technologyId = Guid.NewGuid();
        var technology = new Technology
        {
            Id = technologyId,
            Name = "C#",
            Slug = "csharp",
            Kind = TechnologyKind.Language
        };

        var userId = Guid.NewGuid();
        var user = new User
        {
            Id = userId,
            Name = "Admin User",
            Email = "admin@example.com"
        };

        var lastChecked = DateTimeOffset.UtcNow;
        var resource = new Resource
        {
            Title = "C# Documentation",
            Description = "Official Microsoft C# docs",
            Kind = ResourceKind.Documentation,
            Url = "https://learn.microsoft.com/dotnet/csharp/",
            Citation = "Microsoft Learn",
            Language = "en",
            Level = ResourceLevel.Beginner,
            IsPaid = false,
            Status = ResourceStatus.Approved,
            ConsecutiveFailureCount = 0,
            LastCheckedAt = lastChecked,
            TechnologyId = technologyId,
            Technology = technology,
            CreatedByUserId = userId,
            CreatedByUser = user
        };

        Assert.Equal("C# Documentation", resource.Title);
        Assert.Equal("Official Microsoft C# docs", resource.Description);
        Assert.Equal(ResourceKind.Documentation, resource.Kind);
        Assert.Equal("https://learn.microsoft.com/dotnet/csharp/", resource.Url);
        Assert.Equal("Microsoft Learn", resource.Citation);
        Assert.Equal("en", resource.Language);
        Assert.Equal(ResourceLevel.Beginner, resource.Level);
        Assert.False(resource.IsPaid);
        Assert.Equal(ResourceStatus.Approved, resource.Status);
        Assert.Equal(0, resource.ConsecutiveFailureCount);
        Assert.Equal(lastChecked, resource.LastCheckedAt);
        Assert.Equal(technologyId, resource.TechnologyId);
        Assert.Same(technology, resource.Technology);
        Assert.Equal(userId, resource.CreatedByUserId);
        Assert.Same(user, resource.CreatedByUser);
    }
}
