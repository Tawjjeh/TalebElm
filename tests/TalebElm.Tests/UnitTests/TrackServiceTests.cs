using FluentValidation;
using FluentValidation.Results;
using Moq;
using TalebElm.Application.DTOs;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Interfaces;
using TalebElm.Application.Services.Concrete;
using NotFoundException = TalebElm.Domain.Exceptions.NotFoundException;

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
            .Setup(u => u.Tracks.AddAsync(It.IsAny<Track>(), It.IsAny<CancellationToken>()))
            .Callback<Track, CancellationToken>((track, _) =>
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

    [Fact]
    public async Task GetPagedAsync_With25SeededTracks_Page2ReturnsExpectedSliceAndTotal()
    {
        var allTracks = Enumerable.Range(1, 25)
            .Select(i => new Track
            {
                Id = Guid.NewGuid(),
                Name = $"Track {i}",
                Description = $"Description {i}",
                Status = TrackStatus.Published
            })
            .ToList();

        var page2Slice = allTracks.Skip(5).Take(5).ToList();

        _unitOfWork
            .Setup(u => u.Tracks.GetPagedAsync(2, 5, true, It.IsAny<CancellationToken>()))
            .ReturnsAsync((page2Slice, 25));

        var result = await _sut.GetPagedAsync(page: 2, pageSize: 5);

        Assert.NotNull(result);
        Assert.Equal(2, result.Page);
        Assert.Equal(5, result.PageSize);
        Assert.Equal(25, result.TotalCount);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(page2Slice[0].Id, result.Items[0].Id);
        Assert.Equal(page2Slice[0].Name, result.Items[0].Name);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    public async Task GetPagedAsync_WhenPageLessThan1_ThrowsValidationException(int invalidPage)
    {
        var ex = await Assert.ThrowsAsync<TalebElm.Domain.Exceptions.ValidationException>(
            () => _sut.GetPagedAsync(page: invalidPage, pageSize: 20));

        Assert.Equal("Page must be greater than or equal to 1.", ex.Message);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-5)]
    [InlineData(101)]
    [InlineData(500)]
    public async Task GetPagedAsync_WhenPageSizeOutOfRange_ThrowsValidationException(int invalidPageSize)
    {
        var ex = await Assert.ThrowsAsync<TalebElm.Domain.Exceptions.ValidationException>(
            () => _sut.GetPagedAsync(page: 1, pageSize: invalidPageSize));

        Assert.Equal("PageSize must be between 1 and 100.", ex.Message);
    }


    [Fact]
    public async Task GetAllAsync_ForwardsCancellationTokenToRepository()
    {
        using var cts = new CancellationTokenSource();
        _unitOfWork
            .Setup(u => u.Tracks.GetAllAsync(cts.Token))
            .ReturnsAsync([]);

        await _sut.GetAllAsync(cts.Token);

        _unitOfWork.Verify(u => u.Tracks.GetAllAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task CreateAsync_ForwardsCancellationTokenToValidatorAndUnitOfWork()
    {
        using var cts = new CancellationTokenSource();
        var request = new CreateTrackRequest("Backend .NET", "Learn ASP.NET Core");

        _validator
            .Setup(v => v.ValidateAsync(request, cts.Token))
            .ReturnsAsync(new ValidationResult());

        _unitOfWork
            .Setup(u => u.Tracks.AddAsync(It.IsAny<Track>(), cts.Token))
            .Returns(Task.CompletedTask);

        _unitOfWork
            .Setup(u => u.SaveChangesAsync(cts.Token))
            .ReturnsAsync(1);

        await _sut.CreateAsync(request, cts.Token);

        _validator.Verify(v => v.ValidateAsync(request, cts.Token), Times.Once);
        _unitOfWork.Verify(u => u.Tracks.AddAsync(It.IsAny<Track>(), cts.Token), Times.Once);
        _unitOfWork.Verify(u => u.SaveChangesAsync(cts.Token), Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_WhenTrackIsPublished_ReturnsDetailWithModulesInOrder()
    {
        var trackId = Guid.NewGuid();
        var track = new Track
        {
            Id = trackId,
            Name = "Backend .NET",
            Description = "Learn ASP.NET Core",
            Status = TrackStatus.Published
        };
        var modules = new List<Module>
        {
            new() { Id = Guid.NewGuid(), TrackId = trackId, Title = "M2", Summary = "Second", Order = 2 },
            new() { Id = Guid.NewGuid(), TrackId = trackId, Title = "M1", Summary = "First", Order = 1 },
            new() { Id = Guid.NewGuid(), TrackId = trackId, Title = "M3", Summary = "Third", Order = 3 }
        };

        _unitOfWork
            .Setup(u => u.Tracks.GetByIdAsync(trackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(track);
        _unitOfWork
            .Setup(u => u.Modules.GetByTrackIdAsync(trackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(modules);

        var result = await _sut.GetByIdAsync(trackId);

        Assert.Equal(trackId, result.Id);
        Assert.Equal("Backend .NET", result.Name);
        Assert.Equal((int)TrackStatus.Published, result.Status);
        Assert.Equal(["M1", "M2", "M3"], result.Modules.Select(m => m.Title));
    }

    [Fact]
    public async Task GetByIdAsync_WhenTrackIsMissing_ThrowsNotFoundException()
    {
        var trackId = Guid.NewGuid();

        _unitOfWork
            .Setup(u => u.Tracks.GetByIdAsync(trackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync((Track?)null);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(trackId));

        _unitOfWork.Verify(
            u => u.Modules.GetByTrackIdAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()),
            Times.Never);
    }

    [Theory]
    [InlineData(TrackStatus.Draft)]
    [InlineData(TrackStatus.Archived)]
    public async Task GetByIdAsync_WhenTrackIsNotPublished_ThrowsNotFoundException(TrackStatus status)
    {
        var trackId = Guid.NewGuid();
        var track = new Track
        {
            Id = trackId,
            Name = "Backend .NET",
            Description = "Learn ASP.NET Core",
            Status = status
        };

        _unitOfWork
            .Setup(u => u.Tracks.GetByIdAsync(trackId, It.IsAny<CancellationToken>()))
            .ReturnsAsync(track);

        await Assert.ThrowsAsync<NotFoundException>(() => _sut.GetByIdAsync(trackId));
    }

    [Fact]
    public async Task GetByIdAsync_ForwardsCancellationTokenToRepositories()
    {
        using var cts = new CancellationTokenSource();
        var trackId = Guid.NewGuid();
        var track = new Track
        {
            Id = trackId,
            Name = "Backend .NET",
            Description = "Learn ASP.NET Core",
            Status = TrackStatus.Published
        };

        _unitOfWork
            .Setup(u => u.Tracks.GetByIdAsync(trackId, cts.Token))
            .ReturnsAsync(track);
        _unitOfWork
            .Setup(u => u.Modules.GetByTrackIdAsync(trackId, cts.Token))
            .ReturnsAsync([]);

        await _sut.GetByIdAsync(trackId, cts.Token);

        _unitOfWork.Verify(u => u.Tracks.GetByIdAsync(trackId, cts.Token), Times.Once);
        _unitOfWork.Verify(u => u.Modules.GetByTrackIdAsync(trackId, cts.Token), Times.Once);
    }
}
