using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using TalebElm.Infrastructure;
using TalebElm.Infrastructure.Persistence;

namespace TalebElm.Tests.UnitTests;

public class InfrastructureRegistrationTests
{
    private static IConfiguration BuildConfiguration(Dictionary<string, string?> settings)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(settings)
            .Build();
    }

    [Fact]
    public void MissingConnectionString_Throws()
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>());
        var services = new ServiceCollection();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(
            () => services.AddInfrastructure(configuration));

        // Assert
        Assert.Contains("TalebElm", exception.Message);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    public void BlankConnectionString_Throws(string connectionString)
    {
        // Arrange
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:TalebElm"] = connectionString
        });
        var services = new ServiceCollection();

        // Act
        var exception = Assert.Throws<InvalidOperationException>(
            () => services.AddInfrastructure(configuration));

        // Assert
        Assert.Contains("TalebElm", exception.Message);
    }

    [Fact]
    public void AlternativeConnectionString_IsUsedByAppDbContext()
    {
        // Arrange
        const string connectionString = "Data Source=:memory:";
        var configuration = BuildConfiguration(new Dictionary<string, string?>
        {
            ["ConnectionStrings:TalebElm"] = connectionString
        });
        var services = new ServiceCollection();

        // Act
        services.AddInfrastructure(configuration);
        using var provider = services.BuildServiceProvider();
        using var scope = provider.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AppDbContext>();

        // Assert
        Assert.Equal("Microsoft.EntityFrameworkCore.Sqlite", context.Database.ProviderName);
        Assert.Equal(connectionString, context.Database.GetConnectionString());
    }
}
