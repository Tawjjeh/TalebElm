using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface IExamService
{
    Task<ExamResponse> CreateAsync(CreateExamRequest request, CancellationToken ct = default);
    Task<ExamResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ExamResultResponse> SubmitAsync(Guid userId, SubmitExamAnswersRequest request, CancellationToken ct = default);
}   
