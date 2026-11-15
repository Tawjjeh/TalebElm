using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence.Configurations;

public class CapstoneProjectConfiguration : IEntityTypeConfiguration<CapstoneProject>
{
    public void Configure(EntityTypeBuilder<CapstoneProject> builder)
    {
        builder.HasKey(p => p.Id);

        builder.Property(p => p.Title)
            .IsRequired()
            .HasMaxLength(150);

        builder.Property(p => p.Description)
            .IsRequired()
            .HasMaxLength(4000);

        builder.HasIndex(p => p.TrackId)
            .IsUnique();

        builder.HasIndex(p => p.TaskId)
            .IsUnique();

        builder.HasOne<Track>()
            .WithMany()
            .HasForeignKey(p => p.TrackId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne<GitHubTask>()
            .WithMany()
            .HasForeignKey(p => p.TaskId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
