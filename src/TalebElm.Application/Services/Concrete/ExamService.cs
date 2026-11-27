using FluentValidation;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;
using ValidationException = TalebElm.Domain.Exceptions.ValidationException;

namespace TalebElm.Application.Services.Concrete;

public class ExamService(IUnitOfWork unitOfWork, IValidator<CreateExamRequest> validator) : IExamService
{
    public async Task<ExamResponse> CreateAsync(CreateExamRequest request, CancellationToken ct = default)
    {
        var validationResult = await validator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        var module = await unitOfWork.Modules.GetByIdAsync(request.ModuleId, ct)
            ?? throw new NotFoundException($"Module with id '{request.ModuleId}' was not found.");

        var existing = await unitOfWork.Exams.GetByModuleIdAsync(request.ModuleId, ct);
        if (existing is not null)
            throw new ConflictException($"Module '{request.ModuleId}' already has an exam.");

        if (request.Questions is null)
            throw new ValidationException("Questions are required.");

        var exam = new Exam
        {
            Title = request.Title,
            PassThreshold = request.PassThreshold,
            ModuleId = request.ModuleId,
            Questions = request.Questions.Select(question => new ExamQuestion
            {
                Prompt = question.Prompt,
                Order = question.Order,
                Points = question.Points,
                Options = question.Options.Select(option => new ExamOption
                {
                    Text = option.Text,
                    IsCorrect = option.IsCorrect
                }).ToList()
            }).ToList()
        };

        await unitOfWork.Exams.AddAsync(exam, ct);
        await unitOfWork.SaveChangesAsync(ct);

        // Learner-safe: ExamResponse carries no questions and no IsCorrect.
        return new ExamResponse(exam.Id, exam.Title, exam.PassThreshold, exam.ModuleId);
    }

    public async Task<ExamResponse> GetByIdAsync(Guid id, CancellationToken ct = default)
    {
        var exam = await unitOfWork.Exams.GetByIdAsync(id, ct);
        if (exam is null)
            throw new NotFoundException($"Exam with id '{id}' was not found.");


        return new ExamResponse(
            exam.Id,
            exam.Title,
            exam.PassThreshold,
            exam.ModuleId
        );
    }

    public async Task<ExamResultResponse> SubmitAsync(Guid userId, SubmitExamAnswersRequest request, CancellationToken ct = default)
    {
        if (request.Answers is null)
            throw new ValidationException("Answers are required.");
        var exam = await unitOfWork.Exams.GetWithQuestionsAsync(request.ExamId, ct) ?? throw new NotFoundException($"Exam {request.ExamId} was not found.");
        var questionIds = exam.Questions.Select(q => q.Id).ToHashSet();

        // Checks each answer belongs to the exam and not duplicated
        var answerByQuestion = new Dictionary<Guid, SubmittedAnswer>(request.Answers.Count);
        foreach (var answer in request.Answers)
        {
            if (!questionIds.Contains(answer.QuestionId))
                throw new ValidationException($"Question {answer.QuestionId} is not part of this exam.");

            if (!answerByQuestion.TryAdd(answer.QuestionId, answer))
                throw new ValidationException($"Question {answer.QuestionId} was answered more than once.");
        }

        if (answerByQuestion.Count != questionIds.Count)
            throw new ValidationException("Every question in the exam must be answered exactly once.");

        // Validate options and score.
        var earnedPoints = 0;
        var totalPoints = 0;

        foreach (var question in exam.Questions)
        {
            var answer = answerByQuestion[question.Id];

            var selected = question.Options.FirstOrDefault(o => o.Id == answer.OptionId) ??
                throw new ValidationException($"Option {answer.OptionId} does not belong to question {question.Id}.");

            totalPoints += question.Points;
            if (selected.IsCorrect)
                earnedPoints += question.Points;
        }

        var score = totalPoints <= 0 ? 0 : (int)Math.Round(
            100m * earnedPoints / totalPoints,
            MidpointRounding.AwayFromZero);

        // Update progress for this learner and this exam's module.
        var progress = await unitOfWork.UserProgresses.GetByUserAndModuleAsync(userId, exam.ModuleId, ct)
            ?? throw new NotFoundException($"Progress for learner {userId} was not found.");

        var passed = exam.HasPassed(score);

        progress.Score = score;
        progress.PassedExam |= passed;

        // Failed attempt: persist the score only. Never create or unlock the next Module's progress.
        if (!passed)
        {
            await unitOfWork.SaveChangesAsync(ct);
            return new ExamResultResponse(exam.Id, passed, score);
        }

        // Make checks for unlocking the next module
        if (progress.CompletedRequiredProblems)
        {
            // The progress row is loaded without its Module navigation, so read the
            // Module explicitly instead of dereferencing progress.Module.
            var module = await unitOfWork.Modules.GetByIdAsync(exam.ModuleId, ct)
                ?? throw new NotFoundException($"Module {exam.ModuleId} was not found.");

            Module? nextModule = await unitOfWork.Modules
                .GetNextModuleAsync(module.TrackId, module.Order, ct);

            if (nextModule is not null)
            {
                UserProgress? nextProgress = await unitOfWork.UserProgresses
                    .GetByUserAndModuleAsync(userId, nextModule.Id, ct);

                if (nextProgress is null)
                {
                    await unitOfWork.UserProgresses.AddAsync(new UserProgress
                    {
                        UserId = userId,
                        ModuleId = nextModule.Id,
                        IsUnlocked = true
                    }, ct);
                }
                else
                {
                    nextProgress.IsUnlocked = true;
                }
            }
        }

        await unitOfWork.SaveChangesAsync(ct);

        return new ExamResultResponse(exam.Id, passed, score);
    }
}
