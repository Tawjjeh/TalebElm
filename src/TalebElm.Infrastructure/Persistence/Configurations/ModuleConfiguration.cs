using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence;

public class ModuleConfiguration : IEntityTypeConfiguration<Module>
{
    public void Configure(EntityTypeBuilder<Module> builder)
    {
        builder.HasOne(m => m.Track)
            .WithMany(t => t.Modules)
            .HasForeignKey(m => m.TrackId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}