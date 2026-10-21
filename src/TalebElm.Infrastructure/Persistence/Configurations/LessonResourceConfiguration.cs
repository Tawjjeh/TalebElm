using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class LessonResourceConfiguration : IEntityTypeConfiguration<LessonResource>
{
    public void Configure(EntityTypeBuilder<LessonResource> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.Title).IsRequired().HasMaxLength(200);
        builder.Property(r => r.Citation).HasMaxLength(500);
        builder.Property(r => r.Order).IsRequired();

        builder.ToTable(t => t.HasCheckConstraint(
            "CK_LessonResources_Order_Positive",
            "\"Order\" > 0"));

        builder.HasOne(r => r.Lesson)
            .WithMany()
            .HasForeignKey(r => r.LessonId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);
    }
}
