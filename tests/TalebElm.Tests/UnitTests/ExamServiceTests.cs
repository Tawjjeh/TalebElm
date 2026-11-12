using Moq;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Tests.UnitTests;

public class ExamServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IExamRepository> _exams = new();
    private readonly Mock<IUserProgressRepository> _progresses = new();
    private readonly ExamService _sut;

    private readonly Guid _learnerId = Guid.NewGuid();
    private readonly Exam _exam;
    private readonly UserProgress _progress = new() { Score = 42, PassedExam = false };

    public ExamServiceTests()
    {
        // 3 questions x 10 points, 2 options each (1 correct, 1 wrong), passing score 70.
        _exam = new Exam
        {
            Id = Guid.NewGuid(),
            ModuleId = Guid.NewGuid(),
            PassThreshold = 70,
            Questions = Enumerable.Range(0, 3).Select(_ => new ExamQuestion
            {
                Id = Guid.NewGuid(),
                Points = 10,
                Options = new List<ExamOption>
                {
                    new() { Id = Guid.NewGuid(), IsCorrect = true },
                    new() { Id = Guid.NewGuid(), IsCorrect = false }
                }
            }).ToList()
        };

        _exams.Setup(r => r.GetWithQuestionsAsync(_exam.Id)).ReturnsAsync(_exam);
        _progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _exam.ModuleId)).ReturnsAsync(_progress);
        _unitOfWork.SetupGet(u => u.Exams).Returns(_exams.Object);
        _unitOfWork.SetupGet(u => u.UserProgresses).Returns(_progresses.Object);

        _sut = new ExamService(_unitOfWork.Object);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExamExists_ReturnsExamResponse()
    {
        var exam = new Exam
        {
            Id = Guid.NewGuid(),
            Title = "C# Basics",
            PassThreshold = 60,
            ModuleId = Guid.NewGuid()
        };
        _unitOfWork.Setup(u => u.Exams.GetByIdAsync(exam.Id)).ReturnsAsync(exam);

        var result = await _sut.GetByIdAsync(exam.Id);

        Assert.Equal(exam.Id, result.Id);
        Assert.Equal(exam.Title, result.Title);
        Assert.Equal(exam.PassThreshold, result.PassThreshold);
        Assert.Equal(exam.ModuleId, result.ModuleId);
    }

    [Fact]
    public async Task GetByIdAsync_WhenExamMissing_ThrowsNotFoundException()
    {
        var id = Guid.NewGuid();
        _unitOfWork.Setup(u => u.Exams.GetByIdAsync(id)).ReturnsAsync((Exam?)null);

        var ex = await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(id));

        Assert.Contains(id.ToString(), ex.Message);
    }

    #region SubmtiAsync_Method_Tests


    #region Helper_Methods
    // One answer per question: true = picks the correct option, false = picks the wrong one.
    private List<SubmittedAnswer> Answers(params bool[] correct) =>
        _exam.Questions.Zip(correct, (q, c) => new SubmittedAnswer(q.Id, q.Options.First(o => o.IsCorrect == c).Id)).ToList();

    private Task<ExamResultResponse> Submit(List<SubmittedAnswer>? answers) =>
        _sut.SubmitAsync(_learnerId, new SubmitExamAnswersRequest(_exam.Id, answers!));

    // A rejected submission must throw, save nothing, and leave progress untouched.
    private async Task AssertRejected(List<SubmittedAnswer>? answers)
    {
        await Assert.ThrowsAsync<ValidationException>(() => Submit(answers));
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
        Assert.Equal(42, _progress.Score);
        Assert.False(_progress.PassedExam);
    }
    #endregion

    #region scoring: correct, incorrect, partial

    [Theory]
    [InlineData(new[] { true, true, true }, 100, true)]    // all correct
    [InlineData(new[] { false, false, false }, 0, false)]  // all incorrect
    [InlineData(new[] { true, true, false }, 67, false)]   // partial: 66.67 rounds to 67, below 70
    [InlineData(new[] { true, false, false }, 33, false)]  // partial: 33.33 rounds to 33
    public async Task Submit_ScoresAndSaves(bool[] picks, int expectedScore, bool expectedPassed)
    {
        var result = await Submit(Answers(picks));

        Assert.Equal(_exam.Id, result.ExamId);
        Assert.Equal(expectedScore, result.Score);
        Assert.Equal(expectedPassed, result.Passed);
        Assert.Equal(expectedScore, _progress.Score);
        Assert.Equal(expectedPassed, _progress.PassedExam);
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
    }

    [Fact]
    public async Task Submit_WeightsScoreByQuestionPoints()
    {
        _exam.Questions.First().Points = 30; // total 50
        var result = await Submit(Answers(true, false, false)); // earns 30 of 50

        Assert.Equal(60, result.Score);
    }
    #endregion

    #region score_and_pass_values

    [Theory]
    //        old score, old passed, picks,                       new score, passed stays
    [InlineData(100, true, new[] { false, false, false }, 0, true)]  // fail after pass: score drops, pass kept
    [InlineData(0, false, new[] { true, true, true }, 100, true)]  // pass after fail: pass set
    [InlineData(90, false, new[] { true, true, false }, 67, false)] // lower retry replaces higher, still not passed
    public async Task Submit_LatestScoreWins_PassIsSticky(
        int oldScore, bool oldPassed, bool[] picks, int newScore, bool expectedPassedExam)
    {
        _progress.Score = oldScore;
        _progress.PassedExam = oldPassed;

        await Submit(Answers(picks));

        Assert.Equal(newScore, _progress.Score);              // latest score, never Math.Max
        Assert.Equal(expectedPassedExam, _progress.PassedExam); // needs |= passed
    }

    [Fact]
    public async Task Submit_FailAfterPass_ResponseReportsThisAttemptOnly()
    {
        _progress.PassedExam = true;

        var result = await Submit(Answers(false, false, false));

        Assert.False(result.Passed); // response = this attempt; stored flag stays true
    }
    #endregion

    #region malformed_submissions

    [Fact]
    public Task Submit_NullAnswers_IsRejected() => AssertRejected(null);

    [Fact]
    public Task Submit_UnknownQuestion_IsRejected()
    {
        var answers = Answers(true, true, true);
        answers[0] = new SubmittedAnswer(Guid.NewGuid(), answers[0].OptionId);
        return AssertRejected(answers);
    }

    [Fact]
    public Task Submit_DuplicateAnswer_IsRejected()
    {
        var answers = Answers(true, true, true);
        answers[1] = answers[0]; // question 0 answered twice, question 1 missing
        return AssertRejected(answers);
    }

    [Fact]
    public Task Submit_MissingAnswer_IsRejected() => AssertRejected(Answers(true, true));

    [Fact]
    public Task Submit_OptionFromAnotherQuestion_IsRejected()
    {
        var answers = Answers(true, true, true);
        answers[0] = new SubmittedAnswer(answers[0].QuestionId, _exam.Questions.ElementAt(1).Options.ElementAt(0).Id);
        return AssertRejected(answers);
    }

    [Fact]
    public Task Submit_NonexistentOption_IsRejected()
    {
        var answers = Answers(true, true, true);
        answers[2] = new SubmittedAnswer(answers[2].QuestionId, Guid.NewGuid());
        return AssertRejected(answers);
    }
    #endregion

    #region not_found

    [Fact]
    public async Task Submit_ExamNotFound_Throws()
    {
        _exams.Setup(r => r.GetWithQuestionsAsync(_exam.Id)).ReturnsAsync((Exam?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => Submit(Answers(true, true, true)));
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }

    [Fact]
    public async Task Submit_ProgressNotFound_Throws()
    {
        _progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _exam.ModuleId))
                   .ReturnsAsync((UserProgress?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => Submit(Answers(true, true, true)));
        _unitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
    }
    #endregion


    #endregion
}
