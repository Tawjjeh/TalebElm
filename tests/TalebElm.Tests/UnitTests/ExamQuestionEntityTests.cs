using TalebElm.Domain.Entities;

namespace TalebElm.Tests.UnitTests;

public class ExamQuestionEntityTests
{
    [Fact]
    public void ExamQuestion_DefaultsToEmptyCollectionsAndPrompt()
    {
        var question = new ExamQuestion();
        Assert.NotNull(question.Options);
        Assert.Equal(string.Empty, question.Prompt);
        Assert.Equal(0, question.Points);
    }
}
