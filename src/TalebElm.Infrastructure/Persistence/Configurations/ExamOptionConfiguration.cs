using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class ExamOptionConfiguration : IEntityTypeConfiguration<ExamOption>
{
    public void Configure(EntityTypeBuilder<ExamOption> builder)
    {
        builder.Property(o => o.Text)
            .IsRequired();

        builder.HasOne(o => o.ExamQuestion)
            .WithMany(q => q.Options)
            .HasForeignKey(o => o.ExamQuestionId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
            t.HasCheckConstraint("CK_ExamOptions_Text_NotEmpty", "length(trim(\"Text\")) > 0"));
    }
}
