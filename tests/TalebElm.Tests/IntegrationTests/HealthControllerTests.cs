
using Microsoft.AspNetCore.Mvc;
using TalebElm.Api.Controllers;

namespace TalebElm.Tests.IntegrationTests;

public class HealthControllerTests
{
    [Fact]
    public void Get_ReturnsOkWithHealthyMessage()
    {
        // Arrange
        var controller = new HealthController();

        // Act
        var result = controller.Get();

        // Assert
        var okResult = Assert.IsType<OkObjectResult>(result);
        Assert.Equal("healthy", Assert.IsType<string>(okResult.Value));
    }
}
