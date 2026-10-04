using FluentValidation;
using FluentValidation.Results;
using Moq;
using TalebElm.Application.DTOs;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Services;

namespace TalebElm.Tests.UnitTests;

public class TrackServiceTests
{
    private readonly Mock<IUnitOfWork> _unitOfWork = new();
    private readonly Mock<IValidator<CreateTrackRequest>> _validator = new();
    private readonly TrackService _sut;

    public TrackServiceTests()
    {
        _sut = new TrackService(
            _unitOfWork.Object,
            _validator.Object);
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

    [Fact]
    public async Task CreateAsync_WhenRequestIsValid_CreatesDraftTrackAndReturnsResponse()
    {
        var request = new CreateTrackRequest(
            "Backend .NET",
            "Learn ASP.NET Core");

        _validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(new ValidationResult());

        Track? addedTrack = null;

        _unitOfWork
            .Setup(u => u.Tracks.AddAsync(It.IsAny<Track>()))
            .Callback<Track>(track =>
            {
                addedTrack = track;
                track.Id = Guid.NewGuid();
            });

        _unitOfWork
            .Setup(u => u.SaveChangesAsync())
            .ReturnsAsync(1);

        var result = await _sut.CreateAsync(request);

        Assert.NotNull(addedTrack);
        Assert.Equal(request.Name, addedTrack!.Name);
        Assert.Equal(request.Description, addedTrack.Description);
        Assert.Equal(TrackStatus.Draft, addedTrack.Status);

        Assert.Equal(addedTrack.Id, result.Id);
        Assert.Equal(request.Name, result.Name);
        Assert.Equal(request.Description, result.Description);
        Assert.Equal((int)TrackStatus.Draft, result.Status);

        _unitOfWork.Verify(
            u => u.Tracks.AddAsync(It.IsAny<Track>()),
            Times.Once);

        _unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Once);
    }

    [Fact]
    public async Task CreateAsync_WhenRequestIsInvalid_DoesNotWriteToRepository()
    {
        var request = new CreateTrackRequest("", "");

        var validationResult = new ValidationResult(
        [
            new ValidationFailure("Name", "Track Name is required"),
            new ValidationFailure("Description", "Track Description is required")
        ]);

        _validator
            .Setup(v => v.ValidateAsync(
                request,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(validationResult);

        await Assert.ThrowsAsync<ValidationException>(
            () => _sut.CreateAsync(request));

        _unitOfWork.Verify(
            u => u.Tracks.AddAsync(It.IsAny<Track>()),
            Times.Never);

        _unitOfWork.Verify(
            u => u.SaveChangesAsync(),
            Times.Never);
    }
}