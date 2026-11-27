using FluentValidation;
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Validators;

public class UpdateExamRequestValidator : AbstractValidator<UpdateExamRequest>
{
    public UpdateExamRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Exam.Title.Required")
            .MaximumLength(150).WithMessage("Exam.Title.TooLong");

        RuleFor(x => x.PassThreshold)
            .InclusiveBetween(1, 100).WithMessage("Exam.PassThreshold.Invalid");
    }
}
