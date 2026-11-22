using FluentValidation.TestHelper;
using TalebElm.Application.DTOs.Requests;
using TalebElm.Application.Validators;

namespace TalebElm.Tests.UnitTests;

public class RegisterRequestValidatorTest
{
    private readonly RegisterRequestValidator _validator = new();

    [Fact]
    public async Task Validate_ValidRequest_ShouldNotHaveValidationErrors()
    {
        var request = new RegisterRequestDTO(
            "Ahmed",
            "ahmed@example.com",
            "Ahmed123");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public async Task Validate_EmptyName_ShouldHaveValidationError()
    {
        var request = new RegisterRequestDTO(
            "",
            "ahmed@example.com",
            "Ahmed123");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_NameLongerThan100Characters_ShouldHaveValidationError()
    {
        var request = new RegisterRequestDTO(
            new string('A', 101),
            "ahmed@example.com",
            "Ahmed123");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Name);
    }

    [Fact]
    public async Task Validate_EmptyEmail_ShouldHaveValidationError()
    {
        var request = new RegisterRequestDTO(
            "Ahmed",
            "",
            "Ahmed123");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_InvalidEmail_ShouldHaveValidationError()
    {
        var request = new RegisterRequestDTO(
            "Ahmed",
            "invalid-email",
            "Ahmed123");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_EmailLongerThan256Characters_ShouldHaveValidationError()
    {
        var email = new string('a', 245) + "@example.com";

        var request = new RegisterRequestDTO(
            "Ahmed",
            email,
            "Ahmed123");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_PasswordShorterThan8Characters_ShouldHaveValidationError()
    {
        var request = new RegisterRequestDTO(
            "Ahmed",
            "ahmed@example.com",
            "Abc1234");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_PasswordWithoutLetter_ShouldHaveValidationError()
    {
        var request = new RegisterRequestDTO(
            "Ahmed",
            "ahmed@example.com",
            "12345678");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_PasswordWithoutDigit_ShouldHaveValidationError()
    {
        var request = new RegisterRequestDTO(
            "Ahmed",
            "ahmed@example.com",
            "Abcdefgh");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_PasswordExceedsMaxLength_ShouldHaveValidationError()
    {
        var request = new RegisterRequestDTO(
            "Ahmed",
            "ahmed@example.com",
            new string('a', 128) + "1");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldHaveValidationErrorFor(x => x.Password);
    }

    [Fact]
    public async Task Validate_EmailOfExactly256Characters_ShouldNotHaveValidationError()
    {
        var email = new string('a', 244) + "@example.com";

        var request = new RegisterRequestDTO(
            "Ahmed",
            email,
            "Ahmed123");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public async Task Validate_PasswordOfExactly128Characters_ShouldNotHaveValidationError()
    {
        var request = new RegisterRequestDTO(
            "Ahmed",
            "ahmed@example.com",
            new string('a', 127) + "1");

        var result = await _validator.TestValidateAsync(request);

        result.ShouldNotHaveValidationErrorFor(x => x.Password);
    }
}
