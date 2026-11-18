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
    public async Task Validate_EmailLongerThan256Characters_ShouldHaveValidationError()
    {
        var email = new string('a', 245) + "@example.com";

        var request = new Application.DTOs.Requests.LoginRequestDTO(
            email,
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

    [Fact]
    public async Task Validate_PasswordExceedsMaxLength_ShouldHaveValidationError()
    {
        var request = new Application.DTOs.Requests.LoginRequestDTO(
            "ahmed@example.com",
            new string('a', 128) + "1");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_EmailOfExactly256Characters_ShouldNotHaveValidationError()
    {
        var email = new string('a', 244) + "@example.com";

        var request = new Application.DTOs.Requests.LoginRequestDTO(
            email,
            "Ahmed123");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_PasswordOfExactly128Characters_ShouldNotHaveValidationError()
    {
        var request = new Application.DTOs.Requests.LoginRequestDTO(
            "ahmed@example.com",
            new string('a', 127) + "1");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
