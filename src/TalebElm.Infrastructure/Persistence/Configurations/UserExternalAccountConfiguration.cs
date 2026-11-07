using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class UserExternalAccountConfiguration : IEntityTypeConfiguration<UserExternalAccount>
{
    public void Configure(EntityTypeBuilder<UserExternalAccount> builder)
    {
        builder.Property(x => x.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.HasIndex(x => new { x.Platform, x.Handle })
            .IsUnique()
            .HasFilter("[Status] = 'Verified'");

        builder.HasIndex(x => new { x.Platform, x.ExternalNumericId })
            .IsUnique()
            .HasFilter("[ExternalNumericId] IS NOT NULL");
    }
}
