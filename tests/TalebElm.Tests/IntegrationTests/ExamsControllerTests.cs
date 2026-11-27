using System.Reflection;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Moq;
using TalebElm.Api.Controllers;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;

namespace TalebElm.Tests.IntegrationTests;

public class ExamsControllerTests
{
    private readonly Mock<IExamService> _examService = new();

    private static ExamQuestionResponse QuestionResponse(Guid id) => new(
        id, "What is 2 + 2?", 1, 10,
        [new ExamOptionResponse(Guid.NewGuid(), "4"), new ExamOptionResponse(Guid.NewGuid(), "5")]);

    [Fact]
    public async Task Put_ReturnsOkWithExamResponse()
    {
        var examId = Guid.NewGuid();
        var request = new UpdateExamRequest("New Title", 80);
        var expected = new ExamResponse(examId, "New Title", 80, Guid.NewGuid());
        _examService.Setup(s => s.UpdateAsync(examId, request, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new ExamsController(_examService.Object);

        var result = await controller.Put(examId, request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        _examService.Verify(s => s.UpdateAsync(examId, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PutQuestion_ReturnsOkWithQuestionResponse()
    {
        var examId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var request = new UpdateExamQuestionRequest("New prompt", 2, 20);
        var expected = QuestionResponse(questionId);
        _examService.Setup(s => s.UpdateQuestionAsync(examId, questionId, request, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new ExamsController(_examService.Object);

        var result = await controller.PutQuestion(examId, questionId, request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        _examService.Verify(s => s.UpdateQuestionAsync(examId, questionId, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task AddOption_ReturnsOkWithQuestionResponse()
    {
        var examId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var request = new AddExamOptionRequest("6", false);
        var expected = QuestionResponse(questionId);
        _examService.Setup(s => s.AddOptionAsync(examId, questionId, request, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new ExamsController(_examService.Object);

        var result = await controller.AddOption(examId, questionId, request);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        _examService.Verify(s => s.AddOptionAsync(examId, questionId, request, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task RemoveOption_ReturnsOkWithQuestionResponse()
    {
        var examId = Guid.NewGuid();
        var questionId = Guid.NewGuid();
        var optionId = Guid.NewGuid();
        var expected = QuestionResponse(questionId);
        _examService.Setup(s => s.RemoveOptionAsync(examId, questionId, optionId, It.IsAny<CancellationToken>())).ReturnsAsync(expected);

        var controller = new ExamsController(_examService.Object);

        var result = await controller.RemoveOption(examId, questionId, optionId);

        var ok = Assert.IsType<OkObjectResult>(result);
        Assert.Same(expected, ok.Value);
        _examService.Verify(s => s.RemoveOptionAsync(examId, questionId, optionId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task Delete_ReturnsNoContent()
    {
        var examId = Guid.NewGuid();
        var controller = new ExamsController(_examService.Object);

        var result = await controller.Delete(examId);

        Assert.IsType<NoContentResult>(result);
        _examService.Verify(s => s.DeleteAsync(examId, It.IsAny<CancellationToken>()), Times.Once);
    }

    [Theory]
    [InlineData(nameof(ExamsController.Put))]
    [InlineData(nameof(ExamsController.PutQuestion))]
    [InlineData(nameof(ExamsController.AddOption))]
    [InlineData(nameof(ExamsController.RemoveOption))]
    [InlineData(nameof(ExamsController.Delete))]
    public void MaintenanceEndpoints_AreGatedByAdminOnlyPolicy(string methodName)
    {
        // The 403 for a non-Admin caller is enforced by the authorization policy,
        // which direct controller invocation does not run; assert the gate is declared.
        var method = typeof(ExamsController).GetMethod(methodName)!;
        var authorize = method.GetCustomAttribute<AuthorizeAttribute>();

        Assert.NotNull(authorize);
        Assert.Equal("AdminOnly", authorize.Policy);
    }
}
