using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class TrackStatusEnumTests
{
    [Fact]
    public void TrackStatus_HasStablePersistedValues()
    {
        Assert.Equal(3, Enum.GetValues<TrackStatus>().Length);
        Assert.Equal(0, (int)TrackStatus.Draft);
        Assert.Equal(1, (int)TrackStatus.Published);
        Assert.Equal(2, (int)TrackStatus.Archived);
    }
}
