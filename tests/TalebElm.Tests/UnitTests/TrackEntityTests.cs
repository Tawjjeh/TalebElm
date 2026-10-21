using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;

namespace TalebElm.Tests.UnitTests;

public class TrackEntityTests
{
    [Fact]
    public void NewTrack_HasDraftStatusAndEmptyCollections()
    {
        var track = new Track();

        Assert.Equal(TrackStatus.Draft, track.Status);
        Assert.Equal(string.Empty, track.Name);
        Assert.Equal(string.Empty, track.Description);
        Assert.Empty(track.Modules);
        Assert.Empty(track.TrackEnrollments);
    }
}
