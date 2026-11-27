
using FluentValidation;
using TalebElm.Application.DTOs;
using TalebElm.Application.Ordering;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;
using ValidationException = TalebElm.Domain.Exceptions.ValidationException;

namespace TalebElm.Application.Services.Concrete;

public class ModuleService(
    IUnitOfWork unitOfWork,
    IValidator<CreateModuleRequest> createValidator,
    IValidator<UpdateModuleRequest> updateValidator) : IModuleService
{
    public async Task<IReadOnlyList<ModuleResponse>> GetByTrackAsync(
        Guid trackId,
        CancellationToken ct = default)
    {
        if (await unitOfWork.Tracks.GetByIdAsync(trackId, ct) is null)
        {
            throw new NotFoundException(
                $"Track with id '{trackId}' was not found.");
        }

        var modules = await unitOfWork.Modules.GetByTrackIdAsync(
            trackId, ct);

        return modules
            .OrderBy(module => module.Order)
            .Select(module => new ModuleResponse(
                module.Id,
                module.Title,
                module.Summary,
                module.Order,
                module.TrackId))
            .ToList();
    }

    public async Task<ModuleResponse> GetByIdAsync(
        Guid id,
        CancellationToken ct = default)
    {
        var module = await unitOfWork.Modules.GetByIdAsync(id, ct);

        if (module is null)
        {
            throw new NotFoundException(
                $"Module with id '{id}' was not found.");
        }

        return new ModuleResponse(
            module.Id,
            module.Title,
            module.Summary,
            module.Order,
            module.TrackId);
    }

    public async Task<ModuleResponse> CreateAsync(CreateModuleRequest request, CancellationToken ct = default)
    {
        var validationResult = await createValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        if (await unitOfWork.Tracks.GetByIdAsync(request.TrackId, ct) is null)
            throw new NotFoundException($"Track with id '{request.TrackId}' was not found.");

        var siblings = (await unitOfWork.Modules.GetByTrackIdForUpdateAsync(request.TrackId, ct)).ToList();
        SiblingOrdering.PrepareInsert(siblings, request.Order);

        var module = new Module
        {
            Title = request.Title,
            Summary = request.Summary,
            Order = request.Order,
            TrackId = request.TrackId
        };

        await unitOfWork.Modules.AddAsync(module, ct);
        await unitOfWork.SaveChangesAsync(ct);

        return new ModuleResponse(module.Id, module.Title, module.Summary, module.Order, module.TrackId);
    }

    public async Task<ModuleResponse> UpdateAsync(Guid id, UpdateModuleRequest request, CancellationToken ct = default)
    {
        var validationResult = await updateValidator.ValidateAsync(request, ct);

        if (!validationResult.IsValid)
            throw new FluentValidation.ValidationException(validationResult.Errors);

        var module = await unitOfWork.Modules.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Module with id '{id}' was not found.");

        // Cross-track moves are out of scope: a module stays in its track.
        if (request.TrackId != module.TrackId)
            throw new Domain.Exceptions.ValidationException(
                $"Module with id '{id}' belongs to another track and cannot be moved to track '{request.TrackId}'.");

        module.Title = request.Title;
        module.Summary = request.Summary;

        if (request.Order == module.Order)
        {
            await unitOfWork.SaveChangesAsync(ct);
        }
        else
        {
            var siblings = (await unitOfWork.Modules.GetByTrackIdForUpdateAsync(module.TrackId, ct)).ToList();
            var tracked = siblings.Single(m => m.Id == module.Id);
            // The title/summary changes above are on the same tracked entity,
            // so MoveAsync's saves inside the transaction persist them too.
            await SiblingOrdering.MoveAsync(unitOfWork, siblings, tracked, request.Order, ct);
        }

        return new ModuleResponse(module.Id, module.Title, module.Summary, module.Order, module.TrackId);
    }

    public async Task DeleteAsync(Guid id, CancellationToken ct = default)
    {
        var module = await unitOfWork.Modules.GetByIdAsync(id, ct)
            ?? throw new NotFoundException($"Module with id '{id}' was not found.");

        // Conflict guards first (#251): a blocked delete must not modify
        // anything, and the database's Restrict FKs are only the backstop.
        var lessons = await unitOfWork.Lessons.GetByModuleIdAsync(id, ct);
        if (lessons.Count > 0)
            throw new ConflictException($"Module with id '{id}' has Lessons and cannot be deleted.");

        if (await unitOfWork.Exams.GetByModuleIdAsync(id, ct) is not null)
            throw new ConflictException($"Module with id '{id}' has an Exam and cannot be deleted.");

        if (await unitOfWork.UserProgresses.ExistsForModuleAsync(id, ct))
            throw new ConflictException($"Module with id '{id}' has learner progress and cannot be deleted.");

        var siblings = (await unitOfWork.Modules.GetByTrackIdForUpdateAsync(module.TrackId, ct)).ToList();
        await unitOfWork.Modules.RemoveAsync(module, ct);
        siblings.Remove(module);
        SiblingOrdering.PrepareDelete(siblings, module.Order);
        await unitOfWork.SaveChangesAsync(ct);
    }
}
