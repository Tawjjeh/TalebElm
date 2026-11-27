using FluentValidation.TestHelper;
using TalebElm.Application.DTOs;
using TalebElm.Application.Validators;

namespace TalebElm.Tests.UnitTests;

public class UpdateExamQuestionRequestValidatorTests
{
    private readonly UpdateExamQuestionRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(new UpdateExamQuestionRequest("What is 2 + 2?", 1, 10));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Prompt_Empty_HasError()
    {
        var result = _validator.TestValidate(new UpdateExamQuestionRequest("", 1, 10));

        result.ShouldHaveValidationErrorFor(x => x.Prompt)
            .WithErrorMessage("Exam.Question.Prompt.Required");
    }

    [Fact]
    public void Order_Zero_HasError()
    {
        var result = _validator.TestValidate(new UpdateExamQuestionRequest("Q", 0, 10));

        result.ShouldHaveValidationErrorFor(x => x.Order)
            .WithErrorMessage("Exam.Question.Order.Invalid");
    }

    [Fact]
    public void Points_Zero_HasError()
    {
        var result = _validator.TestValidate(new UpdateExamQuestionRequest("Q", 1, 0));

        result.ShouldHaveValidationErrorFor(x => x.Points)
            .WithErrorMessage("Exam.Question.Points.Invalid");
    }
}
