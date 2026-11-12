using TalebElm.Application.DTOs;
using TalebElm.Application.Validators;

namespace TalebElm.Application.UnitTests.Validators;

public class CreateTrackRequestValidatorTests
{
    private readonly CreateTrackRequestValidator _validator;

    public CreateTrackRequestValidatorTests()
    {
        _validator = new CreateTrackRequestValidator();
    }
    [Theory]
    [InlineData(".NET Basic Level 0", "Introduction to .NET through C# basics.")]
    [InlineData("Frontend Web Development", "Learn HTML, CSS, JavaScript, and modern UI frameworks.")]
    [InlineData("Data Structures & Algorithms", "Master  problem-solving Topics and complexity analysis.")]
    [InlineData("Cloud Computing Essentials", " Basic Concepts of networks link with command lines (Linux) ,Cloud Architecture ,Services,Security .")]
    public void Valid_Name_And_Description_Pass(string name, string description)
    {
        // Arrange
        var request = new CreateTrackRequest(name, description);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Name_And_Description_Boundary_Lengths_Are_Enforced()
    {
        // The limits are inclusive: the exact maximum passes and one character over fails.
        var atLimit = new CreateTrackRequest(new string('n', 150), new string('d', 1000));
        Assert.True(_validator.Validate(atLimit).IsValid);

        var overName = new CreateTrackRequest(new string('n', 151), new string('d', 1000));
        var nameResult = _validator.Validate(overName);
        Assert.False(nameResult.IsValid);
        Assert.Contains(nameResult.Errors, e =>
            e.PropertyName == nameof(CreateTrackRequest.Name) &&
            e.ErrorMessage == "Track.Name.TooLong");

        var overDescription = new CreateTrackRequest(new string('n', 150), new string('d', 1001));
        var descriptionResult = _validator.Validate(overDescription);
        Assert.False(descriptionResult.IsValid);
        Assert.Contains(descriptionResult.Errors, e =>
            e.PropertyName == nameof(CreateTrackRequest.Description) &&
            e.ErrorMessage == "Track.Description.TooLong");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingOrBlankName_Fails(string? name)
    {
        // Arrange
        var request = new CreateTrackRequest(name!, "Introduction to .NET through C# basics.");

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(CreateTrackRequest.Name) &&
            e.ErrorMessage == "Track Name is required");
    }

    [Theory]
    [InlineData(null)]
    [InlineData("")]
    [InlineData("   ")]
    public void MissingOrBlankDescription_Fails(string? description)
    {
        // Arrange
        var request = new CreateTrackRequest(".NET Basic Level 0", description!);

        // Act
        var result = _validator.Validate(request);

        // Assert
        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e =>
            e.PropertyName == nameof(CreateTrackRequest.Description) &&
            e.ErrorMessage == "Track Description is required");
    }
}