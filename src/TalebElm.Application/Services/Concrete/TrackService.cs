using FluentValidation;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public class TrackService(
    IUnitOfWork unitOfWork,
    IValidator<CreateTrackRequest> validator,
    IValidator<UpdateTrackRequest> updateValidator) : ITrackService
{
    public async Task<IReadOnlyList<TrackResponse>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        var tracks = await unitOfWork.Tracks.GetAllAsync(cancellationToken);

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

    public async Task<TrackResponse> CreateAsync(CreateTrackRequest request, CancellationToken cancellationToken = default)
    {
        var validationResult = await validator.ValidateAsync(request, cancellationToken);

        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        var track = new Track
        {
            Name = request.Name,
            Description = request.Description,
            Status = TrackStatus.Draft
        };

        await unitOfWork.Tracks.AddAsync(track, cancellationToken);

        await unitOfWork.SaveChangesAsync(cancellationToken);

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

    public async Task<TrackResponse> UpdateAsync(Guid id, UpdateTrackRequest request, CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        var track = await unitOfWork.Tracks.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Track with id '{id}' was not found.");

        // Edit policy (see #266): a track that already left Draft is live for
        // learners, so its name and description are frozen. Status changes go
        // through the separate PATCH /api/tracks/{id}/status workflow.
        if (track.Status != TrackStatus.Draft)
            throw new TalebElm.Domain.Exceptions.ValidationException(
                $"Only Draft tracks can be edited; track '{id}' is {track.Status}.");

        track.Name = request.Name;
        track.Description = request.Description;

        await unitOfWork.SaveChangesAsync(ct);

        return new TrackResponse(track.Id, track.Name, track.Description, (int)track.Status);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var track = await unitOfWork.Tracks.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Track with id '{id}' was not found.");

        // Conflict guards first (#251): a blocked delete must not modify
        // anything, and the database's Restrict FKs are only the backstop.
        var modules = await unitOfWork.Modules.GetByTrackIdAsync(id, ct);
        if (modules.Count > 0)
            throw new ConflictException($"Track with id '{id}' has Modules and cannot be deleted.");

        if (await unitOfWork.TrackEnrollments.ExistsForTrackAsync(id, ct))
            throw new ConflictException($"Track with id '{id}' has enrollments and cannot be deleted.");

        if (await unitOfWork.UserProgresses.ExistsForTrackAsync(id, ct))
            throw new ConflictException($"Track with id '{id}' has learner progress and cannot be deleted.");

        await unitOfWork.Tracks.RemoveAsync(track, ct);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
