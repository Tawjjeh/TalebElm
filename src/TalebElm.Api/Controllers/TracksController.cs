
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Application.DTOs;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TracksController : ControllerBase
{
    /// <summary>
    /// Retrieves all available tracks.
    /// </summary>
    /// <response code="200">Returns the available tracks.</response>
    /// <response code="501">The endpoint is not implemented yet.</response>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<TrackResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Get()
        => throw new NotImplementedException(
            "Listing tracks is not implemented yet.");

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
