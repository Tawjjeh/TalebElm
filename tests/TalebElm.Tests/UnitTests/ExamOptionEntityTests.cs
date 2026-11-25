using TalebElm.Domain.Entities;

namespace TalebElm.Tests.UnitTests;

public class ExamOptionEntityTests
{
    [Fact]
    public void ExamOption_DefaultsToEmptyTextAndIncorrect()
    {
        var option = new ExamOption();

        Assert.Equal(string.Empty, option.Text);
        Assert.False(option.IsCorrect);
    }

    [Fact]
    public void ExamOption_PreservesAssignedValues()
    {
        var questionId = Guid.NewGuid();

        var option = new ExamOption
        {
            ExamQuestionId = questionId,
            Text = "Paris",
            IsCorrect = true
        };

        Assert.Equal(questionId, option.ExamQuestionId);
        Assert.Equal("Paris", option.Text);
        Assert.True(option.IsCorrect);
    }

    [Fact]
    public void ExamOption_IsCorrectIsIndependentForOptionsOnTheSameQuestion()
    {
        var questionId = Guid.NewGuid();
        var first = new ExamOption { ExamQuestionId = questionId, Text = "Paris", IsCorrect = true };
        var second = new ExamOption { ExamQuestionId = questionId, Text = "Rome", IsCorrect = false };

        Assert.Equal(first.ExamQuestionId, second.ExamQuestionId);
        Assert.True(first.IsCorrect);
        Assert.False(second.IsCorrect);

        first.IsCorrect = false;
        second.IsCorrect = true;

        Assert.False(first.IsCorrect);
        Assert.True(second.IsCorrect);
    }

    [Fact]
    public void ExamOption_ExamQuestionIsNullUntilSet()
    {
        var option = new ExamOption();

        Assert.Null(option.ExamQuestion);
    }
}