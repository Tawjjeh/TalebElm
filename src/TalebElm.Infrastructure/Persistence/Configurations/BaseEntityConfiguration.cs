
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TalebElm.Domain.Entities;

namespace TalebElm.Infrastructure.Persistence;

public static class BaseEntityConfiguration
{
    public static void Configure<TEntity>(
        EntityTypeBuilder<TEntity> builder)
        where TEntity : BaseEntity
    {
        builder.HasKey(entity => entity.Id);

        builder.Property(entity => entity.CreatedAt)
            .IsRequired();
    }
}
