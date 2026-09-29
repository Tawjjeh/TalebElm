using TalebElm.Application.DTOs;
using TalebElm.Application.Services;

namespace TalebElm.Infrastructure.Services;

public class ExamService : IExamService
{
    public Task<ExamResponse> CreateAsync(CreateExamRequest request)
    {
        throw new NotImplementedException();
    }

    public Task<ExamResponse> GetByIdAsync(Guid id)
    {
        throw new NotImplementedException();
    }

    public Task<ExamResultResponse> SubmitAsync(SubmitExamRequest request)
    {
        throw new NotImplementedException();
    }
}
