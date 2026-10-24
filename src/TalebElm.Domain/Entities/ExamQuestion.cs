namespace TalebElm.Domain.Entities;

public class ExamQuestion : BaseEntity
{
    public Guid ExamId { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public int Order { get; set; }
    public int Points { get; set; }
    public Exam Exam { get; set; } = default!;
    public ICollection<ExamOption> Options { get; set; } = new List<ExamOption>();
}
