
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
    public IActionResult Submit(Guid id, [FromBody] SubmitExamAnswersRequest request)
        => throw new NotImplementedException(
            $"Submitting exam '{id}' is not implemented yet.");

    /// <summary>
    /// Updates an exam's title and pass threshold (Admin only).
    /// </summary>
    /// <param name="id">The unique identifier of the exam.</param>
    /// <param name="request">The new title and pass threshold.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="200">Returns the updated exam.</response>
    /// <response code="400">The title or pass threshold is invalid.</response>
    /// <response code="401">The caller is not authenticated as an admin.</response>
    /// <response code="403">The caller is not an admin.</response>
    /// <response code="404">The exam was not found.</response>
    [HttpPut("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ExamResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Put(Guid id, [FromBody] UpdateExamRequest request, CancellationToken ct = default)
    {
        var response = await examService.UpdateAsync(id, request, ct);
        return Ok(response);
    }

    /// <summary>
    /// Updates one question of an exam (Admin only). Decision 7 is re-validated
    /// on the resulting question: Points &gt; 0, at least 2 options, exactly 1 correct option.
    /// </summary>
    /// <param name="examId">The unique identifier of the exam.</param>
    /// <param name="questionId">The unique identifier of the question.</param>
    /// <param name="request">The new prompt, order, and points.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="200">Returns the updated question (learner-safe, no IsCorrect).</response>
    /// <response code="400">The question data violates Decision 7.</response>
    /// <response code="401">The caller is not authenticated as an admin.</response>
    /// <response code="403">The caller is not an admin.</response>
    /// <response code="404">The exam or the question was not found.</response>
    [HttpPut("{examId}/questions/{questionId}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ExamQuestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> PutQuestion(Guid examId, Guid questionId, [FromBody] UpdateExamQuestionRequest request, CancellationToken ct = default)
    {
        var response = await examService.UpdateQuestionAsync(examId, questionId, request, ct);
        return Ok(response);
    }

    /// <summary>
    /// Adds one option to a question (Admin only). Decision 7 is re-validated
    /// on the resulting question: at least 2 options, exactly 1 correct option.
    /// </summary>
    /// <param name="examId">The unique identifier of the exam.</param>
    /// <param name="questionId">The unique identifier of the question.</param>
    /// <param name="request">The option text and whether it is correct.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="200">Returns the updated question (learner-safe, no IsCorrect).</response>
    /// <response code="400">The option data violates Decision 7.</response>
    /// <response code="401">The caller is not authenticated as an admin.</response>
    /// <response code="403">The caller is not an admin.</response>
    /// <response code="404">The exam or the question was not found.</response>
    [HttpPost("{examId}/questions/{questionId}/options")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ExamQuestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> AddOption(Guid examId, Guid questionId, [FromBody] AddExamOptionRequest request, CancellationToken ct = default)
    {
        var response = await examService.AddOptionAsync(examId, questionId, request, ct);
        return Ok(response);
    }

    /// <summary>
    /// Removes one option from a question (Admin only). Decision 7 is re-validated
    /// on the resulting question: at least 2 options, exactly 1 correct option.
    /// </summary>
    /// <param name="examId">The unique identifier of the exam.</param>
    /// <param name="questionId">The unique identifier of the question.</param>
    /// <param name="optionId">The unique identifier of the option.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="200">Returns the updated question (learner-safe, no IsCorrect).</response>
    /// <response code="400">Removing the option would violate Decision 7.</response>
    /// <response code="401">The caller is not authenticated as an admin.</response>
    /// <response code="403">The caller is not an admin.</response>
    /// <response code="404">The exam, the question, or the option was not found.</response>
    [HttpDelete("{examId}/questions/{questionId}/options/{optionId}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(typeof(ExamQuestionResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> RemoveOption(Guid examId, Guid questionId, Guid optionId, CancellationToken ct = default)
    {
        var response = await examService.RemoveOptionAsync(examId, questionId, optionId, ct);
        return Ok(response);
    }

    /// <summary>
    /// Deletes an exam (Admin only). Returns 409 Conflict when any learner has
    /// progress on the exam's module.
    /// </summary>
    /// <param name="id">The unique identifier of the exam.</param>
    /// <param name="ct">Cancels the request.</param>
    /// <response code="204">The exam was deleted.</response>
    /// <response code="401">The caller is not authenticated as an admin.</response>
    /// <response code="403">The caller is not an admin.</response>
    /// <response code="404">The exam was not found.</response>
    /// <response code="409">Learners have progress on the exam's module.</response>
    [HttpDelete("{id}")]
    [Authorize(Policy = "AdminOnly")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Delete(Guid id, CancellationToken ct = default)
    {
        await examService.DeleteAsync(id, ct);
        return NoContent();
    }
}
