using Microsoft.Extensions.DependencyInjection;
using Moq;
using TalebElm.Application;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Tests.UnitTests;

public class ApplicationRegistrationTests
{
    [Fact]
    public void AddApplication_RegistersPasswordHasher()
    {
        var services = new ServiceCollection();
        services.AddApplication();

        using var provider = services.BuildServiceProvider();
        var hasher = provider.GetService<IPasswordHasher>();

        Assert.NotNull(hasher);
        Assert.True(
            hasher.VerifyHashedPassword(
                hasher.HashPassword("Correct-Horse-1"),
                "Correct-Horse-1"));
    }

    [Fact]
    public void AddApplication_RegistersAllApplicationServices()
    {
        var services = new ServiceCollection();
        services.AddApplication();
        services.AddScoped<IUnitOfWork>(_ => new Mock<IUnitOfWork>().Object);

        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();

        Assert.NotNull(scope.ServiceProvider.GetService<ITrackService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IModuleService>());
        Assert.NotNull(scope.ServiceProvider.GetService<ILessonService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IExamService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IUserService>());
        Assert.NotNull(scope.ServiceProvider.GetService<IUserProgressService>());
        Assert.NotNull(scope.ServiceProvider.GetService<ITrackEnrollmentService>());
    }
}
