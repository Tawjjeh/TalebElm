using FluentValidation;
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Validators;

public class UpdateModuleRequestValidator : AbstractValidator<UpdateModuleRequest>
{
    public UpdateModuleRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Module.Title.Required")
            .MaximumLength(150).WithMessage("Module.Title.TooLong");

        RuleFor(x => x.Summary)
            .MaximumLength(500).WithMessage("Module.Summary.TooLong");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(1).WithMessage("Module.Order.Invalid");

        RuleFor(x => x.TrackId)
            .NotEmpty().WithMessage("Module.TrackId.Required");
    }
}