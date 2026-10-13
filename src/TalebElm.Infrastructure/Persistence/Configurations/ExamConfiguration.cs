using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence;

public class ExamConfiguration : IEntityTypeConfiguration<Exam>
{
    public void Configure(EntityTypeBuilder<Exam> builder)
    {
        builder.HasOne(e => e.Module)
            .WithOne(m => m.Exam)
            .HasForeignKey<Exam>(e => e.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(e => e.ModuleId).IsUnique();
    }
}
