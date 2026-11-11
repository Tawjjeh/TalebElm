using TalebElm.Application.DTOs;

namespace TalebElm.Tests.UnitTests;

public class ExamResultResponseTests
{
    [Fact]
    public void ExamResultResponse_PassingResult_StoresAllValues()
    {
        var examId = Guid.NewGuid();
        var result = new ExamResultResponse(examId, true, 85);

        Assert.Equal(examId, result.ExamId);
        Assert.True(result.Passed);
        Assert.Equal(85, result.Score);
    }

    [Fact]
    public void ExamResultResponse_FailingResult_StoresAllValues()
    {
        var examId = Guid.NewGuid();
        var result = new ExamResultResponse(examId, false, 40);

        Assert.Equal(examId, result.ExamId);
        Assert.False(result.Passed);
        Assert.Equal(40, result.Score);
    }

    [Fact]
    public void ExamResultResponse_SameValues_AreEqual()
    {
        var examId = Guid.NewGuid();
        var first = new ExamResultResponse(examId, true, 70);
        var second = new ExamResultResponse(examId, true, 70);

        Assert.Equal(first, second);
    }

    [Fact]
    public void ExamResultResponse_DifferentValues_AreNotEqual()
    {
        var examId = Guid.NewGuid();
        var first = new ExamResultResponse(examId, true, 70);
        var second = new ExamResultResponse(examId, false, 70);

        Assert.NotEqual(first, second);
    }
}