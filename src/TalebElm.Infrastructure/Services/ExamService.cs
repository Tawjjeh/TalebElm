using TalebElm.Application.DTOs;
using TalebElm.Application.Services;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;
using NotImplementedException = System.NotImplementedException;

namespace TalebElm.Infrastructure.Services;

public class ExamService(IUnitOfWork unitOfWork) : IExamService
{
    public Task<ExamResponse> CreateAsync(CreateExamRequest request)
    {
        throw new NotImplementedException();
    }

    public async Task<ExamResponse> GetByIdAsync(Guid id)
    {
        var exam = await unitOfWork.Exams.GetByIdAsync(id);
        if (exam is null)
            throw new NotFoundException($"Exam With Id {id} Not found");


        return new ExamResponse(
            exam.Id,
            exam.Title,
            exam.PassThreshold,
            exam.ModuleId
        );
    }

    public Task<ExamResultResponse> SubmitAsync(SubmitExamRequest request)
    {
        throw new NotImplementedException();
    }
}
