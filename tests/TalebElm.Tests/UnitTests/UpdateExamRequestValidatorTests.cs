using FluentValidation.TestHelper;
using TalebElm.Application.DTOs;
using TalebElm.Application.Validators;

namespace TalebElm.Tests.UnitTests;

public class UpdateExamRequestValidatorTests
{
    private readonly UpdateExamRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(new UpdateExamRequest("Final Exam", 60));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Title_Empty_HasError()
    {
        var result = _validator.TestValidate(new UpdateExamRequest("", 60));

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Exam.Title.Required");
    }

    [Fact]
    public void Title_TooLong_HasError()
    {
        var result = _validator.TestValidate(new UpdateExamRequest(new string('x', 151), 60));

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Exam.Title.TooLong");
    }

    [Theory]
    [InlineData(0)]
    [InlineData(101)]
    public void PassThreshold_OutOfRange_HasError(int threshold)
    {
        var result = _validator.TestValidate(new UpdateExamRequest("Final Exam", threshold));

        result.ShouldHaveValidationErrorFor(x => x.PassThreshold)
            .WithErrorMessage("Exam.PassThreshold.Invalid");
    }

    [Theory]
    [InlineData(1)]
    [InlineData(100)]
    public void PassThreshold_Boundaries_AreValid(int threshold)
    {
        var result = _validator.TestValidate(new UpdateExamRequest("Final Exam", threshold));

        result.ShouldNotHaveValidationErrorFor(x => x.PassThreshold);
    }
}
