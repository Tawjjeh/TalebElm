using FluentValidation.TestHelper;
using TalebElm.Application.DTOs;
using TalebElm.Application.Validators;


namespace TalebElm.Tests.UnitTests;

public class UpdateModuleRequestValidatorTests
{
    private readonly UpdateModuleRequestValidator _validator = new();

    private static UpdateModuleRequest ValidRequest()
    {
        return new UpdateModuleRequest("Intro", "Summary", 1, Guid.NewGuid());
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
            .WithErrorMessage("Module.Title.Required");
    }

    [Fact]
    public void Title_TooLong_HasError()
    {
        var request = ValidRequest() with { Title = new string('a', 151) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Module.Title.TooLong");
    }

    [Fact]
    public void Summary_TooLong_HasError()
    {
        var request = ValidRequest() with { Summary = new string('a', 501) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Summary)
            .WithErrorMessage("Module.Summary.TooLong");
    }

    [Fact]
    public void Order_Zero_HasError()
    {
        var request = ValidRequest() with { Order = 0 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Order)
            .WithErrorMessage("Module.Order.Invalid");
    }

    [Fact]
    public void TrackId_Empty_HasError()
    {
        var request = ValidRequest() with { TrackId = Guid.Empty };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.TrackId)
            .WithErrorMessage("Module.TrackId.Required");
    }
}