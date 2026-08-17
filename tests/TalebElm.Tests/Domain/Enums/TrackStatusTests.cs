using TalebElm.Domain.Enums;
using Xunit;

namespace TalebElm.Tests.Domain.Enums;

public class TrackStatusTests
{
    [Fact]
    public void TrackStatus_ShouldHaveExpectedValues()
    {
        Assert.Equal(0, (int)TrackStatus.Draft);
        Assert.Equal(1, (int)TrackStatus.Published);
        Assert.Equal(2, (int)TrackStatus.Archived);
    }

    [Theory]
    [InlineData(TrackStatus.Draft, "Draft")]
    [InlineData(TrackStatus.Published, "Published")]
    [InlineData(TrackStatus.Archived, "Archived")]
    public void TrackStatus_ShouldHaveExpectedNames(TrackStatus status, string expectedName)
    {
        Assert.Equal(expectedName, status.ToString());
    }
}
