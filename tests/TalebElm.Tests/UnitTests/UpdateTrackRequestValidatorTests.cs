using TalebElm.Application.DTOs;
using TalebElm.Application.Validators;

namespace TalebElm.Application.UnitTests.Validators;

public class UpdateTrackRequestValidatorTests
{
    private readonly UpdateTrackRequestValidator _validator = new();

    [Fact]
    public void Valid_Name_And_Description_Pass()
    {
        var request = new UpdateTrackRequest("Backend .NET", "Learn ASP.NET Core");

        var result = _validator.Validate(request);

        Assert.True(result.IsValid);
        Assert.Empty(result.Errors);
    }

    [Fact]
    public void Name_And_Description_Boundary_Lengths_Are_Enforced()
    {
        var atLimit = new UpdateTrackRequest(new string('n', 150), new string('d', 1000));
        Assert.True(_validator.Validate(atLimit).IsValid);

        var overName = new UpdateTrackRequest(new string('n', 151), new string('d', 1000));
        Assert.False(_validator.Validate(overName).IsValid);

        var overDescription = new UpdateTrackRequest(new string('n', 150), new string('d', 1001));
        Assert.False(_validator.Validate(overDescription).IsValid);
    }

    [Theory]
    [InlineData("", "Description")]
    [InlineData("Name", "")]
    public void Empty_Name_Or_Description_Fails(string name, string description)
    {
        var request = new UpdateTrackRequest(name, description);

        var result = _validator.Validate(request);

        Assert.False(result.IsValid);
    }
}
