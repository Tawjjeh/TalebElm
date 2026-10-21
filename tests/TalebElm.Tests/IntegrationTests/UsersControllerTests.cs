using TalebElm.Api.Controllers;
using DomainNotImplemented = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Tests.IntegrationTests;

public class UsersControllerTests
{
    [Fact]
    public void Get_ThrowsDomainNotImplementedException()
    {
        var exception = Assert.Throws<DomainNotImplemented>(() => new UsersController().Get());

        Assert.Contains("Listing users", exception.Message);
    }

    [Fact]
    public void Post_ThrowsDomainNotImplementedException()
    {
        var exception = Assert.Throws<DomainNotImplemented>(() => new UsersController().Post());

        Assert.Contains("Creating a user", exception.Message);
    }
}
