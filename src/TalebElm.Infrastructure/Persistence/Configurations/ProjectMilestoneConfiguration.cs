using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class ProjectMilestoneConfiguration : IEntityTypeConfiguration<ProjectMilestone>
{
    public void Configure(EntityTypeBuilder<ProjectMilestone> builder)
    {
        builder.HasKey(m => m.Id);

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(m => m.Instructions)
            .IsRequired()
            .HasMaxLength(2000);

        builder.HasIndex(m => new { m.ProjectId, m.Order })
            .IsUnique();

        builder.HasOne<CapstoneProject>()
            .WithMany()
            .HasForeignKey(m => m.ProjectId)
            .IsRequired()
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<Module>()
            .WithMany()
            .HasForeignKey(m => m.ModuleId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
