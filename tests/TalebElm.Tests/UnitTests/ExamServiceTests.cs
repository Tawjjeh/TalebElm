using Moq;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Repositories;

namespace TalebElm.Tests.UnitTests;

public class ExamServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IExamRepository> _exams = new();
    private readonly Mock<IUserProgressRepository> _progresses = new();
    private readonly Mock<IModuleRepository> _modules = new();
    private readonly ExamService _sut;

    private readonly Guid _learnerId = Guid.NewGuid();
    private readonly Exam _exam;
    private readonly Module _module;
    private readonly Module _nextModule;
    private readonly UserProgress _progress = new() { Score = 42, PassedExam = false, IsUnlocked = true };

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

        var trackId = Guid.NewGuid();
        _module = new Module { Id = _exam.ModuleId, TrackId = trackId, Order = 1 };
        _nextModule = new Module { Id = Guid.NewGuid(), TrackId = trackId, Order = 2 };
        _progress.UserId = _learnerId;
        _progress.ModuleId = _module.Id;

        _modules.Setup(r => r.GetByIdAsync(_module.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_module);
        _exams.Setup(r => r.GetWithQuestionsAsync(_exam.Id, It.IsAny<CancellationToken>())).ReturnsAsync(_exam);
        _progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _exam.ModuleId, It.IsAny<CancellationToken>())).ReturnsAsync(_progress);
        _unitOfWork.SetupGet(u => u.Exams).Returns(_exams.Object);
        _unitOfWork.SetupGet(u => u.UserProgresses).Returns(_progresses.Object);
        _unitOfWork.SetupGet(u => u.Modules).Returns(_modules.Object);

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

    [Fact]
    public async Task GetByIdAsync_ForwardsCancellationTokenToRepository()
    {
        using var cts = new CancellationTokenSource();
        var examId = Guid.NewGuid();
        var exam = new Exam { Id = examId, Title = "Midterm", PassThreshold = 70, ModuleId = Guid.NewGuid() };

        _exams
            .Setup(r => r.GetByIdAsync(examId, cts.Token))
            .ReturnsAsync(exam);

        var result = await _sut.GetByIdAsync(examId, cts.Token);

        Assert.Equal(examId, result.Id);
        _exams.Verify(r => r.GetByIdAsync(examId, cts.Token), Times.Once);
    }

    #region GetDetailForLearnerAsync_Method_Tests

    // Questions stored out of order; the detail view must order them by Order.
    private Exam DetailExam() => new()
    {
        Id = Guid.NewGuid(),
        Title = "Midterm",
        PassThreshold = 70,
        ModuleId = _exam.ModuleId,
        Questions = new List<ExamQuestion>
        {
            new()
            {
                Id = Guid.NewGuid(), Prompt = "Second", Order = 2, Points = 20,
                Options = new List<ExamOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "B1", IsCorrect = false },
                    new() { Id = Guid.NewGuid(), Text = "B2", IsCorrect = true }
                }
            },
            new()
            {
                Id = Guid.NewGuid(), Prompt = "First", Order = 1, Points = 10,
                Options = new List<ExamOption>
                {
                    new() { Id = Guid.NewGuid(), Text = "A1", IsCorrect = true },
                    new() { Id = Guid.NewGuid(), Text = "A2", IsCorrect = false }
                }
            }
        }
    };

    [Fact]
    public async Task GetDetailForLearnerAsync_WhenUnlocked_ReturnsOrderedQuestionsWithoutAnswerKey()
    {
        var detailExam = DetailExam();
        _exams.Setup(r => r.GetWithQuestionsAsync(detailExam.Id, It.IsAny<CancellationToken>())).ReturnsAsync(detailExam);

        var result = await _sut.GetDetailForLearnerAsync(_learnerId, detailExam.Id);

        Assert.Equal(detailExam.Id, result.Id);
        Assert.Equal("Midterm", result.Title);
        Assert.Equal(70, result.PassThreshold);
        Assert.Equal(_exam.ModuleId, result.ModuleId);
        Assert.Equal(new[] { "First", "Second" }, result.Questions.Select(q => q.Prompt));
        Assert.Equal(new[] { 10, 20 }, result.Questions.Select(q => q.Points));
        Assert.Equal(new[] { "A1", "A2" }, result.Questions[0].Options.Select(o => o.Text));
        // The learner DTOs have no IsCorrect member anywhere in their shape.
        Assert.DoesNotContain(typeof(ExamDetailResponse).GetProperties(), p => p.Name == "IsCorrect");
        Assert.DoesNotContain(typeof(ExamQuestionResponse).GetProperties(), p => p.Name == "IsCorrect");
        Assert.DoesNotContain(typeof(ExamOptionResponse).GetProperties(), p => p.Name == "IsCorrect");
    }

    [Fact]
    public async Task GetDetailForLearnerAsync_WhenModuleLocked_ThrowsForbiddenException()
    {
        var detailExam = DetailExam();
        _exams.Setup(r => r.GetWithQuestionsAsync(detailExam.Id, It.IsAny<CancellationToken>())).ReturnsAsync(detailExam);
        _progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _exam.ModuleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProgress { UserId = _learnerId, ModuleId = _exam.ModuleId, IsUnlocked = false });

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.GetDetailForLearnerAsync(_learnerId, detailExam.Id));
    }

    [Fact]
    public async Task GetDetailForLearnerAsync_WhenProgressMissing_ThrowsForbiddenException()
    {
        var detailExam = DetailExam();
        _exams.Setup(r => r.GetWithQuestionsAsync(detailExam.Id, It.IsAny<CancellationToken>())).ReturnsAsync(detailExam);
        _progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _exam.ModuleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((UserProgress?)null);

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.GetDetailForLearnerAsync(_learnerId, detailExam.Id));
    }

    [Fact]
    public async Task GetDetailForLearnerAsync_WhenExamMissing_ThrowsNotFoundException()
    {
        var examId = Guid.NewGuid();
        _exams.Setup(r => r.GetWithQuestionsAsync(examId, It.IsAny<CancellationToken>())).ReturnsAsync((Exam?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetDetailForLearnerAsync(_learnerId, examId));
    }

    [Fact]
    public async Task GetDetailForLearnerAsync_ForwardsCancellationToken()
    {
        using var cts = new CancellationTokenSource();
        var detailExam = DetailExam();
        _exams.Setup(r => r.GetWithQuestionsAsync(detailExam.Id, cts.Token)).ReturnsAsync(detailExam);

        var result = await _sut.GetDetailForLearnerAsync(_learnerId, detailExam.Id, cts.Token);

        Assert.Equal(detailExam.Id, result.Id);
        _exams.Verify(r => r.GetWithQuestionsAsync(detailExam.Id, cts.Token), Times.Once);
        _progresses.Verify(r => r.GetByUserAndModuleAsync(_learnerId, _exam.ModuleId, cts.Token), Times.Once);
    }

    [Fact]
    public async Task GetDetailByModuleForLearnerAsync_WhenUnlocked_ReturnsModuleExam()
    {
        var detailExam = DetailExam();
        _exams.Setup(r => r.GetByModuleIdAsync(_exam.ModuleId, It.IsAny<CancellationToken>())).ReturnsAsync(detailExam);
        _exams.Setup(r => r.GetWithQuestionsAsync(detailExam.Id, It.IsAny<CancellationToken>())).ReturnsAsync(detailExam);

        var result = await _sut.GetDetailByModuleForLearnerAsync(_learnerId, _exam.ModuleId);

        Assert.Equal(detailExam.Id, result.Id);
        Assert.Equal(new[] { "First", "Second" }, result.Questions.Select(q => q.Prompt));
    }

    [Fact]
    public async Task GetDetailByModuleForLearnerAsync_WhenModuleHasNoExam_ThrowsNotFoundException()
    {
        var moduleId = Guid.NewGuid();
        _exams.Setup(r => r.GetByModuleIdAsync(moduleId, It.IsAny<CancellationToken>())).ReturnsAsync((Exam?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetDetailByModuleForLearnerAsync(_learnerId, moduleId));
    }

    [Fact]
    public async Task GetDetailByModuleForLearnerAsync_WhenModuleLocked_ThrowsForbiddenException()
    {
        var detailExam = DetailExam();
        _exams.Setup(r => r.GetByModuleIdAsync(_exam.ModuleId, It.IsAny<CancellationToken>())).ReturnsAsync(detailExam);
        _progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _exam.ModuleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProgress { UserId = _learnerId, ModuleId = _exam.ModuleId, IsUnlocked = false });

        await Assert.ThrowsAsync<ForbiddenException>(() => _sut.GetDetailByModuleForLearnerAsync(_learnerId, _exam.ModuleId));
    }

    [Fact]
    public async Task Submit_WhenModuleLocked_ThrowsForbiddenExceptionAndSavesNothing()
    {
        _progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _exam.ModuleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(new UserProgress { UserId = _learnerId, ModuleId = _exam.ModuleId, IsUnlocked = false });

        await Assert.ThrowsAsync<ForbiddenException>(() => Submit(Answers(true, true, true)));
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
        Assert.Equal(42, _progress.Score);
    }
    #endregion

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
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
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
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
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
        _exams.Setup(r => r.GetWithQuestionsAsync(_exam.Id, It.IsAny<CancellationToken>())).ReturnsAsync((Exam?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => Submit(Answers(true, true, true)));
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Submit_ProgressNotFound_Throws()
    {
        _progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _exam.ModuleId, It.IsAny<CancellationToken>()))
                   .ReturnsAsync((UserProgress?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => Submit(Answers(true, true, true)));
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }
    #endregion

    #region Unlock_next_module

    #region Helper_Methods
    // The next module exists, and the learner's row for it is `existingRow` (null = no row yet).
    private void ArrangeNextModule(UserProgress? existingRow)
    {
        _modules.Setup(r => r.GetNextModuleAsync(_module.TrackId, _module.Order, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_nextModule);
        _progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _nextModule.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(existingRow);
    }

    private UserProgress NewLockedRow(Guid userId) =>
        new() { UserId = userId, ModuleId = _nextModule.Id, IsUnlocked = false };

    private void ArrangeRequiredProblemsDone() => _progress.CompletedRequiredProblems = true;
    #endregion

    [Fact]
    public async Task Submit_PassAndRequiredProblemsDone_UnlocksNextModule()
    {
        ArrangeRequiredProblemsDone();
        var next = NewLockedRow(_learnerId);
        ArrangeNextModule(next);

        await Submit(Answers(true, true, true));

        Assert.True(next.IsUnlocked);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // score + unlock in ONE save
    }

    [Fact]
    public async Task Submit_PassButRequiredProblemsNotDone_DoesNotUnlock()
    {
        // CompletedRequiredProblems stays false
        var next = NewLockedRow(_learnerId);
        ArrangeNextModule(next);

        await Submit(Answers(true, true, true));

        Assert.False(next.IsUnlocked);
        _modules.Verify(r => r.GetNextModuleAsync(It.IsAny<Guid>(), It.IsAny<int>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Submit_FailWithRequiredProblemsDone_DoesNotUnlock()
    {
        ArrangeRequiredProblemsDone();
        var next = NewLockedRow(_learnerId);
        ArrangeNextModule(next);

        await Submit(Answers(false, false, false));

        Assert.False(next.IsUnlocked);
    }

    [Fact]
    public async Task Submit_PassAndNextProgressRowMissing_CreatesUnlockedRow()
    {
        ArrangeRequiredProblemsDone();
        ArrangeNextModule(existingRow: null);

        await Submit(Answers(true, true, true));

        _progresses.Verify(r => r.AddAsync(
            It.Is<UserProgress>(p => p.UserId == _learnerId && p.ModuleId == _nextModule.Id && p.IsUnlocked),
            It.IsAny<CancellationToken>()), Times.Once); // check the real "add" method name
    }

    [Fact]
    public async Task Submit_PassTwice_DoesNotCreateDuplicateRows()
    {
        ArrangeRequiredProblemsDone();
        var rows = new List<UserProgress>(); // fake table: starts empty, AddAsync fills it
        _modules.Setup(r => r.GetNextModuleAsync(_module.TrackId, _module.Order, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_nextModule);
        _progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _nextModule.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(() => rows.FirstOrDefault());
        _progresses.Setup(r => r.AddAsync(It.IsAny<UserProgress>(), It.IsAny<CancellationToken>()))
                   .Callback<UserProgress, CancellationToken>((p, _) => rows.Add(p));

        await Submit(Answers(true, true, true));
        await Submit(Answers(true, true, true));

        var row = Assert.Single(rows);   // exactly one row after two passes
        Assert.True(row.IsUnlocked);
    }

    [Fact]
    public async Task Submit_PassOnFinalModule_DoesNotThrowAndCreatesNothing()
    {
        ArrangeRequiredProblemsDone();
        _modules.Setup(r => r.GetNextModuleAsync(_module.TrackId, _module.Order, It.IsAny<CancellationToken>()))
                .ReturnsAsync((Module?)null); // no successor

        var result = await Submit(Answers(true, true, true));

        Assert.True(result.Passed);
        _progresses.Verify(r => r.AddAsync(It.IsAny<UserProgress>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // score still saved
    }

    [Fact]
    public async Task Submit_PassByOneLearner_DoesNotTouchOtherLearners()
    {
        ArrangeRequiredProblemsDone();
        var next = NewLockedRow(_learnerId);
        ArrangeNextModule(next);
        var other = NewLockedRow(Guid.NewGuid()); // another learner's row for the same next module

        await Submit(Answers(true, true, true));

        Assert.True(next.IsUnlocked);
        Assert.False(other.IsUnlocked);
        // The service only ever asks for this learner's rows
        _progresses.Verify(r => r.GetByUserAndModuleAsync(
            It.Is<Guid>(id => id != _learnerId), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Submit_ForwardsCancellationToken()
    {
        ArrangeRequiredProblemsDone();
        ArrangeNextModule(NewLockedRow(_learnerId));
        using var cts = new CancellationTokenSource();
        var request = new SubmitExamAnswersRequest(_exam.Id, Answers(true, true, true));

        await _sut.SubmitAsync(_learnerId, request, cts.Token);

        _exams.Verify(r => r.GetWithQuestionsAsync(_exam.Id, cts.Token), Times.Once);
        _modules.Verify(r => r.GetNextModuleAsync(_module.TrackId, _module.Order, cts.Token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task Submit_FailWithRequiredProblemsDone_CreatesNoNextProgressRow()
    {
        ArrangeRequiredProblemsDone();
        ArrangeNextModule(existingRow: null);

        var result = await Submit(Answers(false, false, false));

        Assert.False(result.Passed);
        _progresses.Verify(r => r.AddAsync(It.IsAny<UserProgress>(), It.IsAny<CancellationToken>()), Times.Never);
        _unitOfWork.Verify(u => u.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once); // score still saved
    }

    [Fact]
    public async Task Submit_FailThenPassRetry_UnlocksNextModule()
    {
        ArrangeRequiredProblemsDone();
        var rows = new List<UserProgress>(); // fake table: starts empty, AddAsync fills it
        _modules.Setup(r => r.GetNextModuleAsync(_module.TrackId, _module.Order, It.IsAny<CancellationToken>()))
                .ReturnsAsync(_nextModule);
        _progresses.Setup(r => r.GetByUserAndModuleAsync(_learnerId, _nextModule.Id, It.IsAny<CancellationToken>()))
                   .ReturnsAsync(() => rows.FirstOrDefault());
        _progresses.Setup(r => r.AddAsync(It.IsAny<UserProgress>(), It.IsAny<CancellationToken>()))
                   .Callback<UserProgress, CancellationToken>((p, _) => rows.Add(p));

        var failed = await Submit(Answers(false, false, false));

        Assert.False(failed.Passed);
        Assert.Empty(rows);                 // nothing created or unlocked by the failure

        var passed = await Submit(Answers(true, true, true));

        Assert.True(passed.Passed);
        var row = Assert.Single(rows);      // the retry creates exactly one unlocked row
        Assert.True(row.IsUnlocked);
        Assert.True(_progress.PassedExam);
    }

    [Fact]
    public async Task Submit_FailByOneLearner_DoesNotTouchOtherLearners()
    {
        ArrangeRequiredProblemsDone();
        var next = NewLockedRow(_learnerId);
        ArrangeNextModule(next);
        var other = NewLockedRow(Guid.NewGuid());

        await Submit(Answers(false, false, false));

        Assert.False(next.IsUnlocked);
        Assert.False(other.IsUnlocked);
        _progresses.Verify(r => r.GetByUserAndModuleAsync(
            It.Is<Guid>(id => id != _learnerId), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
        _progresses.Verify(r => r.AddAsync(It.IsAny<UserProgress>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    // The reviewer's retake gap. Learner already passed, then fails a retake.
    [Fact]
    public async Task Submit_FailRetryAfterPass_KeepsPassAndCreatesNoNextRow()
    {
        _progress.PassedExam = true;
        ArrangeRequiredProblemsDone();
        ArrangeNextModule(existingRow: null);

        var result = await Submit(Answers(false, false, false));

        Assert.False(result.Passed);
        Assert.Equal(0, _progress.Score);
        Assert.True(_progress.PassedExam);   // sticky pass, Decision 4
        _progresses.Verify(r => r.AddAsync(It.IsAny<UserProgress>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Submit_FailRetryAfterPass_KeepsLockedNextModuleLocked()
    {
        _progress.PassedExam = true;
        ArrangeRequiredProblemsDone();
        var next = NewLockedRow(_learnerId);
        ArrangeNextModule(next);

        await Submit(Answers(false, false, false));

        Assert.False(next.IsUnlocked);
    }
    #endregion

    #endregion
}
