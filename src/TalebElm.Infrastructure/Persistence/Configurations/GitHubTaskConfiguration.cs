using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class GitHubTaskConfiguration : IEntityTypeConfiguration<GitHubTask>
{
    public void Configure(EntityTypeBuilder<GitHubTask> builder)
    {
        builder.ToTable("GitHubTask");

        builder.Property(x => x.TemplateRepository)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.WorkflowFileName)
            .IsRequired()
            .HasMaxLength(200);

        builder.Property(x => x.IsActive)
            .IsRequired();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_GitHubTask_TemplateRepository_NotEmpty", "length(trim(TemplateRepository)) > 0");
            t.HasCheckConstraint("CK_GitHubTask_MinPassingTests_Positive", "MinPassingTests IS NULL OR MinPassingTests > 0");
        });
    }
}
