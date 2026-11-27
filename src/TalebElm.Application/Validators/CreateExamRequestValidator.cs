using FluentValidation;
using TalebElm.Application.DTOs;

namespace TalebElm.Application.Validators;

public class CreateExamRequestValidator : AbstractValidator<CreateExamRequest>
{
    public CreateExamRequestValidator()
    {
        RuleFor(x => x.ModuleId)
             .NotEmpty().WithMessage("Exam.ModuleId.Required");

        RuleFor(x => x.Title)
            .NotEmpty().WithMessage("Exam.Title.Required")
            .MaximumLength(150).WithMessage("Exam.Title.TooLong");

        RuleFor(x => x.PassThreshold)
            .InclusiveBetween(1, 100).WithMessage("Exam.PassThreshold.Invalid");

        RuleFor(x => x.Questions)
            .NotNull().WithMessage("Exam.Questions.Required")
            .NotEmpty().WithMessage("Exam.Questions.Required");

        RuleFor(x => x.Questions)
            .Must(questions => questions.Select(q => q.Order).Distinct().Count() == questions.Count)
            .WithMessage("Exam.Question.Order.Duplicate")
            .When(x => x.Questions is not null);

        RuleForEach(x => x.Questions).ChildRules(question =>
        {
            question.RuleFor(q => q.Prompt)
                .NotEmpty().WithMessage("Exam.Question.Prompt.Required");

            question.RuleFor(q => q.Order)
                .GreaterThanOrEqualTo(1).WithMessage("Exam.Question.Order.Invalid");

            question.RuleFor(q => q.Points)
                .GreaterThan(0).WithMessage("Exam.Question.Points.Invalid");

            question.RuleFor(q => q.Options)
                .NotNull().WithMessage("Exam.Question.Options.Required")
                .NotEmpty().WithMessage("Exam.Question.Options.Required");

            question.RuleFor(q => q.Options)
                .Must(options => options.Count >= 2)
                .WithMessage("Exam.Question.Options.TooFew")
                .When(q => q.Options is not null);

            question.RuleFor(q => q.Options)
                .Must(options => options.Count(o => o.IsCorrect) == 1)
                .WithMessage("Exam.Question.Options.ExactlyOneCorrect")
                .When(q => q.Options is not null);

            question.RuleForEach(q => q.Options).ChildRules(option =>
            {
                option.RuleFor(o => o.Text)
                    .NotEmpty().WithMessage("Exam.Option.Text.Required");
            });
        });
    }
}
