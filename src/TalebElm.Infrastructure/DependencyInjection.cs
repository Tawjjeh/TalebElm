using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Persistence;
using TalebElm.Infrastructure.Persistence.Repositories;
using TalebElm.Infrastructure.Repositories;
using Microsoft.Extensions.Http.Resilience;
using TalebElm.Infrastructure.Services;
using Microsoft.Extensions.Options;

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
        services.AddScoped<ITechnologyRepository, TechnologyRepository>();
        services.AddScoped<IResourceRepository, ResourceRepository>();
        services.AddScoped<IUserExternalAccountRepository, UserExternalAccountRepository>();
        services.AddScoped<IModuleProblemRepository, ModuleProblemRepository>();
        services.AddScoped<IUserProblemCompletionRepository, UserProblemCompletionRepository>();

        services.Configure<CodeforcesOptions>(configuration.GetSection(CodeforcesOptions.SectionName));

        services.AddHttpClient<ICodeforcesClient, CodeforcesClient>()
            .ConfigureHttpClient((sp, client) =>
            {
                var options = sp.GetRequiredService<IOptions<CodeforcesOptions>>().Value;
                client.BaseAddress = new Uri(options.BaseUrl);
                client.Timeout = TimeSpan.FromSeconds(options.TimeoutSeconds);
                client.DefaultRequestHeaders.UserAgent.ParseAdd("TalebElm/1.0");
            })
            .AddStandardResilienceHandler();
      
        services.AddHttpClient<IGitHubClient, GitHubClient>();
        services.AddSingleton<IDnsResolver, SystemDnsResolver>();
        services.AddSingleton<PerHostRequestSpacer>();
        services.AddHttpClient<ILinkHealthChecker, LinkHealthChecker>()
            .ConfigurePrimaryHttpMessageHandler(sp =>
            {
                var dnsResolver = sp.GetRequiredService<IDnsResolver>();
                return new SocketsHttpHandler
                {
                    AllowAutoRedirect = false,
                    ConnectCallback = (context, ct) =>
                        LinkHealthChecker.ConnectWithSsrfGuardAsync(context, dnsResolver, ct)
                };
            });

        return services;
    }
}
