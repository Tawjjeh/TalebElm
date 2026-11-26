using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using TalebElm.Domain.Enums;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TracksController(ITrackService trackService) : ControllerBase
{
    /// <summary>
    /// Retrieves a page of available tracks.
    /// </summary>
    /// <param name="page">The 1-based page number. Defaults to 1.</param>
    /// <param name="pageSize">The number of tracks per page. Defaults to 20.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="200">Returns the requested page of tracks.</response>
    /// <response code="400">The page or page size is outside the allowed range.</response>
    [HttpGet]
    [ProducesResponseType(typeof(PagedTracksResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var response = await trackService.GetPagedAsync(page, pageSize, ct);
        return Ok(response);
    }

    /// <summary>
    /// Creates a new track.
    /// </summary>
    /// <param name="request">The track creation details.</param>
    /// <response code="200">The endpoint accepted the request.</response>
    /// <response code="501">The endpoint is not implemented yet.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Post([FromBody] CreateTrackRequest request)
        => throw new NotImplementedException(
            "Creating a track via this endpoint is not implemented yet.");

    [HttpPatch("{trackId}/publish")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<ActionResult> PublishAsync(Guid trackId, CancellationToken cancellationToken)
    {
        if (!User.IsInRole(nameof(UserRole.Admin)))
            return Forbid();

        await trackService.PublishAsync(trackId, cancellationToken);
        return Ok();
    } 

    [HttpPatch("{trackId}/Archive")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<ActionResult> ArchiveAsync(Guid trackId, CancellationToken cancellationToken)
    {
        if (!User.IsInRole(nameof(UserRole.Admin)))
            return Forbid();

        await trackService.ArchiveAsync(trackId, cancellationToken);
        return Ok();
    }
}
