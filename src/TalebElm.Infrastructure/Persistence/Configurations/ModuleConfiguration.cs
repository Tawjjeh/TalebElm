using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.Property(m => m.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(m => m.Summary)
            .IsRequired()
            .HasMaxLength(500);

        builder.Property(m => m.Order)
            .IsRequired();

        builder.HasIndex(m => new { m.TrackId, m.Order })
            .IsUnique();

        builder.HasOne(m => m.Track)
            .WithMany(t => t.Modules)
            .HasForeignKey(m => m.TrackId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Modules_Title_NotEmpty", "length(trim(Title)) > 0");
            t.HasCheckConstraint("CK_Modules_Order_Positive", "\"Order\" > 0");
        });
    }
}
