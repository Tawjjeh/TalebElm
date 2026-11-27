using TalebElm.Domain.Entities;

namespace TalebElm.Domain.Interfaces;

public interface IExamRepository : IRepository<Exam>
{
    Task<Exam?> GetByModuleIdAsync(Guid moduleId, CancellationToken ct = default);
    Task<Exam?> GetWithQuestionsAsync(Guid examId, CancellationToken ct = default);

    /// <summary>
    /// Deletes the exam together with its questions and options. The exam
    /// must be loaded with <see cref="GetWithQuestionsAsync"/> first, since
    /// the FKs use DeleteBehavior.Restrict and there are no separate
    /// question/option repositories (Decision 37).
    /// </summary>
    Task RemoveAsync(Exam exam, CancellationToken ct = default);

    /// <summary>
    /// Adds one option to a question. Adds through the set (not the navigation
    /// collection): a new <see cref="ExamOption"/> carries a client-generated
    /// <see cref="Exam.Id"/>, so adding it via the collection would mark it
    /// Modified instead of Added. EF fixup still adds it to the question's
    /// <c>Options</c> collection.
    /// </summary>
    Task AddOptionAsync(ExamQuestion question, ExamOption option, CancellationToken ct = default);

    /// <summary>
    /// Removes one option. Removes through the set (not the navigation
    /// collection): the FK uses DeleteBehavior.Restrict, so severing the
    /// relationship via the collection throws instead of deleting the orphan.
    /// EF fixup still removes it from the question's <c>Options</c> collection.
    /// </summary>
    Task RemoveOptionAsync(ExamOption option, CancellationToken ct = default);
}
