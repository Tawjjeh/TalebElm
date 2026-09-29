
using FluentValidation;
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Validators;

public class CreateTrackRequestValidator : AbstractValidator<CreateTrackRequest>
{
    public CreateTrackRequestValidator()
    {
        RuleFor(CRT => CRT.Name).NotEmpty().WithMessage("Track Name is required"); //CRT->CreateRequestTrack
        RuleFor(CRT => CRT.Description)
                .NotEmpty()
                .WithMessage("Track Description is required");
    }
}