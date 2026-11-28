using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Entities;
using TalebElm.Domain.Enums;
using TalebElm.Domain.Exceptions;
using TalebElm.Domain.Interfaces;

namespace TalebElm.Application.Services.Concrete;

public sealed class ResourceHubReadService(IUnitOfWork unitOfWork) : IResourceHubReadService
{
    public async Task<IReadOnlyList<TechnologySummaryResponse>> GetTechnologiesAsync(
        Guid? parentId,
        CancellationToken ct = default)
    {
        var technologies = await unitOfWork.Technologies.GetByParentIdAsync(parentId, ct);

        return await ToSummariesAsync(technologies, ct);
    }

    public async Task<TechnologyDetailResponse> GetTechnologyBySlugPathAsync(
        IReadOnlyList<string> slugPath,
        ResourceFilterParameters filters,
        CancellationToken ct = default)
    {
        var normalizedPath = NormalizeSlugPath(slugPath);
        ValidatePaging(filters);

        var technology = await unitOfWork.Technologies.GetBySlugPathAsync(normalizedPath, ct)
            ?? throw new NotFoundException("Technology path was not found.");

        var children = await unitOfWork.Technologies.GetByParentIdAsync(technology.Id, ct);
        var childrenResponses = await ToSummariesAsync(children, ct);
        var resources = await GetResourcesCoreAsync(technology.Id, filters, ct);

        return new TechnologyDetailResponse(
            technology.Id,
            technology.ParentId,
            technology.Name,
            technology.Slug,
            technology.Description,
            (int)technology.Kind,
            technology.Order,
            childrenResponses,
            resources);
    }

    public async Task<PagedResult<ResourceResponse>> GetResourcesAsync(
        Guid? technologyId,
        ResourceFilterParameters filters,
        CancellationToken ct = default)
    {
        ValidatePaging(filters);

        if (technologyId is not null)
        {
            var technology = await unitOfWork.Technologies.GetByIdAsync(technologyId.Value, ct);
            if (technology is null)
                throw new NotFoundException("Technology was not found.");
        }

        return await GetResourcesCoreAsync(technologyId, filters, ct);
    }

    private async Task<PagedResult<ResourceResponse>> GetResourcesCoreAsync(
        Guid? technologyId,
        ResourceFilterParameters filters,
        CancellationToken ct)
    {
        var (items, totalCount) = await unitOfWork.Resources.GetApprovedPagedAsync(
            technologyId,
            filters.SearchQuery?.Trim(),
            filters.Language,
            filters.Level is null ? null : (ResourceLevel)filters.Level.Value,
            filters.IsPaid,
            filters.Kind is null ? null : (ResourceKind)filters.Kind.Value,
            filters.PageNumber,
            filters.PageSize,
            ct);

        var totalPages = (int)Math.Ceiling(totalCount / (double)filters.PageSize);

        return new PagedResult<ResourceResponse>(
            items.Select(ToResource).ToList(),
            filters.PageNumber,
            filters.PageSize,
            totalCount,
            totalPages);
    }

    private static IReadOnlyList<string> NormalizeSlugPath(IReadOnlyList<string> slugPath)
    {
        if (slugPath is null || slugPath.Count == 0)
            throw new ValidationException("A technology slug path is required.");

        if (slugPath.Count > 3)
            throw new ValidationException("A technology slug path cannot exceed 3 segments.");

        var normalized = new List<string>(slugPath.Count);

        foreach (var slug in slugPath)
        {
            if (string.IsNullOrWhiteSpace(slug))
                throw new ValidationException("Technology slug path segments cannot be empty.");

            normalized.Add(slug.Trim().ToLowerInvariant());
        }

        return normalized;
    }

    private static void ValidatePaging(ResourceFilterParameters filters)
    {
        if (filters is null)
            throw new ValidationException("Resource filters are required.");

        if (filters.Level is not null && !Enum.IsDefined((ResourceLevel)filters.Level.Value))
            throw new ValidationException("Level must be a valid resource level.");

        if (filters.Kind is not null && !Enum.IsDefined((ResourceKind)filters.Kind.Value))
            throw new ValidationException("Kind must be a valid resource kind.");

        if (filters.PageNumber < 1)
            throw new ValidationException("Page must be greater than or equal to 1.");

        if (filters.PageSize < 1 || filters.PageSize > 100)
            throw new ValidationException("PageSize must be between 1 and 100.");
    }

    private async Task<IReadOnlyList<TechnologySummaryResponse>> ToSummariesAsync(
        IReadOnlyList<Technology> technologies,
        CancellationToken ct)
    {
        if (technologies.Count == 0)
            return [];

        var technologyIds = technologies.Select(t => t.Id).ToList();
        var childCounts = await unitOfWork.Technologies.GetChildCountsAsync(technologyIds, ct);
        var resourceCounts = await unitOfWork.Resources.GetApprovedResourceCountsAsync(technologyIds, ct);

        return technologies
            .Select(t => ToSummary(
                t,
                childCounts.GetValueOrDefault(t.Id),
                resourceCounts.GetValueOrDefault(t.Id)))
            .ToList();
    }

    private static TechnologySummaryResponse ToSummary(
        Technology technology,
        int childCount,
        int resourceCount)
        => new(
            technology.Id,
            technology.ParentId,
            technology.Name,
            technology.Slug,
            (int)technology.Kind,
            technology.Order,
            childCount,
            resourceCount);

    private static ResourceResponse ToResource(Resource resource)
        => new(
            resource.Id,
            resource.TechnologyId,
            resource.Title,
            (int)resource.Kind,
            resource.Url,
            resource.Citation,
            resource.Language,
            (int)resource.Level,
            resource.IsPaid,
            (int)resource.Status);
}
