using System.Security.Cryptography;
using System.Transactions;
using FluentValidation;
using FluentValidation.Results;
using Microsoft.AspNetCore.Mvc.Filters;
using Moq;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Application.Services.Concrete;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;
using TalebElm.Infrastructure.Repositories;

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

        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
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
    public async Task PublishAsync_DraftTrackWithModules_PublishesTrack()
    {
        //. Arrange

        //. Track Status is draft
        var trackId = Guid.NewGuid();
        var track = new Track
        {
            Id = trackId,
            Status = TrackStatus.Draft
        };

        _unitOfWork.Setup(
                op => op.Tracks.GetByIdAsync(trackId, It.IsAny<CancellationToken>())
                )
            .ReturnsAsync(track);

        _unitOfWork.Setup(
            op => op.Modules.GetByTrackIdAsync(trackId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new List<Module>()
            {
                new Module()
            });

        //. Act
        await _sut.PublishAsync(trackId);

        //. Assert

        Assert.Equal(TrackStatus.Published, track.Status);

        _unitOfWork.Verify(
                op => op.SaveChangesAsync(It.IsAny<CancellationToken>()), times: Times.Once
              );

    }

    [Fact]
    public async Task PublishAsync_DraftTrackWithoutModules_ThrowsValidationException()
    {
        //. Arrange
        var trackId = Guid.NewGuid();
        var track = new Track
        {
            Id = trackId,
            Status = TrackStatus.Draft
        };

        _unitOfWork.Setup(
                op => op.Tracks.GetByIdAsync(trackId, It.IsAny<CancellationToken>())
                )
            .ReturnsAsync(track);

        _unitOfWork.Setup(
            op => op.Modules.GetByTrackIdAsync(trackId, It.IsAny<CancellationToken>())
            )
            .ReturnsAsync(new List<Module>());

        //. Act & Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _sut.PublishAsync(trackId)
            );

        _unitOfWork.Verify(op => op.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Never);
    }

    [Fact]
    public async Task PublishAsync_ArchivedTrack_PublishesTrack()
    {
        //. Arrange
        var trackId = Guid.NewGuid();
        var track = new Track
        {
            Id = trackId,
            Status = TrackStatus.Archived 
        };

        _unitOfWork
            .Setup(x => x.Tracks.GetByIdAsync(
                trackId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(track);

        //. Act
        await _sut.PublishAsync(trackId);

        //. Assert
        Assert.Equal(TrackStatus.Published, track.Status);

        _unitOfWork.Verify(op => op.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }

    [Fact]
    public async Task PublishAsync_NonExistingTrack_ThrowsNotFoundException()
    {
        //. Arrange
        var trackId = Guid.NewGuid();

        _unitOfWork
            .Setup(x => x.Tracks.GetByIdAsync(
                trackId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Track?)null);


        //. Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.PublishAsync(trackId, It.IsAny<CancellationToken>())
            );
    }

    [Fact]
    public async Task ArchiveAsync_PublishedTrack_ArchivesTrack()
    {
        //. Arrange
        var trackId = Guid.NewGuid();
        var track = new Track
        {
            Id = trackId,
            Status = TrackStatus.Published
        };

        _unitOfWork
            .Setup(x => x.Tracks.GetByIdAsync(
                trackId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(track);

        //. Act
        await _sut.ArchiveAsync(trackId);

        //. Assert
        Assert.Equal(TrackStatus.Archived, track.Status);

        _unitOfWork.Verify(op => op.SaveChangesAsync(It.IsAny<CancellationToken>()), Times.Once);
    }


    [Fact]
    public async Task ArchiveAsync_NonExistingTrack_ThrowsNotFoundException()
    {
        //. Arrange
        var trackId = Guid.NewGuid();

        _unitOfWork
            .Setup(x => x.Tracks.GetByIdAsync(
                trackId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync((Track?)null);


        //. Assert
        await Assert.ThrowsAsync<NotFoundException>(
            () => _sut.ArchiveAsync(trackId, It.IsAny<CancellationToken>())
            );
    }

    [Fact]
    public async Task PublishAsync_PublishedTrack_ThrowsValidationException()
    {
        //. Arrange
        var trackId = Guid.NewGuid();
        var track = new Track
        {
            Id = trackId,
            Status = TrackStatus.Published
        };

        _unitOfWork
            .Setup(x => x.Tracks.GetByIdAsync(
                trackId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(track);

        //. Assert
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _sut.PublishAsync(trackId, It.IsAny<CancellationToken>())
            );
    }

    [Fact]
    public async Task ArchiveAsync_DraftTrack_ThrowsValidationException()
    {
        //. Arrange
        var trackId = Guid.NewGuid();
        var track = new Track
        {
            Id = trackId,
            Status = TrackStatus.Draft
        };

        _unitOfWork
            .Setup(x => x.Tracks.GetByIdAsync(
                trackId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(track);

        //. Assert & Act
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _sut.ArchiveAsync(trackId, It.IsAny<CancellationToken>())
            );
    }

    [Fact]
    public async Task ArchiveAsync_ArchivedTrack_ThrowsValidationException()
    {
        //. Arrange
        var trackId = Guid.NewGuid();
        var track = new Track
        {
            Id = trackId,
            Status = TrackStatus.Archived
        };

        _unitOfWork
            .Setup(x => x.Tracks.GetByIdAsync(
                trackId,
                It.IsAny<CancellationToken>()))
            .ReturnsAsync(track);

        //. Assert & Act
        await Assert.ThrowsAsync<FluentValidation.ValidationException>(
            () => _sut.ArchiveAsync(trackId, It.IsAny<CancellationToken>())
            );
    }
}
