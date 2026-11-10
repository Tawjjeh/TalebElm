using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Application.Services.Concrete;

public class ExamService(IUnitOfWork unitOfWork) : IExamService
{
    public Task<ExamResponse> CreateAsync(CreateExamRequest request)
    {
        throw new NotImplementedException("Creating an exam is not implemented yet.");
    }

    public async Task<ExamResponse> GetByIdAsync(Guid id)
    {
        var exam = await unitOfWork.Exams.GetByIdAsync(id);
        if (exam is null)
            throw new NotFoundException($"Exam with id '{id}' was not found.");


        return new ExamResponse(
            exam.Id,
            exam.Title,
            exam.PassThreshold,
            exam.ModuleId
        );
    }

    public async Task<ExamResultResponse> SubmitAsync(SubmitExamAnswersRequest request)
    {
        if (request.Answers is null)
            throw new ValidationException("Answers are required.");
        var exam = await unitOfWork.Exams.GetByIdAsync(request.ExamId) ?? throw new NotFoundException($"Exam {request.ExamId} was not found.");
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

            var selected = question.Options.FirstOrDefault(o => o.Id == answer.OptionId) ?? throw new ValidationException($"Option {answer.OptionId} does not belong to question {question.Id}.");

            totalPoints += question.Points;
            if (selected.IsCorrect)
                earnedPoints += question.Points;
        }

        if (totalPoints <= 0)
            throw new ValidationException("Exam has no scorable questions.");

        var score = (int)Math.Round(
            100m * earnedPoints / totalPoints,
            MidpointRounding.AwayFromZero);

        return new ExamResultResponse(exam.Id, exam.HasPassed(score), score);
    }
}
