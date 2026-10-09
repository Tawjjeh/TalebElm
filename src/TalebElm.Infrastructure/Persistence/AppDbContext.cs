
using Microsoft.EntityFrameworkCore;
using TalebElm.Domain.Entities;


namespace TalebElm.Infrastructure.Persistence;

public class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
      
    }
    public DbSet<User> Users => Set<User>();
    public DbSet<Exam> Exams => Set<Exam>();
    public DbSet<UserProgress> UserProgresses => Set<UserProgress>();
    public DbSet<Module> Modules => Set<Module>();
    public DbSet<Lesson> Lessons => Set<Lesson>();
    public DbSet<Track> Tracks => Set<Track>();
}
