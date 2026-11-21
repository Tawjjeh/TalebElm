using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Api.Models;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;

namespace TalebElm.Api.Controllers;

[ApiController]
[AllowAnonymous]
[Route("api/hub")]
public sealed class HubController(IResourceHubReadService hubReadService) : ControllerBase
{
    /// <summary>Lists top-level technology categories.</summary>
    [HttpGet("technologies")]
    [ProducesResponseType(typeof(IReadOnlyList<TechnologySummaryResponse>), StatusCodes.Status200OK)]
    public async Task<ActionResult<IReadOnlyList<TechnologySummaryResponse>>> GetTechnologies()
    {
        var items = await hubReadService.GetTechnologiesAsync(
            null, HttpContext.RequestAborted);
        return Ok(items);
    }

    /// <summary>Gets a technology by its nested slug path, with approved resources.</summary>
    [HttpGet("technologies/{**slugPath}")]
    [ProducesResponseType(typeof(TechnologyDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<TechnologyDetailResponse>> GetTechnologyBySlugPath(
        [FromRoute] string slugPath,
        [FromQuery] HubResourceQuery query)
    {
        var segments = slugPath.Split('/', StringSplitOptions.RemoveEmptyEntries);
        var detail = await hubReadService.GetTechnologyBySlugPathAsync(
            segments, query.ToFilters(), HttpContext.RequestAborted);
        return Ok(detail);
    }

    /// <summary>Gets approved resources for a specific topic.</summary>
    [HttpGet("topics/{id:guid}/resources")]
    [ProducesResponseType(typeof(PagedResult<ResourceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<ActionResult<PagedResult<ResourceResponse>>> GetTopicResources(
        [FromRoute] Guid id,
        [FromQuery] HubResourceQuery query)
    {
        var resources = await hubReadService.GetResourcesAsync(
            id, query.ToFilters(), HttpContext.RequestAborted);
        return Ok(resources);
    }

    /// <summary>Searches approved resources across all technologies.</summary>
    [HttpGet("resources")]
    [ProducesResponseType(typeof(PagedResult<ResourceResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<PagedResult<ResourceResponse>>> GetResources(
        [FromQuery] HubResourceQuery query)
    {
        var resources = await hubReadService.GetResourcesAsync(
            null, query.ToFilters(), HttpContext.RequestAborted);
        return Ok(resources);
    }
}
