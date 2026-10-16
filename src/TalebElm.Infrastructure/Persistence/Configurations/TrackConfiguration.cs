using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence;

public class TrackConfiguration : IEntityTypeConfiguration<Track>
{
    public void Configure(EntityTypeBuilder<Track> builder)
    {
        builder.Property(t => t.Name)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(t => t.Description)
            .IsRequired()
            .HasMaxLength(1000);

        builder.Property(t => t.Status)
            .IsRequired();

        builder.ToTable(t =>
        {
            t.HasCheckConstraint("CK_Tracks_Name_NotEmpty", "length(trim(Name)) > 0");
            t.HasCheckConstraint("CK_Tracks_Description_NotEmpty", "length(trim(Description)) > 0");
        });
    }
}
