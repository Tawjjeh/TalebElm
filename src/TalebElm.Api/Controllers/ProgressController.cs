using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Application.Services.Abstraction;

namespace TalebElm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgressController(IUserProgressService  progressService) : ControllerBase
    {

        [HttpGet("me")]
        [Authorize(Policy = "StudentOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        public async Task<IActionResult> MyProgress()
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userId, out var result))
                return Unauthorized();

            var SendingInService = await progressService.GetMyProgressAsync(result, HttpContext.RequestAborted);

            return Ok(SendingInService);
        }

        [HttpGet("me/tracks/{trackId}")]
        [Authorize(Policy = "StudentOnly")]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> GetByTrack(Guid trackId)
        {
            string? userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

            if (!Guid.TryParse(userId, out var result))
                return Unauthorized();

            var sendingInService = await progressService.GetByTrackAsync(result, trackId, HttpContext.RequestAborted);

            return Ok(sendingInService);
        }
    }
}
