
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Http;
using TalebElm.Application.DTOs;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExamsController : ControllerBase
{
    /// <summary>
    /// Retrieves an exam by its identifier.
    /// </summary>
    /// <param name="id">The unique identifier of the exam.</param>
    /// <response code="200">Returns the requested exam.</response>
    /// <response code="501">The endpoint is not implemented yet.</response>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ExamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Get(Guid id)
        => throw new NotImplementedException(
            $"Retrieving exam '{id}' is not implemented yet.");

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
    /// <response code="200">Returns the exam result.</response>
    /// <response code="501">The endpoint is not implemented yet.</response>
    [HttpPost("{id}/submit")]
    [ProducesResponseType(typeof(ExamResultResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public IActionResult Submit(Guid id, [FromBody] SubmitExamRequest request)
        => throw new NotImplementedException(
            $"Submitting exam '{id}' is not implemented yet.");
}
