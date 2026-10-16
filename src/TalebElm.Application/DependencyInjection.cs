using Microsoft.Extensions.DependencyInjection;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Application.Services.Concrete;

namespace TalebElm.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Stateless and thread-safe: a single shared instance is enough.
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        // NOTE (#274): service, repository, and validator registrations land here.
        return services;
    }
}
