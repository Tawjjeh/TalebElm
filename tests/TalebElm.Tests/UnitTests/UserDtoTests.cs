using TalebElm.Application.DTOs;

namespace TalebElm.Tests.UnitTests;

public class UserDtoTests
{
    [Fact]
    public void CreateUserRequest_StoresNameAndEmail()
    {
        var request = new CreateUserRequest("Ada Lovelace", "ada@example.com");

        Assert.Equal("Ada Lovelace", request.Name);
        Assert.Equal("ada@example.com", request.Email);
    }

    [Fact]
    public void NoDto_ShouldExposePasswordOrHash()
    {
        var dtoTypes = typeof(UserResponse).Assembly
            .GetTypes()
            .Where(t => t.IsClass && t.Namespace == typeof(UserResponse).Namespace);

        foreach (var dtoType in dtoTypes)
        {
            Assert.DoesNotContain(dtoType.GetProperties(), property =>
                property.Name.Contains("Password", StringComparison.OrdinalIgnoreCase) ||
                property.Name.Contains("Hash", StringComparison.OrdinalIgnoreCase));
        }
    }
}
