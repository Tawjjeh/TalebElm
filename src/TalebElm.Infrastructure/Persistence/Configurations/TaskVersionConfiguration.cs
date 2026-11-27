using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class TaskVersionConfiguration : IEntityTypeConfiguration<TaskVersion>
{
    public void Configure(EntityTypeBuilder<TaskVersion> builder)
    {
        builder.Property(x => x.VersionTag)
            .IsRequired()
            .HasMaxLength(50);

        builder.Property(x => x.CommitSha)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(x => x.WorkflowFileSha)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(x => x.TestTreeSha)
            .IsRequired()
            .HasMaxLength(40);

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasOne(x => x.GitHubTask)
            .WithMany()
            .HasForeignKey(x => x.GitHubTaskId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(x => new { x.GitHubTaskId, x.VersionTag })
            .IsUnique();

        builder.HasIndex(x => x.GitHubTaskId)
            .IsUnique()
            .HasFilter("Status = 1");
    }
}
