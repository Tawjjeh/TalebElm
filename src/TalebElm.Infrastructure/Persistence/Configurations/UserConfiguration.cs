using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence;

public class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.Property(u => u.Name)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.Email)
            .IsRequired()
            .HasMaxLength(256)
            .UseCollation("NOCASE");

        builder.HasIndex(u => u.Email)
            .IsUnique();

        builder.Property(u => u.PasswordHash)
            .IsRequired();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Users_Name_NotEmpty", "length(trim(Name)) > 0");
            t.HasCheckConstraint("CK_Users_Email_NotEmpty", "length(trim(Email)) > 0");
            t.HasCheckConstraint("CK_Users_PasswordHash_NotEmpty", "length(PasswordHash) > 0");
        });
    }
}