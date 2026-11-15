using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class ProjectMilestoneConfiguration : IEntityTypeConfiguration<ProjectMilestone>
{
    public void Configure(EntityTypeBuilder<ProjectMilestone> builder)
    {
        builder.ToTable("ProjectMilestones", t =>
        {
            t.HasCheckConstraint(
                "CK_ProjectMilestones_Title_NotEmpty",
                "length(trim(Title)) > 0");
            t.HasCheckConstraint(
                "CK_ProjectMilestones_Order_Positive",
                "\"Order\" > 0");
            t.HasCheckConstraint(
                "CK_ProjectMilestones_WorkflowJobName_NotEmpty",
                "length(trim(WorkflowJobName)) > 0");
        });

        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(m => m.Instructions)
            .IsRequired()
            .HasMaxLength(2000);

        builder.Property(m => m.WorkflowJobName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(m => new { m.ProjectId, m.WorkflowJobName })
            .IsUnique();

        builder.HasIndex(m => new { m.ProjectId, m.Order })
            .IsUnique();

        builder.HasOne(m => m.Project)
            .WithMany(p => p.Milestones)
            .HasForeignKey(m => m.ProjectId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(m => m.Module)
            .WithMany()
            .HasForeignKey(m => m.ModuleId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
