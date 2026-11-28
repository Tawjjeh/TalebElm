using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface IExamRepository : IRepository<Exam>
{
    Task<Exam?> GetByModuleIdAsync(Guid moduleId, CancellationToken ct = default);
    Task<Exam?> GetWithQuestionsAsync(Guid examId, CancellationToken ct = default);
}
