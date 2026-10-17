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

    public Task<ExamResultResponse> SubmitAsync(SubmitExamRequest request)
    {
        throw new NotImplementedException("Submitting an exam is not implemented yet.");
    }
}
