using FluentValidation;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Infrastructure.Services;

public class TrackService(
    IUnitOfWork unitOfWork,
    IValidator<CreateTrackRequest> validator) : ITrackService
{
    public async Task<IReadOnlyList<TrackResponse>> GetAllAsync()
    {
        var tracks = await unitOfWork.Tracks.GetAllAsync();

        if (tracks is null || tracks.Count == 0)
            return Array.Empty<TrackResponse>();

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
}