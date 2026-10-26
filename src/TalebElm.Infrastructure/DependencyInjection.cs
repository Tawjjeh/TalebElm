using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;
using TalebElm.Infrastructure.Persistence.Repositories;
using TalebElm.Infrastructure.Repositories;

namespace TalebElm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("TalebElm");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                "Connection string 'TalebElm' does not exist or is empty. See docs/LOCAL_SETUP.md for details");
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));

        services.AddScoped<IUnitOfWork, UnitOfWork>();
        services.AddScoped<ITrackRepository, TrackRepository>();
        services.AddScoped<IModuleRepository, ModuleRepository>();
        services.AddScoped<ILessonRepository, LessonRepository>();
        services.AddScoped<ILessonResourceRepository, LessonResourceRepository>();
        services.AddScoped<IExamRepository, ExamRepository>();
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserProgressRepository, UserProgressRepository>();
        services.AddScoped<ITrackEnrollmentRepository, TrackEnrollmentRepository>();

        return services;
    }
}