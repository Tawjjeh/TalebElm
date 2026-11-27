using System.Security.Claims;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TalebElm.Api.Controllers;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;

namespace TalebElm.Tests.IntegrationTests;

public class ModulesControllerExamTests
{
    private readonly Mock<IExamService> _examService = new();
    private readonly Guid _learnerId = Guid.NewGuid();
    private readonly Guid _moduleId = Guid.NewGuid();

    private ModulesController ControllerWithClaims(string? learnerClaim = null)
    {
        var claims = learnerClaim is null
            ? Array.Empty<Claim>()
            : new[] { new Claim(ClaimTypes.NameIdentifier, learnerClaim) };
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };
        var controller = new ModulesController(_examService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
        return controller;
    }

    [Fact]
    public async Task GetExam_WithClaims_ReturnsOkWithLearnerSafeDetail()
    {
        var examId = Guid.NewGuid();
        var expected = new ExamDetailResponse(
            examId, "Midterm", 70, _moduleId,
            new List<ExamQuestionResponse>
            {
                new(Guid.NewGuid(), "First", 1, 10, new List<ExamOptionResponse> { new(Guid.NewGuid(), "A1") })
            });
        _examService.Setup(s => s.GetDetailByModuleForLearnerAsync(_learnerId, _moduleId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await ControllerWithClaims(_learnerId.ToString()).GetExam(_moduleId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        _examService.Verify(s => s.GetDetailByModuleForLearnerAsync(_learnerId, _moduleId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task GetExam_WithoutClaims_Returns401()
    {
        var result = await new ModulesController(_examService.Object).GetExam(_moduleId);

        Assert.IsType<UnauthorizedResult>(result);
        _examService.Verify(s => s.GetDetailByModuleForLearnerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }
}
