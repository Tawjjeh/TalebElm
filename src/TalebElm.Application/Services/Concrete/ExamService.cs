using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public class ExamService(IUnitOfWork unitOfWork) : IExamService
{
    public Task<ExamResponse> CreateAsync(CreateExamRequest request)
    {
        throw new TalebElm.Domain.Exceptions.NotImplementedException("Creating an exam is not implemented yet.");
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

    public async Task<ExamResultResponse> SubmitAsync(Guid userId, SubmitExamRequest request)
    {
        var exam = await unitOfWork.Exams.GetByIdAsync(request.ExamId);
        if (exam is null)
            throw new NotFoundException($"Exam with id '{request.ExamId}' was not found.");

        var currentModule = await unitOfWork.Modules.GetByIdAsync(exam.ModuleId);
        if (currentModule is null)
            throw new NotFoundException($"Module with id '{exam.ModuleId}' was not found.");

        var currentProgress = await unitOfWork.UserProgresses.GetByUserAndModuleAsync(userId, currentModule.Id);
        if (currentProgress is null)
        {
            currentProgress = new UserProgress
            {
                UserId = userId,
                ModuleId = currentModule.Id
            };
            await unitOfWork.UserProgresses.AddAsync(currentProgress);
        }

        currentProgress.Score = request.Score;
        currentProgress.PassedExam = exam.HasPassed(request.Score);

        if (exam.HasPassed(request.Score))
        {
            var modules = await unitOfWork.Modules.GetByTrackIdAsync(currentModule.TrackId);
            var nextModule = modules.FirstOrDefault(m => m.Order == currentModule.Order + 1);

            if (nextModule is not null)
            {
                var nextProgress = await unitOfWork.UserProgresses.GetByUserAndModuleAsync(userId, nextModule.Id);
                if (nextProgress is null)
                {
                    nextProgress = new UserProgress
                    {
                        UserId = userId,
                        ModuleId = nextModule.Id
                    };
                    await unitOfWork.UserProgresses.AddAsync(nextProgress);
                }

                nextProgress.IsUnlocked = true;
            }
        }
        else
        {
            currentProgress.PassedExam = false;
        }

        await unitOfWork.SaveChangesAsync();
        return new ExamResultResponse(exam.Id, exam.HasPassed(request.Score), request.Score);
    }
}
