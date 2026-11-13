
using FluentValidation;
using TalebElm.Application.DTOs.Requests;

namespace TalebElm.Application.Validators;

public class RegisterRequestValidator : AbstractValidator<RegisterRequestDTO>
{
    public RegisterRequestValidator()
    {
        RuleFor(x => x.Name)
            .NotEmpty()
            .MaximumLength(100);

        RuleFor(x => x.Email)
            .NotEmpty()
            .EmailAddress()
            .MaximumLength(256);

        RuleFor(x => x.Password)
            .MinimumLength(8)
            .Matches(@"[a-zA-Z]")
            .WithMessage("Password must contain at least one letter.")
            .Matches(@"[0-9]")
            .WithMessage("Password must contain at least one digit.");
    }
}
