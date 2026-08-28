namespace TalebElm.Domain.Entities
{
    public class Lesson :BaseEntity
    { 
        public string Title { get; set; } = string.Empty;

        public string Content { get; set; } = string.Empty;

        public int Order { get; set; }

        public Guid ModuleId { get; set; }

    }
}
