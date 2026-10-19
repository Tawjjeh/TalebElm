using FluentValidation;
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Validators;

public class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    public CreateTrackRequestValidator()
    {
        RuleFor(request => request.Name).NotEmpty().WithMessage("Track Name is required");
        RuleFor(request => request.Description)
                .NotEmpty()
                .WithMessage("Track Description is required");
    }
}
