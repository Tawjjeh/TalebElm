using TalebElm.Application.DTOs;

namespace TalebElm.Tests.UnitTests;

public class ExamDtoTests
{
    [Fact]
    public void CreateExamRequest_StoresAllValues()
    {
        var moduleId = Guid.NewGuid();
        var questions = new List<CreateExamQuestionRequest>
        {
            new("What is 2 + 2?", 1, 10, new List<CreateExamOptionRequest>
            {
                new("4", IsCorrect: true),
                new("5", IsCorrect: false)
            })
        };
        var request = new CreateExamRequest(moduleId, "Final Exam", 60, questions);

        Assert.Equal(moduleId, request.ModuleId);
        Assert.Equal("Final Exam", request.Title);
        Assert.Equal(60, request.PassThreshold);
        Assert.Single(request.Questions);
        Assert.Equal("What is 2 + 2?", request.Questions[0].Prompt);
        Assert.Equal(2, request.Questions[0].Options.Count);
        Assert.True(request.Questions[0].Options[0].IsCorrect);
    }
}
