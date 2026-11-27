using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface IExamService
{
    Task<ExamResponse> CreateAsync(CreateExamRequest request, CancellationToken ct = default);
    Task<ExamResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ExamDetailResponse> GetDetailForLearnerAsync(Guid learnerId, Guid examId, CancellationToken ct = default);
    Task<ExamDetailResponse> GetDetailByModuleForLearnerAsync(Guid learnerId, Guid moduleId, CancellationToken ct = default);
    Task<ExamResultResponse> SubmitAsync(Guid userId, SubmitExamAnswersRequest request, CancellationToken ct = default);
}   
