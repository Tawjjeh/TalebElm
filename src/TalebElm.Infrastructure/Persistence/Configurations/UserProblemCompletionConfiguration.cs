
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class UserProblemCompletionConfiguration : IEntityTypeConfiguration<UserProblemCompletion>
{
    public void Configure(EntityTypeBuilder<UserProblemCompletion> builder)
    {
        builder.Property(op => op.ExternalSubmissionId)
            .HasMaxLength(200);

        builder.Property(op => op.VerifiedHeadCommitSha)
            .HasMaxLength(40);

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(op => op.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(op => op.ModuleProblem)
            .WithMany()
            .HasForeignKey(op => op.ModuleProblemId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(op => new
        {
            op.UserId,
            op.ModuleProblemId
        }).IsUnique();

    }
}
