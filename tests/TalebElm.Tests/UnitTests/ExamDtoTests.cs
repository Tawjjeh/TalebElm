using TalebElm.Application.DTOs;

namespace TalebElm.Tests.UnitTests;

public class ExamDtoTests
{
    [Fact]
    public void CreateExamRequest_StoresAllValues()
    {
        var moduleId = Guid.NewGuid();
        var request = new CreateExamRequest(moduleId, "Final Exam", 60);

        Assert.Equal(moduleId, request.ModuleId);
        Assert.Equal("Final Exam", request.Title);
        Assert.Equal(60, request.PassThreshold);
    }
}
