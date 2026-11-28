using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Application.Services.Abstraction;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class LessonsController(ILessonService lessonService) : ControllerBase
{
    [HttpGet("{lessonId}")]
    [Authorize(Policy = "StudentOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> GetLessonAsync(Guid lessonId)
    {
        var lessons = await lessonService.GetByIdAsync(lessonId, HttpContext.RequestAborted);

        return Ok(lessons);
    }
}
