using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class ModuleProblemConfiguration : IEntityTypeConfiguration<ModuleProblem>
{
    public void Configure(EntityTypeBuilder<ModuleProblem> builder)
    {
        builder.HasIndex(mp => new { mp.ModuleId, mp.Order })
            .IsUnique();

        builder.HasOne(mp => mp.GitHubTask)
            .WithMany()
            .HasForeignKey(mp => mp.GitHubTaskId)
            .IsRequired(false)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_ModuleProblems_GitHubTaskId_Value", "(Kind = 1 AND GitHubTaskId IS NULL) OR (Kind = 2 AND GitHubTaskId IS NOT NULL)");
        });
    }
}
