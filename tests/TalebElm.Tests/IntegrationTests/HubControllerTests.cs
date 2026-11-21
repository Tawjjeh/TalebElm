using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TalebElm.Api.Controllers;
using TalebElm.Api.Middlewares;
using TalebElm.Api.Models;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Tests.IntegrationTests;

public class HubControllerTests
{
    private static HubController CreateController(
        IResourceHubReadService service, CancellationToken ct = default)
    {
        var context = new DefaultHttpContext();
        context.RequestAborted = ct;
        return new HubController(service)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
    }

    [Fact]
    public async Task GetTechnologies_ReturnsTopLevelCategories_AndForwardsToken()
    {
        using var cts = new CancellationTokenSource();
        var expected = new List<TechnologySummaryResponse>
        {
            new(Guid.NewGuid(), null, "C#", "csharp", 1, 1, 1, 2)
        };
        var service = new Mock<IResourceHubReadService>();
        service.Setup(s => s.GetTechnologiesAsync(null, cts.Token))
            .ReturnsAsync(expected);

        var result = await CreateController(service.Object, cts.Token).GetTechnologies();

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        service.Verify(s => s.GetTechnologiesAsync(null, cts.Token), Times.Once);
    }

    [Fact]
    public async Task GetTechnologyBySlugPath_ForwardsSegmentsFiltersAndToken()
    {
        using var cts = new CancellationTokenSource();
        var empty = new PagedResult<ResourceResponse>([], 2, 5, 0, 0);
        var expected = new TechnologyDetailResponse(
            Guid.NewGuid(), null, "ASP.NET Core", "aspnet-core", null, 2, 1, [], empty);
        var service = new Mock<IResourceHubReadService>();
        service.Setup(s => s.GetTechnologyBySlugPathAsync(
                It.Is<IReadOnlyList<string>>(p => p.SequenceEqual(new[] { "CSharp", "ASPNet-Core" })),
                It.Is<ResourceFilterParameters>(f => f.SearchQuery == "api" && f.PageNumber == 2 && f.PageSize == 5),
                cts.Token))
            .ReturnsAsync(expected);

        var controller = CreateController(service.Object, cts.Token);
        var result = await controller.GetTechnologyBySlugPath(
            "CSharp/ASPNet-Core", new HubResourceQuery { Search = "api", Page = 2, Size = 5 });

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Same(expected, ok.Value);
        service.VerifyAll();
    }

    [Fact]
    public async Task GetResources_ForwardsAllFilters_AndReturnsEmpty200Page()
    {
        using var cts = new CancellationTokenSource();
        var empty = new PagedResult<ResourceResponse>([], 1, 10, 0, 0);
        var service = new Mock<IResourceHubReadService>();
        service.Setup(s => s.GetResourcesAsync(
                null,
                It.Is<ResourceFilterParameters>(f =>
                    f.SearchQuery == "dotnet" && f.Language == "en" &&
                    f.Level == 2 && f.IsPaid == false && f.Kind == 3 &&
                    f.PageNumber == 1 && f.PageSize == 10),
                cts.Token))
            .ReturnsAsync(empty);

        var query = new HubResourceQuery
        {
            Search = "dotnet", Language = "en", Level = 2,
            IsPaid = false, Kind = 3, Page = 1, Size = 10
        };
        var result = await CreateController(service.Object, cts.Token).GetResources(query);

        var ok = Assert.IsType<OkObjectResult>(result.Result);
        var response = Assert.IsType<PagedResult<ResourceResponse>>(ok.Value);
        Assert.Empty(response.Items);
        Assert.Equal(0, response.TotalCount);
        service.VerifyAll();
    }

    [Fact]
    public async Task GetTopicResources_ForwardsTopicId_AndReturns200()
    {
        using var cts = new CancellationTokenSource();
        var topicId = Guid.NewGuid();
        var expected = new PagedResult<ResourceResponse>([], 1, 20, 0, 0);
        var service = new Mock<IResourceHubReadService>();
        service.Setup(s => s.GetResourcesAsync(
                topicId,
                It.Is<ResourceFilterParameters>(f => f.PageNumber == 1 && f.PageSize == 20),
                cts.Token))
            .ReturnsAsync(expected);

        var result = await CreateController(service.Object, cts.Token)
            .GetTopicResources(topicId, new HubResourceQuery());

        Assert.Same(expected, Assert.IsType<OkObjectResult>(result.Result).Value);
        service.VerifyAll();
    }

    [Theory]
    [InlineData(0, 20)]
    [InlineData(-1, 20)]
    [InlineData(1, 0)]
    [InlineData(1, 101)]
    public async Task GetResources_InvalidPaging_Returns400(int page, int size)
    {
        var controller = CreateController(new ResourceHubReadService(Mock.Of<IUnitOfWork>()));

        var status = await RunThroughMiddlewareAsync(async () =>
        {
            await controller.GetResources(new HubResourceQuery { Page = page, Size = size });
        });

        Assert.Equal(StatusCodes.Status400BadRequest, status);
    }

    [Fact]
    public async Task GetTopicResources_UnknownTopic_Returns404()
    {
        var id = Guid.NewGuid();
        var technologies = new Mock<ITechnologyRepository>();
        technologies.Setup(t => t.GetByIdAsync(id, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technology?)null);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(u => u.Technologies).Returns(technologies.Object);
        var controller = CreateController(new ResourceHubReadService(unitOfWork.Object));

        var status = await RunThroughMiddlewareAsync(async () =>
        {
            await controller.GetTopicResources(id, new HubResourceQuery());
        });

        Assert.Equal(StatusCodes.Status404NotFound, status);
    }

    [Fact]
    public async Task GetTechnologyBySlugPath_UnknownPath_Returns404()
    {
        var technologies = new Mock<ITechnologyRepository>();
        technologies.Setup(t => t.GetBySlugPathAsync(
                It.IsAny<IReadOnlyList<string>>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync((Technology?)null);
        var unitOfWork = new Mock<IUnitOfWork>();
        unitOfWork.SetupGet(u => u.Technologies).Returns(technologies.Object);
        var controller = CreateController(new ResourceHubReadService(unitOfWork.Object));

        var status = await RunThroughMiddlewareAsync(async () =>
        {
            await controller.GetTechnologyBySlugPath("missing/topic", new HubResourceQuery());
        });

        Assert.Equal(StatusCodes.Status404NotFound, status);
    }

    [Fact]
    public void HubRoutes_AreAnonymousAndMatchThePublicApiContract()
    {
        Assert.NotNull(typeof(HubController).GetCustomAttribute<AllowAnonymousAttribute>());
        Assert.Equal("api/hub", typeof(HubController).GetCustomAttribute<RouteAttribute>()?.Template);
        Assert.Equal("technologies", GetTemplate(nameof(HubController.GetTechnologies)));
        Assert.Equal("technologies/{**slugPath}", GetTemplate(nameof(HubController.GetTechnologyBySlugPath)));
        Assert.Equal("topics/{id:guid}/resources", GetTemplate(nameof(HubController.GetTopicResources)));
        Assert.Equal("resources", GetTemplate(nameof(HubController.GetResources)));
    }

    private static string? GetTemplate(string actionName) =>
        typeof(HubController).GetMethod(actionName)?.GetCustomAttribute<HttpGetAttribute>()?.Template;

    private static async Task<int> RunThroughMiddlewareAsync(Func<Task> action)
    {
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();
        var middleware = new ExceptionHandlingMiddleware(async _ => await action());
        await middleware.InvokeAsync(context);
        return context.Response.StatusCode;
    }
}
