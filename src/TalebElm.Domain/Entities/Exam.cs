namespace TalebElm.Domain.Entities;

public class Exam : BaseEntity
{
    public string Title { get; set; } = string.Empty;
    public int PassThreshold { get; set; }
    public Guid ModuleId { get; set; }
    public Module Module { get; set; } = default!;
    public ICollection<ExamQuestion> Questions { get; set; } = new List<ExamQuestion>();
    public bool HasPassed(int score) => score >= PassThreshold;
}

