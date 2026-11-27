using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;

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
    /// Creates a new track (Draft).
    /// </summary>
    /// <param name="request">The track creation details.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="201">Returns the created track.</response>
    /// <response code="400">The request is invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Admin.</response>
    [HttpPost]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(TrackResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    public async Task<IActionResult> Post([FromBody] CreateTrackRequest request, CancellationToken ct = default)
    {
        var response = await trackService.CreateAsync(request, ct);
        return Created($"/api/tracks/{response.Id}", response);
    }

    /// <summary>
    /// Updates a track's name and description. Only Draft tracks can be
    /// edited; editing a Published or Archived track returns 400.
    /// </summary>
    /// <param name="id">The track identifier.</param>
    /// <param name="request">The new name and description.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="200">Returns the updated track.</response>
    /// <response code="400">The request is invalid or the track is not Draft.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Admin.</response>
    /// <response code="404">The track does not exist.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(TrackResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateTrackRequest request, CancellationToken ct = default)
    {
        var response = await trackService.UpdateAsync(id, request, ct);
        return Ok(response);
    }

    /// <summary>
    /// Deletes a track. Returns 409 when the track still has Modules,
    /// enrollments, or learner progress.
    /// </summary>
    /// <param name="id">The track identifier.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="204">The track was deleted.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Admin.</response>
    /// <response code="404">The track does not exist.</response>
    /// <response code="409">The track still has Modules, enrollments, or progress.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await trackService.DeleteAsync(id, ct);
        return NoContent();
    }
}
