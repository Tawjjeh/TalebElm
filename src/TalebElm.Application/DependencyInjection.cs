using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TalebElm.Application.Validators;

namespace TalebElm.Application;

public static class DependencyInjection {
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
      
        services.AddValidatorsFromAssemblyContaining<CreateTrackRequestValidator>();

        return services;
    }
}