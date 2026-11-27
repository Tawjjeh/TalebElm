using FluentValidation.TestHelper;
using TalebElm.Application.DTOs;
using TalebElm.Application.Validators;

namespace TalebElm.Tests.UnitTests;

public class AddExamOptionRequestValidatorTests
{
    private readonly AddExamOptionRequestValidator _validator = new();

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(new AddExamOptionRequest("4", true));

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Text_Empty_HasError()
    {
        var result = _validator.TestValidate(new AddExamOptionRequest("", false));

        result.ShouldHaveValidationErrorFor(x => x.Text)
            .WithErrorMessage("Exam.Option.Text.Required");
    }
}
