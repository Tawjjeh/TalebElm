using TalebElm.Application.DTOs;

namespace TalebElm.Tests.UnitTests;

public class ModuleDtoTests
{
    [Fact]
    public void ModuleResponse_StoresAllValues()
    {
        var id = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var response = new ModuleResponse(id, "Introduction to C#", "Covers the basics of C#.", 1, trackId);

        Assert.Equal(id, response.Id);
        Assert.Equal("Introduction to C#", response.Title);
        Assert.Equal("Covers the basics of C#.", response.Summary);
        Assert.Equal(1, response.Order);
        Assert.Equal(trackId, response.TrackId);
    }

    [Fact]
    public void ModuleResponse_ComparesEqualWhenAllValuesMatch()
    {
        var id = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var first = new ModuleResponse(id, "Introduction to C#", "Covers the basics of C#.", 1, trackId);
        var second = new ModuleResponse(id, "Introduction to C#", "Covers the basics of C#.", 1, trackId);

        Assert.Equal(first, second);
        Assert.True(first == second);
    }

    [Fact]
    public void ModuleResponse_DiffersWhenOrderChanges()
    {
        var id = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var first = new ModuleResponse(id, "Introduction to C#", "Covers the basics of C#.", 1, trackId);
        var second = new ModuleResponse(id, "Introduction to C#", "Covers the basics of C#.", 2, trackId);

        Assert.NotEqual(first, second);
        Assert.True(first != second);
    }

    [Fact]
    public void ModuleResponse_KeepsOrderAsPassed()
    {
        var response = new ModuleResponse(Guid.NewGuid(), "Introduction to C#", "Covers the basics of C#.", 1, Guid.NewGuid());

        Assert.Equal(1, response.Order);
    }
}
