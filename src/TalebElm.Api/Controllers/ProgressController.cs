using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;

namespace TalebElm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgressController(IUserProgressService progressService) : ControllerBase
    {
        [HttpGet("me")]
        public IActionResult GetMyProgress() => StatusCode(StatusCodes.Status501NotImplemented);

        [HttpGet("me/tracks/{trackId}")]
        public IActionResult GetByTrack(Guid trackId) => StatusCode(StatusCodes.Status501NotImplemented);

        /// <summary>
        /// Manually unlocks a Module for a learner (Admin support action).
        /// </summary>
        /// <param name="request">The learner and module to unlock.</param>
        /// <param name="ct">Cancellation token.</param>
        /// <response code="200">Returns the learner's progress for the module, now unlocked.</response>
        /// <response code="401">The admin is not authenticated.</response>
        /// <response code="403">The caller is not an Admin.</response>
        /// <response code="404">The learner (as a Student) or the module was not found.</response>
        [HttpPost("unlock")]
        [Authorize(Policy = "AdminOnly")]
        [ProducesResponseType(typeof(ProgressResponse), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status401Unauthorized)]
        [ProducesResponseType(StatusCodes.Status403Forbidden)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<IActionResult> Unlock([FromBody] AdminUnlockModuleRequest request, CancellationToken ct = default)
        {
            var adminId = GetAdminId();
            if (adminId is null)
                return Unauthorized();

            var result = await progressService.AdminUnlockAsync(adminId.Value, request, ct);
            return Ok(result);
        }

        private Guid? GetAdminId()
        {
            var claim = ControllerContext?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(claim, out var adminId) ? adminId : null;
        }
    }
}
