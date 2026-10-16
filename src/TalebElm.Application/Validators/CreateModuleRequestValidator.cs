using FluentValidation;
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Validators;

public class CreateModuleRequestValidator : AbstractValidator<CreateModuleRequest>
{
    public CreateModuleRequestValidator()
    {
        // Use error codes as messages so tests and clients can rely on them
        RuleFor(m => m.Title)
            .NotEmpty().WithMessage("Module.Title.Required")
            .MaximumLength(150).WithMessage("Module.Title.TooLong");

        RuleFor(m => m.Summary)
            .NotEmpty().WithMessage("Module.Summary.Required")
            .MaximumLength(500).WithMessage("Module.Summary.TooLong");

        RuleFor(m => m.Order)
            .GreaterThanOrEqualTo(1).WithMessage("Module.Order.Invalid");

        RuleFor(m => m.TrackId)
            .NotEmpty().WithMessage("Module.TrackId.Required");
    }
}