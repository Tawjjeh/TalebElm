using TalebElm.Domain.Entities;

namespace TalebElm.Tests.UnitTests;

public class ExamEntityTests
{
    [Fact]
    public void NewExam_HasEmptyTitleAndZeroThreshold()
    {
        var exam = new Exam();

        Assert.Equal(string.Empty, exam.Title);
        Assert.Equal(0, exam.PassThreshold);
        Assert.Equal(Guid.Empty, exam.ModuleId);
    }

    [Fact]
    public void HasPassed_ScoreBelowThreshold_ReturnsFalse()
    {
        var exam = new Exam { PassThreshold = 60 };

        Assert.False(exam.HasPassed(59));
    }

    [Fact]
    public void HasPassed_ScoreEqualToThreshold_ReturnsTrue()
    {
        var exam = new Exam { PassThreshold = 60 };

        Assert.True(exam.HasPassed(60));
    }

    [Fact]
    public void HasPassed_ScoreAboveThreshold_ReturnsTrue()
    {
        var exam = new Exam { PassThreshold = 60 };

        Assert.True(exam.HasPassed(62));
    }
}
