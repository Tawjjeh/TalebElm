using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Concrete;
using TalebElm.Application.Validators;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Repositories;
using TalebElm.Tests.Infrastructure;

namespace TalebElm.Tests.IntegrationTests;

/// <summary>
/// Exercises the Decision 32 unlock against real repositories, so the navigation
/// properties the service reads are loaded the same way they are in production.
/// </summary>
public class ExamProgressionTests : SqliteTestBase
{
    private readonly UnitOfWork _unitOfWork;
    private readonly ExamService _sut;
    private readonly User _learner;
    private readonly Track _track;
    private readonly Module _firstModule;
    private readonly Module _secondModule;
    private readonly Exam _exam;
    private readonly ExamQuestion _question;
    private readonly ExamOption _correctOption;

    public ExamProgressionTests()
    {
        _unitOfWork = new UnitOfWork(DbContext);
        _sut = new ExamService(_unitOfWork, new CreateExamRequestValidator(), new UpdateExamRequestValidator(), new UpdateExamQuestionRequestValidator(), new AddExamOptionRequestValidator());

        _track = new Track { Name = "Backend", Description = "Backend track", Status = Domain.Enums.TrackStatus.Published };
        _firstModule = new Module { Track = _track, Title = "M1", Order = 1 };
        _secondModule = new Module { Track = _track, Title = "M2", Order = 2 };
        _learner = new User { Name = "Learner", Email = "learner@test.com", PasswordHash = "hash", JoinedAt = DateTime.UtcNow };

        _exam = new Exam { Title = "M1 exam", PassThreshold = 50, Module = _firstModule };
        _question = new ExamQuestion { Prompt = "2 + 2?", Order = 1, Points = 10 };
        _correctOption = new ExamOption { Text = "4", IsCorrect = true };
        _question.Options.Add(_correctOption);
        _exam.Questions.Add(_question);

        DbContext.Tracks.Add(_track);
        DbContext.Modules.AddRange(_firstModule, _secondModule);
        DbContext.Users.Add(_learner);
        DbContext.Exams.Add(_exam);
        DbContext.UserProgresses.Add(new UserProgress
        {
            UserId = _learner.Id,
            ModuleId = _firstModule.Id,
            IsUnlocked = true,
            CompletedRequiredProblems = true
        });
        DbContext.SaveChanges();
        ClearTracker();
    }

    private Task<ExamResultResponse> Submit() =>
        _sut.SubmitAsync(
            _learner.Id,
            new SubmitExamAnswersRequest(_exam.Id, [new SubmittedAnswer(_question.Id, _correctOption.Id)]));

    [Fact]
    public async Task Submit_WithBothGatesMet_UnlocksTheNextModule()
    {
        var result = await Submit();

        Assert.True(result.Passed);

        await using var check = CreateContext();
        var next = await Microsoft.EntityFrameworkCore.EntityFrameworkQueryableExtensions
            .FirstOrDefaultAsync(check.UserProgresses, p => p.UserId == _learner.Id && p.ModuleId == _secondModule.Id);

        Assert.NotNull(next);
        Assert.True(next.IsUnlocked);
    }
}
