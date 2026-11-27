using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModulesController(IModuleService moduleService) : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => throw new NotImplementedException("Listing modules is not implemented yet.");

    /// <summary>
    /// Creates a module in a track at the requested order, shifting siblings
    /// so Order stays contiguous. The route's track id is authoritative.
    /// </summary>
    /// <param name="trackId">The track identifier.</param>
    /// <param name="request">The module creation details.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="201">Returns the created module.</response>
    /// <response code="400">The request is invalid.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Admin.</response>
    /// <response code="404">The track does not exist.</response>
    [HttpPost("/api/tracks/{trackId:guid}/modules")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ModuleResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Post(Guid trackId, [FromBody] CreateModuleRequest request, CancellationToken ct = default)
    {
        var response = await moduleService.CreateAsync(request with { TrackId = trackId }, ct);
        return Created($"/api/modules/{response.Id}", response);
    }

    /// <summary>
    /// Updates a module's title, summary, and order within its track.
    /// Moving a module to another track is not supported.
    /// </summary>
    /// <param name="id">The module identifier.</param>
    /// <param name="request">The new title, summary, order, and track id.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="200">Returns the updated module.</response>
    /// <response code="400">The request is invalid or moves the module to another track.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Admin.</response>
    /// <response code="404">The module does not exist.</response>
    [HttpPut("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ModuleResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateModuleRequest request, CancellationToken ct = default)
    {
        var response = await moduleService.UpdateAsync(id, request, ct);
        return Ok(response);
    }

    /// <summary>
    /// Deletes a module and compacts the remaining siblings. Returns 409 when
    /// the module still has Lessons, an Exam, or learner progress.
    /// </summary>
    /// <param name="id">The module identifier.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="204">The module was deleted.</response>
    /// <response code="401">The caller is not authenticated.</response>
    /// <response code="403">The caller is not an Admin.</response>
    /// <response code="404">The module does not exist.</response>
    /// <response code="409">The module still has Lessons, an Exam, or progress.</response>
    [HttpDelete("{id:guid}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await moduleService.DeleteAsync(id, ct);
        return NoContent();
    }
}
