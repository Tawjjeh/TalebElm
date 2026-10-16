using FluentValidation.TestHelper;
using TalebElm.Application.DTOs;
using TalebElm.Application.Validators;


namespace TalebElm.Tests.UnitTests;

public class CreateExamRequestValidatorTests
{
    private readonly CreateExamRequestValidator _validator = new();

    private static CreateExamRequest ValidRequest()
    {
        return new CreateExamRequest(Guid.NewGuid(), "Final Exam", 60);
    }

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Title_Empty_HasError()
    {
        var request = ValidRequest() with { Title = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Exam.Title.Required");
    }

    [Fact]
    public void PassThreshold_Zero_HasError()
    {
        var request = ValidRequest() with { PassThreshold = 0 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PassThreshold)
            .WithErrorMessage("Exam.PassThreshold.Invalid");
    }

    [Fact]
    public void PassThreshold_Over100_HasError()
    {
        var request = ValidRequest() with { PassThreshold = 101 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PassThreshold)
            .WithErrorMessage("Exam.PassThreshold.Invalid");
    }

    [Fact]
    public void PassThreshold_1And100_AreValid()
    {
        var low = _validator.TestValidate(ValidRequest() with { PassThreshold = 1 });
        var high = _validator.TestValidate(ValidRequest() with { PassThreshold = 100 });

        low.ShouldNotHaveValidationErrorFor(x => x.PassThreshold);
        high.ShouldNotHaveValidationErrorFor(x => x.PassThreshold);
    }
}