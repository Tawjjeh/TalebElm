using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface IExamService
{
    Task<ExamResponse> CreateAsync(CreateExamRequest request);
    Task<ExamResponse> GetByIdAsync(Guid id);
    Task<ExamResultResponse> SubmitAsync(SubmitExamRequest request);
}   