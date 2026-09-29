using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        string? connectionString = configuration.GetConnectionString("TalebElm");
        if (string.IsNullOrWhiteSpace(connectionString))
            throw new InvalidOperationException(
                $"Connection string 'TalebElm' does not exist or is empty. See docs/LOCAL_SETUP.md for details)");
        services.AddDbContext<AppDbContext>(o => o.UseSqlite(connectionString));
        return services;
    }
}