using FluentValidation;
using FluentValidation.Results;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;
using ValidationException = TalebElm.Domain.Exceptions.ValidationException;

namespace TalebElm.Application.Services.Concrete;

public class ExamService(
    IUnitOfWork unitOfWork,
    IValidator<CreateExamRequest> createValidator,
    IValidator<UpdateExamRequest> updateValidator,
    IValidator<UpdateExamQuestionRequest> updateQuestionValidator,
    IValidator<AddExamOptionRequest> addOptionValidator) : IExamService
{
    public async Task<ExamResponse> CreateAsync(CreateExamRequest request, CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(request, ct);
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

    public async Task<ExamResponse> UpdateAsync(Guid id, UpdateExamRequest request, CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        var exam = await unitOfWork.Exams.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Exam with id '{id}' was not found.");

        exam.Title = request.Title;
        exam.PassThreshold = request.PassThreshold;

        // Changing PassThreshold never touches existing UserProgress rows:
        // scores and PassedExam were computed against the threshold in force
        // at submit time and are left as they are.
        await unitOfWork.SaveChangesAsync(ct);

        return new ExamResponse(exam.Id, exam.Title, exam.PassThreshold, exam.ModuleId);
    }

    public async Task<ExamQuestionResponse> UpdateQuestionAsync(Guid examId, Guid questionId, UpdateExamQuestionRequest request, CancellationToken ct = default)
    {
        var validationResult = await updateQuestionValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        var exam = await LoadExamWithQuestionsAsync(examId, ct);
        var question = FindQuestion(exam, questionId);

        if (exam.Questions.Any(q => q.Id != question.Id && q.Order == request.Order))
            throw new FluentValidation.ValidationException(
                [new ValidationFailure("Order", "Exam.Question.Order.Duplicate")]);

        question.Prompt = request.Prompt;
        question.Order = request.Order;
        question.Points = request.Points;

        ValidateQuestionState(question, question.Options);
        await unitOfWork.SaveChangesAsync(ct);

        return ToQuestionResponse(question);
    }

    public async Task<ExamQuestionResponse> AddOptionAsync(Guid examId, Guid questionId, AddExamOptionRequest request, CancellationToken ct = default)
    {
        var validationResult = await addOptionValidator.ValidateAsync(request, ct);
        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        var exam = await LoadExamWithQuestionsAsync(examId, ct);
        var question = FindQuestion(exam, questionId);

        var option = new ExamOption { Text = request.Text, IsCorrect = request.IsCorrect };
        await unitOfWork.Exams.AddOptionAsync(question, option, ct);

        // Adding through the set populates question.Options via fixup, so the
        // new option is already counted here.
        ValidateQuestionState(question, question.Options);
        await unitOfWork.SaveChangesAsync(ct);

        return ToQuestionResponse(question);
    }

    public async Task<ExamQuestionResponse> RemoveOptionAsync(Guid examId, Guid questionId, Guid optionId, CancellationToken ct = default)
    {
        var exam = await LoadExamWithQuestionsAsync(examId, ct);
        var question = FindQuestion(exam, questionId);

        var option = question.Options.FirstOrDefault(o => o.Id == optionId)
            ?? throw new NotFoundException($"Option with id '{optionId}' was not found.");

        await unitOfWork.Exams.RemoveOptionAsync(option, ct);

        // Validate against the post-removal state explicitly: removing through
        // the set only drops the option from the in-memory collection at
        // SaveChanges time, so question.Options still contains it here.
        ValidateQuestionState(question, question.Options.Where(o => o.Id != option.Id).ToList());
        await unitOfWork.SaveChangesAsync(ct);

        return ToQuestionResponse(question);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var exam = await LoadExamWithQuestionsAsync(id, ct);

        if (await unitOfWork.UserProgresses.ExistsForModuleAsync(exam.ModuleId, ct))
            throw new ConflictException(
                $"Exam '{id}' cannot be deleted because learners have progress on its module.");

        await unitOfWork.Exams.RemoveAsync(exam, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }

    private async Task<Exam> LoadExamWithQuestionsAsync(Guid examId, CancellationToken ct)
    {
        return await unitOfWork.Exams.GetWithQuestionsAsync(examId, ct)
            ?? throw new NotFoundException($"Exam with id '{examId}' was not found.");
    }

    private static ExamQuestion FindQuestion(Exam exam, Guid questionId)
    {
        return exam.Questions.FirstOrDefault(q => q.Id == questionId)
            ?? throw new NotFoundException($"Question with id '{questionId}' was not found.");
    }

    /// <summary>
    /// Re-validates Decision 7 on the question's post-write state: Points &gt; 0,
    /// at least 2 options, exactly 1 correct option. Violations throw
    /// <see cref="FluentValidation.ValidationException"/> (400 via the middleware).
    /// The options to check are passed explicitly because EF fixup timing differs
    /// per operation (adds appear in the collection immediately, removals only at
    /// SaveChanges time).
    /// </summary>
    private static void ValidateQuestionState(ExamQuestion question, IEnumerable<ExamOption> options)
    {
        var failures = new List<ValidationFailure>();
        if (question.Points <= 0)
            failures.Add(new ValidationFailure("Points", "Exam.Question.Points.Invalid"));
        if (options.Count() < 2)
            failures.Add(new ValidationFailure("Options", "Exam.Question.Options.TooFew"));
        if (options.Count(o => o.IsCorrect) != 1)
            failures.Add(new ValidationFailure("Options", "Exam.Question.Options.ExactlyOneCorrect"));

        if (failures.Count > 0)
            throw new FluentValidation.ValidationException(failures);
    }

    private static ExamQuestionResponse ToQuestionResponse(ExamQuestion question)
    {
        return new ExamQuestionResponse(
            question.Id,
            question.Prompt,
            question.Order,
            question.Points,
            question.Options.Select(o => new ExamOptionResponse(o.Id, o.Text)).ToList());
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
