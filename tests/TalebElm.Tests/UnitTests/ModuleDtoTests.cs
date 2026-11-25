using TalebElm.Application.DTOs;

namespace TalebElm.Tests.UnitTests;

public class ModuleDtoTests
{
    static readonly Guid ModuleId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    static readonly Guid TrackId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    [Fact]
    public void ModuleResponse_WhenCreated_InitializesAllProperties()
    {
        var mr = new ModuleResponse(ModuleId, "New to C#", "Basic of the language", 3, TrackId);

        Assert.Equal(ModuleId, mr.Id);
        Assert.Equal("New to C#", mr.Title);
        Assert.Equal("Basic of the language", mr.Summary);
        Assert.Equal(3, mr.Order);
        Assert.Equal(TrackId, mr.TrackId);
    }

    [Fact]
    public void ModuleResponse_WithSameValues_AreEqual()
    {
        var mr1 = new ModuleResponse(ModuleId, "New to C#", "Basic of the language", 3, TrackId);
        var mr2 = new ModuleResponse(ModuleId, "New to C#", "Basic of the language", 3, TrackId);

        Assert.Equal(mr1, mr2);
        Assert.True(mr1 == mr2);
    }

    [Fact]
    public void ModuleResponse_WithDifferentOrder_AreNotEqual()
    {
        var original = new ModuleResponse(ModuleId, "New to C#", "Basic of the language", 3, TrackId);
        ModuleResponse copy = original with { Order = 2 };

        Assert.NotEqual(original, copy);
        Assert.True(original != copy);
    }

    [Fact]
    public void ModuleResponse_WithOrderOne_KeepsOrderUnchanged()
    {
        var mr = new ModuleResponse(ModuleId, "New to C#", "Basic of the language", 1, TrackId);

        Assert.Equal(1, mr.Order);
    }
}
