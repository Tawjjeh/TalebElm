using System.Security.Claims;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TalebElm.Api.Controllers;
using TalebElm.Api.Middlewares;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Exceptions;

namespace TalebElm.Tests.IntegrationTests;

public class ExamsControllerTests
{
    private readonly Mock<IExamService> _examService = new();
    private readonly Guid _learnerId = Guid.NewGuid();
    private readonly Guid _examId = Guid.NewGuid();

    private ExamsController ControllerWithClaims(string? learnerClaim = null)
    {
        var claims = learnerClaim is null
            ? Array.Empty<Claim>()
            : new[] { new Claim(ClaimTypes.NameIdentifier, learnerClaim) };
        var context = new DefaultHttpContext
        {
            User = new ClaimsPrincipal(new ClaimsIdentity(claims, "Test"))
        };
        var controller = new ExamsController(_examService.Object)
        {
            ControllerContext = new ControllerContext { HttpContext = context }
        };
        return controller;
    }

    private ExamsController ControllerWithoutClaims() => new(_examService.Object);

    private static ExamDetailResponse Detail(Guid examId, Guid moduleId) => new(
        examId,
        "Midterm",
        70,
        moduleId,
        new List<ExamQuestionResponse>
        {
            new(Guid.NewGuid(), "First", 1, 10, new List<ExamOptionResponse>
            {
                new(Guid.NewGuid(), "A1"),
                new(Guid.NewGuid(), "A2")
            }),
            new(Guid.NewGuid(), "Second", 2, 20, new List<ExamOptionResponse>
            {
                new(Guid.NewGuid(), "B1"),
                new(Guid.NewGuid(), "B2")
            })
        });

    [Fact]
    public async Task Get_WithClaims_ReturnsOkWithLearnerSafeDetail()
    {
        var moduleId = Guid.NewGuid();
        var expected = Detail(_examId, moduleId);
        _examService.Setup(s => s.GetDetailForLearnerAsync(_learnerId, _examId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await ControllerWithClaims(_learnerId.ToString()).Get(_examId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        _examService.Verify(s => s.GetDetailForLearnerAsync(_learnerId, _examId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Get_WithoutClaims_Returns401()
    {
        var result = await ControllerWithoutClaims().Get(_examId);

        Assert.IsType<UnauthorizedResult>(result);
        _examService.Verify(s => s.GetDetailForLearnerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Get_WithNonGuidClaim_Returns401()
    {
        var result = await ControllerWithClaims("not-a-guid").Get(_examId);

        Assert.IsType<UnauthorizedResult>(result);
        _examService.Verify(s => s.GetDetailForLearnerAsync(It.IsAny<Guid>(), It.IsAny<Guid>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Submit_WithClaims_ReturnsOkWithResult()
    {
        var request = new SubmitExamAnswersRequest(Guid.NewGuid(), new List<SubmittedAnswer>());
        var expected = new ExamResultResponse(_examId, Passed: true, Score: 100);
        _examService.Setup(s => s.SubmitAsync(_learnerId, It.IsAny<SubmitExamAnswersRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(expected);

        var result = await ControllerWithClaims(_learnerId.ToString()).Submit(_examId, request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
    }

    [Fact]
    public async Task Submit_UsesRouteIdOverBodyExamId()
    {
        var bodyExamId = Guid.NewGuid();
        var request = new SubmitExamAnswersRequest(bodyExamId, new List<SubmittedAnswer>());
        _examService.Setup(s => s.SubmitAsync(_learnerId, It.IsAny<SubmitExamAnswersRequest>(), It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ExamResultResponse(_examId, Passed: false, Score: 0));

        await ControllerWithClaims(_learnerId.ToString()).Submit(_examId, request);

        _examService.Verify(s => s.SubmitAsync(
            _learnerId,
            It.Is<SubmitExamAnswersRequest>(r => r.ExamId == _examId),
            It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Submit_WithoutClaims_Returns401()
    {
        var request = new SubmitExamAnswersRequest(_examId, new List<SubmittedAnswer>());

        var result = await ControllerWithoutClaims().Submit(_examId, request);

        Assert.IsType<UnauthorizedResult>(result);
        _examService.Verify(s => s.SubmitAsync(It.IsAny<Guid>(), It.IsAny<SubmitExamAnswersRequest>(), It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task Middleware_MapsForbiddenExceptionTo403Problem()
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new ForbiddenException("Module is locked for this learner."));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status403Forbidden, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("\"status\":403", body);
    }

    [Fact]
    public async Task Middleware_MapsNotFoundExceptionTo404Problem()
    {
        var middleware = new ExceptionHandlingMiddleware(
            _ => throw new NotFoundException("Exam was not found."));
        var context = new DefaultHttpContext();
        context.Response.Body = new MemoryStream();

        await middleware.InvokeAsync(context);

        Assert.Equal(StatusCodes.Status404NotFound, context.Response.StatusCode);
        Assert.Equal("application/problem+json", context.Response.ContentType);
        context.Response.Body.Position = 0;
        var body = await new StreamReader(context.Response.Body).ReadToEndAsync();
        Assert.Contains("\"status\":404", body);
    }

    [Fact]
    public void Json_SerializesDetailWithoutIsCorrect()
    {
        var detail = Detail(_examId, Guid.NewGuid());

        var json = JsonSerializer.Serialize(detail);

        Assert.DoesNotContain("IsCorrect", json, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("First", json);
        Assert.Contains("A1", json);
    }
}
