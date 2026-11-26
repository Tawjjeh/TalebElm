using System.ComponentModel;
using System.Runtime.InteropServices.Marshalling;
using System.Runtime.InteropServices.ObjectiveC;
using FluentValidation;
using Microsoft.AspNetCore.Identity;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public class TrackService(
    IUnitOfWork unitOfWork,
    IValidator<CreateTrackRequest> validator) : ITrackService
{
    public async Task<IReadOnlyList<TrackResponse>> GetAllAsync()
    {
        var tracks = await unitOfWork.Tracks.GetAllAsync();

        if (tracks.Count == 0)
            return [];

        return tracks
            .Select(track => new TrackResponse(
                track.Id,
                track.Name,
                track.Description,
                (int)track.Status))
            .ToList();
    }

    public async Task<TrackResponse> CreateAsync(CreateTrackRequest request)
    {
        var validationResult = await validator.ValidateAsync(request);

        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        var track = new Track
        {
            Name = request.Name,
            Description = request.Description,
            Status = TrackStatus.Draft
        };

        await unitOfWork.Tracks.AddAsync(track);

        await unitOfWork.SaveChangesAsync();

        return new TrackResponse(
            track.Id,
            track.Name,
            track.Description,
            (int)track.Status);
    }

    public async Task<PagedTracksResponse> GetPagedAsync(
        int page = 1,
        int pageSize = 20,
        CancellationToken cancellationToken = default)
    {
        if (page < 1)
            throw new TalebElm.Domain.Exceptions.ValidationException("Page must be greater than or equal to 1.");

        if (pageSize < 1 || pageSize > 100)
            throw new TalebElm.Domain.Exceptions.ValidationException("PageSize must be between 1 and 100.");

        var (tracks, totalCount) = await unitOfWork.Tracks.GetPagedAsync(page, pageSize, publishedOnly: true, cancellationToken);

        var items = tracks
            .Select(t => new TrackResponse(t.Id, t.Name, t.Description, (int)t.Status))
            .ToList();

        return new PagedTracksResponse(items, page, pageSize, totalCount);
    }

    public async Task PublishAsync(Guid id, CancellationToken ct = default)
    {
        var track = await unitOfWork.Tracks.GetByIdAsync(id, ct);
        if (track == null)
            throw new NotFoundException("Track not found");

        if (track.Status == TrackStatus.Draft)
        {
            var hasModules = await unitOfWork.Modules.GetByTrackIdAsync(id, ct);
            if (!hasModules.Any())
                throw new FluentValidation.ValidationException("A track must contain at least one module before publishing.");
        }
        else if(track.Status != TrackStatus.Archived) 
        {
            throw new FluentValidation
                    .ValidationException("The track cannot be published from its current status.");
        }
        track.Status = TrackStatus.Published;

        await unitOfWork.SaveChangesAsync(ct);
    }

    public async Task ArchiveAsync(Guid id, CancellationToken ct = default)
    {
        var track = await unitOfWork.Tracks
                                    .GetByIdAsync(id, ct);

        if (track is null)
            throw new NotFoundException("Track not found.");

        if (track.Status != TrackStatus.Published)
        {
            throw new FluentValidation.ValidationException(
                "Only published tracks can be archived.");
        }

        track.Status = TrackStatus.Archived;

        await unitOfWork.SaveChangesAsync(ct);
    }


}
