using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class CapstoneProjectConfiguration : IEntityTypeConfiguration<CapstoneProject>
{
    public void Configure(EntityTypeBuilder<CapstoneProject> builder)
    {
        builder.ToTable("CapstoneProjects", t =>
        {
            t.HasCheckConstraint(
                "CK_CapstoneProjects_Title_NotEmpty",
                "length(trim(Title)) > 0");
        });

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasIndex(p => p.TrackId)
            .IsUnique();

        builder.HasIndex(p => p.TaskId)
            .IsUnique();

        builder.HasOne(p => p.Track)
            .WithMany()
            .HasForeignKey(p => p.TrackId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(p => p.Task)
            .WithMany()
            .HasForeignKey(p => p.TaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
