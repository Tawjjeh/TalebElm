using Microsoft.Extensions.DependencyInjection;
using TalebElm.Application;
using TalebElm.Application.Services.Abstraction;

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
}
