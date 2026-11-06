using FluentValidation;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TalebElm.Application;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure;

namespace TalebElm.Tests.UnitTests;

public class DependencyResolutionTests
{
    private static ServiceProvider BuildServiceProvider()
    {
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:TalebElm"] = "Data Source=:memory:"
            })
            .Build();

        var services = new ServiceCollection();
        services.AddLogging();
        services.AddInfrastructure(configuration);
        services.AddApplication();

        return services.BuildServiceProvider();
    }

    [Fact]
    public void AllApplicationServices_ResolveFromDependencyInjection()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        Assert.NotNull(sp.GetRequiredService<ITrackService>());
        Assert.NotNull(sp.GetRequiredService<IModuleService>());
        Assert.NotNull(sp.GetRequiredService<ILessonService>());
        Assert.NotNull(sp.GetRequiredService<IExamService>());
        Assert.NotNull(sp.GetRequiredService<IUserService>());
        Assert.NotNull(sp.GetRequiredService<IUserProgressService>());
        Assert.NotNull(sp.GetRequiredService<ITrackEnrollmentService>());
        Assert.NotNull(sp.GetRequiredService<IPasswordHasher>());
    }

    [Fact]
    public void AllRepositoriesAndUnitOfWork_ResolveFromDependencyInjection()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        Assert.NotNull(sp.GetRequiredService<IUnitOfWork>());
        Assert.NotNull(sp.GetRequiredService<ITrackRepository>());
        Assert.NotNull(sp.GetRequiredService<IModuleRepository>());
        Assert.NotNull(sp.GetRequiredService<ILessonRepository>());
        Assert.NotNull(sp.GetRequiredService<ILessonResourceRepository>());
        Assert.NotNull(sp.GetRequiredService<IExamRepository>());
        Assert.NotNull(sp.GetRequiredService<IUserRepository>());
        Assert.NotNull(sp.GetRequiredService<IUserProgressRepository>());
        Assert.NotNull(sp.GetRequiredService<ITrackEnrollmentRepository>());
    }

    [Fact]
    public void AllValidators_ResolveFromDependencyInjection()
    {
        using var provider = BuildServiceProvider();
        using var scope = provider.CreateScope();
        var sp = scope.ServiceProvider;

        Assert.NotNull(sp.GetRequiredService<IValidator<CreateTrackRequest>>());
        Assert.NotNull(sp.GetRequiredService<IValidator<CreateModuleRequest>>());
        Assert.NotNull(sp.GetRequiredService<IValidator<UpdateModuleRequest>>());
        Assert.NotNull(sp.GetRequiredService<IValidator<CreateLessonRequest>>());
        Assert.NotNull(sp.GetRequiredService<IValidator<UpdateLessonRequest>>());
        Assert.NotNull(sp.GetRequiredService<IValidator<CreateExamRequest>>());
    }
}
