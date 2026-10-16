using FluentValidation;
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Validators;

public class UpdateLessonRequestValidator : AbstractValidator<UpdateLessonRequest>
{
    public UpdateLessonRequestValidator()
    {
        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Lesson.Title.Required")
            .MaximumLength(150).WithMessage("Lesson.Title.TooLong");

        RuleFor(x => x.ModuleId)
            .NotEmpty().WithMessage("Lesson.ModuleId.Required");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(1).WithMessage("Lesson.Order.Invalid");
    }
}