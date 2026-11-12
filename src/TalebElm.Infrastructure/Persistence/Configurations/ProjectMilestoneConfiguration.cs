
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class ProjectMilestoneConfiguration : IEntityTypeConfiguration<ProjectMilestone>
{
    public void Configure(EntityTypeBuilder<ProjectMilestone> builder)
    {
        builder.Property(op => op.WorkflowJobName)
            .IsRequired()
            .HasMaxLength(100);

        builder.HasIndex(milestone => new
        {
            milestone.ProjectId,
            milestone.WorkflowJobName
        })
            .IsUnique();

        builder.HasIndex(milestone => new
        {
            milestone.ProjectId,
            milestone.Order
        })
        .IsUnique();


    }
}
