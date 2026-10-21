using TalebElm.Domain.Entities;

namespace TalebElm.Tests.UnitTests;

public class ModuleEntityTests
{
    [Fact]
    public void NewModule_HasEmptyContentAndNoExam()
    {
        var module = new Module();

        Assert.Equal(string.Empty, module.Title);
        Assert.Equal(string.Empty, module.Summary);
        Assert.Equal(0, module.Order);
        Assert.Empty(module.Lessons);
        Assert.Null(module.Exam);
    }
}
