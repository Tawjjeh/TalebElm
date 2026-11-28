using System.Runtime.CompilerServices;
using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TracksController(
    ITrackService trackService,
    ITrackEnrollmentService trackEnrollmentService) : ControllerBase
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
    /// Retrieves a published track with its ordered modules.
    /// </summary>
    /// <param name="trackId">The unique identifier of the track.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="200">Returns the requested track.</response>
    /// <response code="401">The caller is not authenticated as a student.</response>
    /// <response code="404">The track does not exist or is not published.</response>
    [HttpGet("{trackId:guid}")]
    [Authorize(Policy = "StudentOnly")]
    [ProducesResponseType(typeof(TrackDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetById(Guid trackId, CancellationToken ct = default)
    {
        var response = await trackService.GetByIdAsync(trackId, ct);
        return Ok(response);
    }

    /// <summary>
    /// Enrolls the authenticated student in a track.
    /// </summary>
    /// <param name="trackId">The unique identifier of the track.</param>
    /// <response code="201">Returns the created enrollment.</response>
    /// <response code="401">The caller is not authenticated as a student.</response>
    /// <response code="404">The track does not exist.</response>
    /// <response code="409">The student is already enrolled in the track.</response>
    [HttpPost("{trackId:guid}/enrollment")]
    [Authorize(Policy = "StudentOnly")]
    [ProducesResponseType(typeof(EnrollmentResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Enroll(Guid trackId)
    {
        var userId = ControllerContext?.HttpContext?.User
            .FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (!Guid.TryParse(userId, out var studentId))
            return Unauthorized();

        var enrollment = await trackEnrollmentService.EnrollAsync(
            studentId,
            trackId,
            HttpContext.RequestAborted);

        return StatusCode(StatusCodes.Status201Created, enrollment);
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

}
