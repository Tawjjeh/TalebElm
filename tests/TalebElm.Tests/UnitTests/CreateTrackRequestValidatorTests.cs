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
        // Arrange: exact limits pass, one character over fails
        var description = new string('d', 1000);
        var atNameLimit = new CreateTrackRequest(new string('n', 150), description);
        var overNameLimit = new CreateTrackRequest(new string('n', 151), description);
        var atDescriptionLimit = new CreateTrackRequest("Track", new string('d', 1000));
        var overDescriptionLimit = new CreateTrackRequest("Track", new string('d', 1001));

        // Act & Assert
        Assert.True(_validator.Validate(atNameLimit).IsValid);
        Assert.True(_validator.Validate(atDescriptionLimit).IsValid);

        var nameErrors = _validator.Validate(overNameLimit).Errors.Select(e => e.ErrorMessage).ToList();
        Assert.Equal(new[] { "Track.Name.TooLong" }, nameErrors);

        var descriptionErrors = _validator.Validate(overDescriptionLimit).Errors.Select(e => e.ErrorMessage).ToList();
        Assert.Equal(new[] { "Track.Description.TooLong" }, descriptionErrors);
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