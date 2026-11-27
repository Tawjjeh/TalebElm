using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TalebElm.Application.DTOs;
using TalebElm.Application.Services.Abstraction;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExamsController(IExamService examService) : ControllerBase
{
    /// <summary>
    /// Retrieves the learner-safe view of an exam by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the exam.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the requested exam without the answer key.</response>
    /// <response code="401">The learner is not authenticated.</response>
    /// <response code="403">The exam belongs to a locked module.</response>
    /// <response code="404">The exam was not found.</response>
    [HttpGet("{id:guid}")]
    [Authorize(Policy = "StudentOnly")]
    [ProducesResponseType(typeof(ExamDetailResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Get(Guid id, CancellationToken ct = default)
    {
        var learnerId = GetLearnerId();
        if (learnerId is null)
            return Unauthorized();

        var exam = await examService.GetDetailForLearnerAsync(learnerId.Value, id, ct);
        return Ok(exam);
    }

    /// <summary>
    /// Creates a new exam.
    /// </summary>
    /// <param name="request">The exam creation details.</param>
    /// <response code="200">The endpoint accepted the request.</response>
    /// <response code="501">The endpoint is not implemented yet.</response>
    [HttpPost]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Post([FromBody] CreateExamRequest request)
        => throw new NotImplementedException(
            "Creating an exam via this endpoint is not implemented yet.");

    /// <summary>
    /// Submits an exam for evaluation.
    /// </summary>
    /// <param name="id">The unique identifier of the exam.</param>
    /// <param name="request">The submitted exam answers.</param>
    /// <param name="ct">Cancellation token.</param>
    /// <response code="200">Returns the exam result.</response>
    /// <response code="400">The answers are invalid.</response>
    /// <response code="401">The learner is not authenticated.</response>
    /// <response code="403">The exam belongs to a locked module.</response>
    /// <response code="404">The exam was not found.</response>
    [HttpPost("{id:guid}/submit")]
    [Authorize(Policy = "StudentOnly")]
    [ProducesResponseType(typeof(ExamResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Submit(Guid id, [FromBody] SubmitExamAnswersRequest request, CancellationToken ct = default)
    {
        var learnerId = GetLearnerId();
        if (learnerId is null)
            return Unauthorized();

        // The route id is authoritative; a body ExamId that disagrees is ignored.
        var result = await examService.SubmitAsync(learnerId.Value, request with { ExamId = id }, ct);
        return Ok(result);
    }

    private Guid? GetLearnerId()
    {
        var claim = ControllerContext?.HttpContext?.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        return Guid.TryParse(claim, out var learnerId) ? learnerId : null;
    }
}
