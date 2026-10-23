using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence;

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.Property(e => e.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(e => e.PassThreshold)
            .IsRequired();

        builder.HasOne(e => e.Module)
            .WithOne(m => m.Exam)
            .HasForeignKey<Exam>(e => e.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ModuleId).IsUnique();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Exams_Title_NotEmpty", "length(trim(Title)) > 0");
            t.HasCheckConstraint("CK_Exams_PassThreshold_Range", "PassThreshold >= 1 AND PassThreshold <= 100");
        });
    }
}
