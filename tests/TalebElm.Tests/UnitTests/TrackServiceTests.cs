using Moq;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Services;

namespace TalebElm.Tests.UnitTests;

public class TrackServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly TrackService _sut;

    public TrackServiceTests()
    {
        _sut = new TrackService(_unitOfWork.Object);
    }

    [Fact]
    public async Task GetAllAsync_WhenTracksExist_ReturnsMappedTrackResponses()
    {
        var tracks = new List<Track>
        {
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Backend .NET",
                Description = "Learn ASP.NET Core",
                Status = TrackStatus.Published
            },
            new()
            {
                Id = Guid.NewGuid(),
                Name = "Frontend",
                Description = "Learn frontend development",
                Status = TrackStatus.Archived
            }
        };

        _unitOfWork
            .Setup(u => u.Tracks.GetAllAsync())
            .ReturnsAsync(tracks);

        var result = await _sut.GetAllAsync();

        Assert.Equal(2, result.Count);

        Assert.Equal(tracks[0].Id, result[0].Id);
        Assert.Equal(tracks[0].Name, result[0].Name);
        Assert.Equal(tracks[0].Description, result[0].Description);
        Assert.Equal((int)tracks[0].Status, result[0].Status);

        Assert.Equal(tracks[1].Id, result[1].Id);
        Assert.Equal(tracks[1].Name, result[1].Name);
        Assert.Equal(tracks[1].Description, result[1].Description);
        Assert.Equal((int)tracks[1].Status, result[1].Status);
    }

    [Fact]
    public async Task GetAllAsync_WhenNoTracksExist_ReturnsEmptyCollection()
    {
        _unitOfWork
            .Setup(u => u.Tracks.GetAllAsync())
            .ReturnsAsync(new List<Track>());

        var result = await _sut.GetAllAsync();

        Assert.NotNull(result);
        Assert.Empty(result);
    }
}