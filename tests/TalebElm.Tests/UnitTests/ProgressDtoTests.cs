using TalebElm.Application.DTOs;

namespace TalebElm.Tests.UnitTests;

public class ProgressDtoTests
{
    [Fact]
    public void ProgressResponse_StoresAllValues()
    {
        var moduleId = Guid.NewGuid();
        var response = new ProgressResponse(
            moduleId,
            IsUnlocked: true,
            PassedExam: false,
            Score: 80,
            RequiredProblemsTotal: 5,
            RequiredProblemsCompleted: 3);

        Assert.Equal(moduleId, response.ModuleId);
        Assert.True(response.IsUnlocked);
        Assert.False(response.PassedExam);
        Assert.Equal(80, response.Score);
        Assert.Equal(5, response.RequiredProblemsTotal);
        Assert.Equal(3, response.RequiredProblemsCompleted);
    }

    [Fact]
    public void ProgressResponse_DefaultsRequiredProblemsCountsToZero()
    {
        var moduleId = Guid.NewGuid();
        var response = new ProgressResponse(moduleId, IsUnlocked: true, PassedExam: true, Score: 100);

        Assert.Equal(0, response.RequiredProblemsTotal);
        Assert.Equal(0, response.RequiredProblemsCompleted);
    }
}
