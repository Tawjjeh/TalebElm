
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class TrackEnrollmentConfiguration : IEntityTypeConfiguration<TrackEnrollment>
{
    public void Configure(EntityTypeBuilder<TrackEnrollment> builder)
    {
        builder.HasKey(te => te.Id); 

        builder.HasIndex(te => new { te.UserId, te.TrackId })
            .IsUnique();

        builder.HasOne(te => te.User)
            .WithMany(u => u.TrackEnrollments)
            .HasForeignKey(te => te.UserId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(te => te.Track)
            .WithMany(t => t.TrackEnrollments)
            .HasForeignKey(te => te.TrackId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
