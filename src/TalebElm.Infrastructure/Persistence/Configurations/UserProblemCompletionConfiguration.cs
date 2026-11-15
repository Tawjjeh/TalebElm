using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class UserProblemCompletionConfiguration : IEntityTypeConfiguration<UserProblemCompletion>
{
    public void Configure(EntityTypeBuilder<UserProblemCompletion> builder)
    {
        builder.Property(c => c.ExternalSubmissionId)
            .HasMaxLength(200);

        builder.Property(c => c.VerifiedHeadCommitSha)
            .HasMaxLength(40);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(c => c.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.ModuleProblem)
            .WithMany()
            .HasForeignKey(c => c.ModuleProblemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(c => new { c.UserId, c.ModuleProblemId })
            .IsUnique();
    }
}
