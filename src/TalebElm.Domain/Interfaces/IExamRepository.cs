using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface IExamRepository : IRepository<Exam>
{
    Task<Exam?> GetByModuleIdAsync(Guid moduleId);
    Task<Exam?> GetWithQuestionsAsync(Guid examId, CancellationToken ct = default);
}
