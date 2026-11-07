using FluentValidation;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
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
            throw new ValidationException(validationResult.Errors);

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
}
