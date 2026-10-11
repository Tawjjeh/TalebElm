
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;


namespace TalebElm.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
         foreach (var entityType in modelBuilder.Model.GetEntityTypes())
        {
            if (!typeof(BaseEntity).IsAssignableFrom(entityType.ClrType))
                continue;

            var entityBuilder = modelBuilder.Entity(entityType.ClrType);

            entityBuilder.HasKey(nameof(BaseEntity.Id));

            entityBuilder.Property(nameof(BaseEntity.CreatedAt))
                .IsRequired();
        }
    }
    public DbSet<User> Users => Set<User>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<UserProgress> UserProgresses => Set<UserProgress>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Track> Tracks => Set<Track>();
}
