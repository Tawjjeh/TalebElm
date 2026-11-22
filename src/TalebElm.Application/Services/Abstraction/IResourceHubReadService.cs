using TalebElm.Application.DTOs;

namespace TalebElm.Application.Services.Abstraction;

public interface IResourceHubReadService
{
    Task<IReadOnlyList<TechnologySummaryResponse>> GetTechnologiesAsync(
        Guid? parentId,
        CancellationToken ct = default);

    Task<TechnologyDetailResponse> GetTechnologyBySlugPathAsync(
        IReadOnlyList<string> slugPath,
        ResourceFilterParameters filters,
        CancellationToken ct = default);

    Task<PagedResult<ResourceResponse>> GetResourcesAsync(
        Guid? technologyId,
        ResourceFilterParameters filters,
        CancellationToken ct = default);
}
