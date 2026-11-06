using FluentValidation;
using Microsoft.Extensions.DependencyInjection;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Application.Services.Concrete;
using TalebElm.Application.Validators;

namespace TalebElm.Application;

public static class DependencyInjection
{
    public static IServiceCollection AddApplication(this IServiceCollection services)
    {
        // Stateless and thread-safe: a single shared instance is enough.
        services.AddSingleton<IPasswordHasher, PasswordHasher>();

        services.AddValidatorsFromAssemblyContaining<CreateTrackRequestValidator>();

        services.AddScoped<ITrackService, TrackService>();
        services.AddScoped<IModuleService, ModuleService>();
        services.AddScoped<ILessonService, LessonService>();
        services.AddScoped<IExamService, ExamService>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IUserProgressService, UserProgressService>();
        services.AddScoped<ITrackEnrollmentService, TrackEnrollmentService>();

        return services;
    }
}
