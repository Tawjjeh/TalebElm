using TalebElm.Application.DTOs;

namespace TalebElm.Tests.UnitTests;

public class ProgressDtoTests
{
    [Fact]
    public void ProgressResponse_StoresAllValues()
    {
        var moduleId = Guid.NewGuid();
        var response = new ProgressResponse(moduleId, IsUnlocked: true, PassedExam: false, Score: 80);

        Assert.Equal(moduleId, response.ModuleId);
        Assert.True(response.IsUnlocked);
        Assert.False(response.PassedExam);
        Assert.Equal(80, response.Score);
    }
}
