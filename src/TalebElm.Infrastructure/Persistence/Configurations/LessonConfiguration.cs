using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence;

public class LessonConfiguration : IEntityTypeConfiguration<Lesson>
{
    public void Configure(EntityTypeBuilder<Lesson> builder)
    {
        builder.Property(l => l.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(l => l.Content)
            .IsRequired();

        builder.Property(l => l.Order)
            .IsRequired();

        builder.HasIndex(l => new { l.ModuleId, l.Order })
            .IsUnique();

        builder.HasOne(l => l.Module)
            .WithMany(m => m.Lessons)
            .HasForeignKey(l => l.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Lessons_Title_NotEmpty", "length(trim(Title)) > 0");
            t.HasCheckConstraint("CK_Lessons_Order_Positive", "\"Order\" > 0");
        });
    }
}
