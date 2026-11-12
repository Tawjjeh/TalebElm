using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;


namespace TalebElm.Infrastructure.Persistence;

    public class ResourceConfiguration : IEntityTypeConfiguration<Resource>
    {
        public void Configure(EntityTypeBuilder<Resource> builder)
        {
            builder.Property(r => r.Title)
                .IsRequired()
                .HasMaxLength(200);

            builder.Property(r => r.Url)
                .HasMaxLength(2000);

            builder.Property(r => r.Citation)
                .HasMaxLength(500);

            builder.Property(r => r.Language)
                .IsRequired()
                .HasMaxLength(10);

            builder.HasIndex(r => new { r.TechnologyId, r.Status });

            builder.HasOne(r => r.Technology)
                .WithMany()
                .HasForeignKey(r => r.TechnologyId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasOne(r => r.CreatedByUser)
                .WithMany()
                .HasForeignKey(r => r.CreatedByUserId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Resources_Title_NotEmpty", "length(trim(Title)) > 0");
                t.HasCheckConstraint("CK_Resources_Language_NotEmpty", "length(trim(Language)) > 0");
                t.HasCheckConstraint("CK_Resources_ConsecutiveFailureCount_NonNegative", "ConsecutiveFailureCount >= 0");
            });
        }
    }

