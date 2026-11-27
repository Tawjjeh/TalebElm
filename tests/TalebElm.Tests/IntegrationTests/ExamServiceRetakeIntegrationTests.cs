using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Concrete;
using TalebElm.Application.Validators;
using TalebElm.Domain.Entities;
using TalebElm.Infrastructure.Persistence;
using TalebElm.Infrastructure.Repositories;

namespace TalebElm.Tests.IntegrationTests;

/// <summary>
/// Decision 4: unlimited retakes keep the latest Score, while PassedExam and the
/// next-Module unlock are sticky and never reverted by a later failing retake.
/// Runs through the real ExamService + UnitOfWork against SQLite in-memory.
/// </summary>
public sealed class ExamServiceRetakeIntegrationTests : IAsyncLifetime
{
    private readonly Guid _userId = Guid.NewGuid();
    private readonly Guid _trackId = Guid.NewGuid();
    private readonly Guid _module1Id = Guid.NewGuid();
    private readonly Guid _module2Id = Guid.NewGuid();
    private readonly Guid _exam1Id = Guid.NewGuid();
    private readonly Guid _exam2Id = Guid.NewGuid();

    private readonly Dictionary<Guid, List<(Guid QuestionId, Guid CorrectOptionId, Guid WrongOptionId)>> _key = new();

    private SqliteConnection _connection = null!;
    private DbContextOptions<AppDbContext> _options = null!;

    public async Task InitializeAsync()
    {
        // An in-memory SQLite database only lives as long as its connection stays open.
        _connection = new SqliteConnection("DataSource=:memory:");
        await _connection.OpenAsync();

        _options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        await using var ctx = new AppDbContext(_options);
        await ctx.Database.EnsureCreatedAsync();
        await SeedAsync(ctx);
    }

    public async Task DisposeAsync() => await _connection.DisposeAsync();

    [Fact]
    public async Task Submit_PassThenFailingRetake_KeepsPassAndUnlock_ButScoreIsLatest()
    {
        var passResult = await SubmitAsync(_exam1Id, AnswersFor(_exam1Id, correctCount: 2));

        Assert.True(passResult.Passed);
        Assert.Equal(100, passResult.Score);

        await using (var ctx = new AppDbContext(_options))
        {
            var module1Progress = await GetProgressAsync(ctx, _module1Id);
            var module2Progress = await GetProgressAsync(ctx, _module2Id);

            Assert.True(module1Progress.PassedExam);
            Assert.Equal(100, module1Progress.Score);
            Assert.True(module2Progress.IsUnlocked);
        }

        var failResult = await SubmitAsync(_exam1Id, AnswersFor(_exam1Id, correctCount: 0));

        // The response reflects this attempt only.
        Assert.False(failResult.Passed);
        Assert.Equal(0, failResult.Score);

        await using (var ctx = new AppDbContext(_options))
        {
            var module1Progress = await GetProgressAsync(ctx, _module1Id);
            var module2Progress = await GetProgressAsync(ctx, _module2Id);

            Assert.Equal(0, module1Progress.Score);
            Assert.True(module1Progress.PassedExam);
            Assert.True(module2Progress.IsUnlocked);

            var module2Rows = await ctx.UserProgresses
                .CountAsync(p => p.UserId == _userId && p.ModuleId == _module2Id);
            Assert.Equal(1, module2Rows);
        }
    }

    [Fact]
    public async Task Submit_PassOnFinalModule_DoesNotThrow_AndRecordsPass()
    {
        await using (var ctx = new AppDbContext(_options))
        {
            ctx.UserProgresses.Add(new UserProgress
            {
                UserId = _userId,
                ModuleId = _module2Id,
                IsUnlocked = true,
                CompletedRequiredProblems = true
            });
            await ctx.SaveChangesAsync();
        }

        int rowsBefore;
        await using (var ctx = new AppDbContext(_options))
            rowsBefore = await ctx.UserProgresses.CountAsync(p => p.UserId == _userId);

        var result = await SubmitAsync(_exam2Id, AnswersFor(_exam2Id, correctCount: 2));

        Assert.True(result.Passed);
        Assert.Equal(100, result.Score);

        await using var verify = new AppDbContext(_options);
        var finalProgress = await GetProgressAsync(verify, _module2Id);
        Assert.True(finalProgress.PassedExam);

        var rowsAfter = await verify.UserProgresses.CountAsync(p => p.UserId == _userId);
        Assert.Equal(rowsBefore, rowsAfter);
    }

    // Each call uses a fresh DbContext, like a separate request would.
    private async Task<ExamResultResponse> SubmitAsync(Guid examId, List<SubmittedAnswer> answers)
    {
        await using var ctx = new AppDbContext(_options);
        var service = new ExamService(new UnitOfWork(ctx), new CreateExamRequestValidator(), new UpdateExamRequestValidator(), new UpdateExamQuestionRequestValidator(), new AddExamOptionRequestValidator());
        return await service.SubmitAsync(_userId, new SubmitExamAnswersRequest(examId, answers));
    }

    private Task<UserProgress> GetProgressAsync(AppDbContext ctx, Guid moduleId) =>
        ctx.UserProgresses.AsNoTracking()
            .SingleAsync(p => p.UserId == _userId && p.ModuleId == moduleId);

    private List<SubmittedAnswer> AnswersFor(Guid examId, int correctCount) =>
        _key[examId]
            .Select((q, i) => new SubmittedAnswer(
                q.QuestionId,
                i < correctCount ? q.CorrectOptionId : q.WrongOptionId))
            .ToList();

    private async Task SeedAsync(AppDbContext ctx)
    {
        ctx.Users.Add(new User
        {
            Id = _userId,
            Name = "LearnerOne",
            Email = "learner@test.dev",
            PasswordHash = new PasswordHasher().HashPassword("TestPassword123!")
        });

        ctx.Tracks.Add(new Track
        {
            Id = _trackId,
            Name = "Track",
            Description = "Integration test track"
        });

        ctx.Modules.AddRange(
            new Module { Id = _module1Id, TrackId = _trackId, Order = 1, Title = "Module 1" },
            new Module { Id = _module2Id, TrackId = _trackId, Order = 2, Title = "Module 2" });

        ctx.Exams.AddRange(
            BuildExam(_exam1Id, _module1Id),
            BuildExam(_exam2Id, _module2Id));

        ctx.UserProgresses.Add(new UserProgress
        {
            UserId = _userId,
            ModuleId = _module1Id,
            IsUnlocked = true,
            CompletedRequiredProblems = true
        });

        await ctx.SaveChangesAsync();
    }

    private Exam BuildExam(Guid examId, Guid moduleId)
    {
        var keys = new List<(Guid, Guid, Guid)>();
        var questions = new List<ExamQuestion>();

        for (var i = 0; i < 2; i++)
        {
            var questionId = Guid.NewGuid();
            var correctId = Guid.NewGuid();
            var wrongId = Guid.NewGuid();
            keys.Add((questionId, correctId, wrongId));

            questions.Add(new ExamQuestion
            {
                Id = questionId,
                Points = 1,
                Order = i,
                Options =
                {
                    new ExamOption { Id = correctId, IsCorrect = true, Text = "Right" },
                    new ExamOption { Id = wrongId, IsCorrect = false, Text = "Wrong" }
                }
            });
        }

        _key[examId] = keys;

        return new Exam
        {
            Id = examId,
            Title = "Exam",
            ModuleId = moduleId,
            PassThreshold = 70,
            Questions = questions
        };
    }
}
