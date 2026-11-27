using FluentValidation;
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Validators;

public class AddExamOptionRequestValidator : AbstractValidator<AddExamOptionRequest>
{
    public AddExamOptionRequestValidator()
    {
        RuleFor(x => x.Text)
            .NotEmpty().WithMessage("Exam.Option.Text.Required");
    }
}
