using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class UserExternalAccountConfiguration : IEntityTypeConfiguration<UserExternalAccount>
{
    public void Configure(EntityTypeBuilder<UserExternalAccount> builder)
    {
        builder.Property(x => x.Handle)
            .IsRequired()
            .HasMaxLength(64);

        builder.Property(x => x.ExternalNumericId)
            .HasMaxLength(64);

        builder.Property(x => x.VerificationToken)
            .HasMaxLength(64);

        builder.Property(x => x.Platform)
            .IsRequired();

        builder.Property(x => x.Status)
            .IsRequired();

        builder.HasOne<User>()
            .WithMany()
            .HasForeignKey(x => x.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        // Exactly one external account per platform per learner (Decision 33)
        builder.HasIndex(x => new { x.UserId, x.Platform })
            .IsUnique();

        // Unique handle per platform for verified accounts only (Decision 33)
        // Pending claims never block a real owner
        builder.HasIndex(x => new { x.Platform, x.Handle })
            .IsUnique()
            .HasFilter("Status = 1");

        // Unique external numeric ID per platform where provided (Decision 33)
        builder.HasIndex(x => new { x.Platform, x.ExternalNumericId })
            .IsUnique()
            .HasFilter("ExternalNumericId IS NOT NULL");

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_UserExternalAccounts_Handle_NotEmpty", "length(trim(Handle)) > 0");
        });
    }
}
