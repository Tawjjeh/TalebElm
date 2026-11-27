using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface IExamService
{
    Task<ExamResponse> CreateAsync(CreateExamRequest request, CancellationToken ct = default);
    Task<ExamResponse> GetByIdAsync(Guid id, CancellationToken ct = default);
    Task<ExamResultResponse> SubmitAsync(Guid userId, SubmitExamAnswersRequest request, CancellationToken ct = default);
    Task<ExamResponse> UpdateAsync(Guid id, UpdateExamRequest request, CancellationToken ct = default);
    Task<ExamQuestionResponse> UpdateQuestionAsync(Guid examId, Guid questionId, UpdateExamQuestionRequest request, CancellationToken ct = default);
    Task<ExamQuestionResponse> AddOptionAsync(Guid examId, Guid questionId, AddExamOptionRequest request, CancellationToken ct = default);
    Task<ExamQuestionResponse> RemoveOptionAsync(Guid examId, Guid questionId, Guid optionId, CancellationToken ct = default);
    Task DeleteAsync(Guid id, CancellationToken ct = default);
}   
