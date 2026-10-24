namespace TalebElm.Domain.Entities;

public class ExamOption : BaseEntity
{
    public Guid ExamQuestionId { get; set; }
    public string Text { get; set; } = string.Empty;
    public bool IsCorrect { get; set; }
    public ExamQuestion ExamQuestion { get; set; } = default!;
}
