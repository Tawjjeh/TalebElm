using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;


namespace TalebElm.Infrastructure.Persistence;

public class TechnologyConfiguration : IEntityTypeConfiguration<Technology>
{
        public void Configure(EntityTypeBuilder<Technology> builder)
        {
            builder.Property(t => t.Name)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Description)
                .HasMaxLength(500);

            builder.Property(t => t.Slug)
                .IsRequired()
                .HasMaxLength(100);

            builder.Property(t => t.Order)
                .IsRequired();

            builder.HasOne(t => t.Parent)
                .WithMany(t => t.Children)
                .HasForeignKey(t => t.ParentId)
                .OnDelete(DeleteBehavior.Restrict);


            builder.HasIndex(t => new { t.ParentId, t.Slug })
                .IsUnique()
                .HasFilter("\"ParentId\" IS NOT NULL");

            builder.HasIndex(t => t.Slug)
                .IsUnique()
                .HasFilter("\"ParentId\" IS NULL");

            builder.HasIndex(t => new { t.ParentId, t.Order })
                .IsUnique()
                .HasFilter("\"ParentId\" IS NOT NULL");

            builder.HasIndex(t => t.Order)
                .IsUnique()
                .HasFilter("\"ParentId\" IS NULL");

            builder.ToTable(t =>
            {
                t.HasCheckConstraint("CK_Technologies_Name_NotEmpty", "length(trim(Name)) > 0");
                t.HasCheckConstraint("CK_Technologies_Slug_NotEmpty", "length(trim(Slug)) > 0");
                t.HasCheckConstraint("CK_Technologies_Order_Positive", "\"Order\" > 0");
            });
        }
    }

