using FluentValidation;
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Validators;

public class UpdateExamQuestionRequestValidator : AbstractValidator<UpdateExamQuestionRequest>
{
    public UpdateExamQuestionRequestValidator()
    {
        RuleFor(x => x.Prompt)
            .NotEmpty().WithMessage("Exam.Question.Prompt.Required");

        RuleFor(x => x.Order)
            .GreaterThanOrEqualTo(1).WithMessage("Exam.Question.Order.Invalid");

        RuleFor(x => x.Points)
            .GreaterThan(0).WithMessage("Exam.Question.Points.Invalid");
    }
}
