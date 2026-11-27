using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TalebElm.Api.Controllers;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;

namespace TalebElm.Tests.IntegrationTests;

public class ModulesControllerTests
{
    private readonly Mock<IModuleService> _moduleService = new();

    [Fact]
    public async Task Post_ReturnsCreatedWithModuleResponse()
    {
        var trackId = Guid.NewGuid();
        var expected = new ModuleResponse(Guid.NewGuid(), "M1", "Summary", 1, trackId);

        _moduleService
            .Setup(s => s.CreateAsync(It.IsAny<CreateModuleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new ModulesController(_moduleService.Object);

        var actionResult = await controller.Post(trackId, new CreateModuleRequest("M1", "Summary", 1, Guid.Empty));

        var createdResult = Assert.IsType<CreatedResult>(actionResult);
        Assert.Equal(StatusCodes.Status201Created, createdResult.StatusCode);
        Assert.Equal(expected, createdResult.Value);
        _moduleService.Verify(
            s => s.CreateAsync(It.Is<CreateModuleRequest>(r => r.TrackId == trackId), It.IsAny<CancellationToken>()),
            Times.Once);
    }

    [Fact]
    public async Task Put_ReturnsOkWithModuleResponse()
    {
        var moduleId = Guid.NewGuid();
        var trackId = Guid.NewGuid();
        var expected = new ModuleResponse(moduleId, "New", "New summary", 2, trackId);

        _moduleService
            .Setup(s => s.UpdateAsync(moduleId, It.IsAny<UpdateModuleRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var controller = new ModulesController(_moduleService.Object);

        var actionResult = await controller.Put(moduleId, new UpdateModuleRequest("New", "New summary", 2, trackId));

        var okResult = Assert.IsType<OkObjectResult>(actionResult);
        Assert.Equal(expected, okResult.Value);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var moduleId = Guid.NewGuid();
        var controller = new ModulesController(_moduleService.Object);

        var actionResult = await controller.Delete(moduleId);

        Assert.IsType<NoContentResult>(actionResult);
        _moduleService.Verify(s => s.DeleteAsync(moduleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData("Post")]
    [InlineData("Put")]
    [InlineData("Delete")]
    public void WriteActions_AreGatedByAdminOnly(string methodName)
    {
        var method = typeof(ModulesController).GetMethod(methodName)!;
        var attribute = method.GetCustomAttributes(typeof(AuthorizeAttribute), inherit: false)
            .Cast<AuthorizeAttribute>().Single();

        Assert.Equal("AdminOnly", attribute.Policy);
    }
}
