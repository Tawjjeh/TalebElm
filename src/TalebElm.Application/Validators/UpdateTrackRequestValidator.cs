using FluentValidation;
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Validators;

public class UpdateTrackRequestValidator : AbstractValidator<UpdateTrackRequest>
{
    public UpdateTrackRequestValidator()
    {
        RuleFor(request => request.Name)
                .NotEmpty().WithMessage("Track Name is required")
                .MaximumLength(150).WithMessage("Track.Name.TooLong");
        RuleFor(request => request.Description)
                .NotEmpty()
                .WithMessage("Track Description is required")
                .MaximumLength(1000).WithMessage("Track.Description.TooLong");
    }
}
