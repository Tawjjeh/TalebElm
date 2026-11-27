using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModulesController(IExamService examService) : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => throw new NotImplementedException("Listing modules is not implemented yet.");

    /// <summary>
    /// Retrieves the learner-safe view of a module's exam.
    /// </summary>
    /// <param name="moduleId">The unique identifier of the module.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the module's exam without the answer key.</response>
    /// <response code="401">The learner is not authenticated.</response>
    /// <response code="403">The module is locked for the learner.</response>
    /// <response code="404">The module has no exam.</response>
    [HttpGet("{moduleId:guid}/exam")]
    [Authorize(Policy = "StudentOnly")]
    [ProducesResponseType(typeof(ExamDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> GetExam(Guid moduleId, CancellationToken ct = default)
    {
        var learnerId = GetLearnerId();
        if (learnerId is null)
            return Unauthorized();

        var exam = await examService.GetDetailByModuleForLearnerAsync(learnerId.Value, moduleId, ct);
        return Ok(exam);
    }

    private Guid? GetLearnerId()
    {
        var claim = ControllerContext?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var learnerId) ? learnerId : null;
    }
}
