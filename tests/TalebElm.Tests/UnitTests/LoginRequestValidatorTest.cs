using FluentValidation.TestHelper;
using TalebElm.Application.Validators;

namespace TalebElm.Tests.UnitTests;

public class LoginRequestValidatorTest
{
    private readonly LoginRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
    {
        var request = new Application.DTOs.Requests.LoginRequestDTO(
            "ahmed@example.com",
            "123");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_EmptyEmail_ShouldHaveValidationError()
    {
        var request = new Application.DTOs.Requests.LoginRequestDTO(
            "",
            "Ahmed123");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_InvalidEmail_ShouldHaveValidationError()
    {
        var request = new Application.DTOs.Requests.LoginRequestDTO(
            "invalid-email",
            "Ahmed123");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_EmptyPassword_ShouldHaveValidationError()
    {
        var request = new Application.DTOs.Requests.LoginRequestDTO(
            "ahmed@example.com",
            "");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_ShortPassword_ShouldNotHaveValidationErrors()
    {
        var request = new Application.DTOs.Requests.LoginRequestDTO(
            "ahmed@example.com",
            "123");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

}
