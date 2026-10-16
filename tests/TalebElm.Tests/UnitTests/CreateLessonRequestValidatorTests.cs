using FluentValidation.TestHelper;
using TalebElm.Application.DTOs;
using TalebElm.Application.Validators;


namespace TalebElm.Tests.UnitTests;

public class CreateLessonRequestValidatorTests
{
    private readonly CreateLessonRequestValidator _validator = new();

    private static CreateLessonRequest ValidRequest()
    {
        return new CreateLessonRequest("Intro", Guid.NewGuid(), 1);
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
            .WithErrorMessage("Lesson.Title.Required");
    }

    [Fact]
    public void Title_TooLong_HasError()
    {
        var request = ValidRequest() with { Title = new string('a', 151) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Lesson.Title.TooLong");
    }

    [Fact]
    public void ModuleId_Empty_HasError()
    {
        var request = ValidRequest() with { ModuleId = Guid.Empty };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ModuleId)
            .WithErrorMessage("Lesson.ModuleId.Required");
    }

    [Fact]
    public void Order_Zero_HasError()
    {
        var request = ValidRequest() with { Order = 0 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Order)
            .WithErrorMessage("Lesson.Order.Invalid");
    }
}