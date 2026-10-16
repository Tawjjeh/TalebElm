
using FluentValidation;
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Validators;

public class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    public CreateTrackRequestValidator()
    {
        RuleFor(t => t.Name).NotEmpty().WithMessage("Track Name is required"); //t => for track
        RuleFor(t => t.Description)
                .NotEmpty()
                .WithMessage("Track Description is required");
    }
}