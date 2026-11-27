using FluentValidation.TestHelper;
using TalebElm.Application.DTOs;
using TalebElm.Application.Validators;

namespace TalebElm.Tests.UnitTests;

public class CreateExamRequestValidatorTests
{
    private readonly CreateExamRequestValidator _validator = new();

    private static CreateExamRequest ValidRequest()
    {
        return new CreateExamRequest(Guid.NewGuid(), "Final Exam", 60, ValidQuestions());
    }

    private static IReadOnlyList<CreateExamQuestionRequest> ValidQuestions() =>
        new List<CreateExamQuestionRequest>
        {
            new("What is 2 + 2?", 1, 10, new List<CreateExamOptionRequest>
            {
                new("4", IsCorrect: true),
                new("5", IsCorrect: false)
            })
        };

    private static CreateExamRequest ValidRequestWithQuestions(IReadOnlyList<CreateExamQuestionRequest> questions) =>
        new(Guid.NewGuid(), "Final Exam", 60, questions);


    [Fact]
    public void ModuleId_Empty_HasError()
    {
        var request = ValidRequest() with { ModuleId = Guid.Empty };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.ModuleId)
            .WithErrorMessage("Exam.ModuleId.Required");
    }

    [Fact]
    public void ValidRequest_HasNoErrors()
    {
        var result = _validator.TestValidate(ValidRequest());

        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Title_Empty_HasError()
    {
        var request = ValidRequest() with { Title = "" };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Exam.Title.Required");
    }

    [Fact]
    public void Title_150Characters_IsValid()
    {
        var request = ValidRequest() with { Title = new string('a', 150) };

        var result = _validator.TestValidate(request);
        result.ShouldNotHaveValidationErrorFor(x => x.Title);
    }

    [Fact]
    public void Title_151Characters_HasError()
    {
        var request = ValidRequest() with { Title = new string('a', 151) };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.Title)
            .WithErrorMessage("Exam.Title.TooLong");
    }

    [Fact]
    public void PassThreshold_Zero_HasError()
    {
        var request = ValidRequest() with { PassThreshold = 0 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PassThreshold)
            .WithErrorMessage("Exam.PassThreshold.Invalid");
    }

    [Fact]
    public void PassThreshold_Over100_HasError()
    {
        var request = ValidRequest() with { PassThreshold = 101 };

        var result = _validator.TestValidate(request);

        result.ShouldHaveValidationErrorFor(x => x.PassThreshold)
            .WithErrorMessage("Exam.PassThreshold.Invalid");
    }

    [Fact]
    public void PassThreshold_1And100_AreValid()
    {
        var low = _validator.TestValidate(ValidRequest() with { PassThreshold = 1 });
        var high = _validator.TestValidate(ValidRequest() with { PassThreshold = 100 });

        low.ShouldNotHaveValidationErrorFor(x => x.PassThreshold);
        high.ShouldNotHaveValidationErrorFor(x => x.PassThreshold);
    }

    [Fact]
    public void Questions_Null_HasError()
    {
        var result = _validator.TestValidate(ValidRequest() with { Questions = null! });

        result.ShouldHaveValidationErrorFor(x => x.Questions)
            .WithErrorMessage("Exam.Questions.Required");
    }

    [Fact]
    public void Questions_Empty_HasError()
    {
        var result = _validator.TestValidate(ValidRequestWithQuestions(new List<CreateExamQuestionRequest>()));

        result.ShouldHaveValidationErrorFor(x => x.Questions)
            .WithErrorMessage("Exam.Questions.Required");
    }

    [Fact]
    public void Question_PromptEmpty_HasError()
    {
        var questions = new List<CreateExamQuestionRequest>
        {
            new("", 1, 10, new List<CreateExamOptionRequest> { new("4", true), new("5", false) })
        };

        var result = _validator.TestValidate(ValidRequestWithQuestions(questions));

        result.ShouldHaveValidationErrorFor("Questions[0].Prompt")
            .WithErrorMessage("Exam.Question.Prompt.Required");
    }

    [Fact]
    public void Question_OrderZero_HasError()
    {
        var questions = new List<CreateExamQuestionRequest>
        {
            new("What is 2 + 2?", 0, 10, new List<CreateExamOptionRequest> { new("4", true), new("5", false) })
        };

        var result = _validator.TestValidate(ValidRequestWithQuestions(questions));

        result.ShouldHaveValidationErrorFor("Questions[0].Order")
            .WithErrorMessage("Exam.Question.Order.Invalid");
    }

    [Fact]
    public void Question_DuplicateOrder_HasError()
    {
        var questions = new List<CreateExamQuestionRequest>
        {
            new("Q1", 1, 10, new List<CreateExamOptionRequest> { new("A", true), new("B", false) }),
            new("Q2", 1, 10, new List<CreateExamOptionRequest> { new("C", true), new("D", false) })
        };

        var result = _validator.TestValidate(ValidRequestWithQuestions(questions));

        result.ShouldHaveValidationErrorFor(x => x.Questions)
            .WithErrorMessage("Exam.Question.Order.Duplicate");
    }

    [Fact]
    public void Question_PointsZero_HasError()
    {
        var questions = new List<CreateExamQuestionRequest>
        {
            new("What is 2 + 2?", 1, 0, new List<CreateExamOptionRequest> { new("4", true), new("5", false) })
        };

        var result = _validator.TestValidate(ValidRequestWithQuestions(questions));

        result.ShouldHaveValidationErrorFor("Questions[0].Points")
            .WithErrorMessage("Exam.Question.Points.Invalid");
    }

    [Fact]
    public void Question_OneOption_HasError()
    {
        var questions = new List<CreateExamQuestionRequest>
        {
            new("What is 2 + 2?", 1, 10, new List<CreateExamOptionRequest> { new("4", true) })
        };

        var result = _validator.TestValidate(ValidRequestWithQuestions(questions));

        result.ShouldHaveValidationErrorFor("Questions[0].Options")
            .WithErrorMessage("Exam.Question.Options.TooFew");
    }

    [Fact]
    public void Question_NoCorrectOption_HasError()
    {
        var questions = new List<CreateExamQuestionRequest>
        {
            new("What is 2 + 2?", 1, 10, new List<CreateExamOptionRequest> { new("4", false), new("5", false) })
        };

        var result = _validator.TestValidate(ValidRequestWithQuestions(questions));

        result.ShouldHaveValidationErrorFor("Questions[0].Options")
            .WithErrorMessage("Exam.Question.Options.ExactlyOneCorrect");
    }

    [Fact]
    public void Question_TwoCorrectOptions_HasError()
    {
        var questions = new List<CreateExamQuestionRequest>
        {
            new("What is 2 + 2?", 1, 10, new List<CreateExamOptionRequest> { new("4", true), new("5", true) })
        };

        var result = _validator.TestValidate(ValidRequestWithQuestions(questions));

        result.ShouldHaveValidationErrorFor("Questions[0].Options")
            .WithErrorMessage("Exam.Question.Options.ExactlyOneCorrect");
    }

    [Fact]
    public void Option_TextEmpty_HasError()
    {
        var questions = new List<CreateExamQuestionRequest>
        {
            new("What is 2 + 2?", 1, 10, new List<CreateExamOptionRequest> { new("", true), new("5", false) })
        };

        var result = _validator.TestValidate(ValidRequestWithQuestions(questions));

        result.ShouldHaveValidationErrorFor("Questions[0].Options[0].Text")
            .WithErrorMessage("Exam.Option.Text.Required");
    }
}
