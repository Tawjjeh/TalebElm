using FluentValidation.TestHelper;
using TalebElm.Application.DTOs;
using TalebElm.Application.Validators;
using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class UpdateLessonRequestValidatorTests
{
    private readonly UpdateLessonRequestValidator _validator = new();

    private static UpdateLessonRequest ValidRequest()
    {
        return new UpdateLessonRequest("Intro", Guid.NewGuid(), 1, "Lesson body", LessonType.Text);
    }

    [Fact]
    public void Content_Empty_HasError()
    {
        var request = ValidRequest() with { Content = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Content)
            .WithErrorMessage("Lesson.Content.Required");
    }

    [Fact]
    public void Type_InvalidEnumValue_HasError()
    {
        var request = ValidRequest() with { Type = (LessonType)99 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Type)
            .WithErrorMessage("Lesson.Type.Invalid");
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