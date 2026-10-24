using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class TechnologyEntityTests
{
    [Fact]
    public void NewTechnology_HasExpectedDefaults()
    {
        var technology = new Technology();

        Assert.NotEqual(Guid.Empty, technology.Id);
        Assert.True(technology.CreatedAt <= DateTimeOffset.UtcNow);
        Assert.Equal(string.Empty, technology.Name);
        Assert.Equal(string.Empty, technology.Slug);
        Assert.Null(technology.Description);
        Assert.Equal(default, technology.Kind);
        Assert.Equal(0, technology.Order);
        Assert.Null(technology.ParentId);
        Assert.Null(technology.Parent);
        Assert.NotNull(technology.Children);
        Assert.Empty(technology.Children);
    }

    [Fact]
    public void Technology_CanAssignPropertiesAndLinkTree()
    {
        var parent = new Technology
        {
            Name = "C#",
            Slug = "csharp",
            Kind = TechnologyKind.Language,
            Order = 1,
            Description = "General-purpose programming language"
        };

        var child = new Technology
        {
            Name = "ASP.NET Core",
            Slug = "aspnet-core",
            Kind = TechnologyKind.Framework,
            Order = 1,
            ParentId = parent.Id,
            Parent = parent
        };

        parent.Children.Add(child);

        Assert.Equal("C#", parent.Name);
        Assert.Equal("csharp", parent.Slug);
        Assert.Equal(TechnologyKind.Language, parent.Kind);
        Assert.Equal(1, parent.Order);
        Assert.Equal("General-purpose programming language", parent.Description);

        Assert.Equal("ASP.NET Core", child.Name);
        Assert.Equal("aspnet-core", child.Slug);
        Assert.Equal(TechnologyKind.Framework, child.Kind);
        Assert.Equal(parent.Id, child.ParentId);
        Assert.Same(parent, child.Parent);
        Assert.Single(parent.Children);
        Assert.Contains(child, parent.Children);
    }
}
