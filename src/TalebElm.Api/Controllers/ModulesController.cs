using System.Drawing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using TalebElm.Application.Services.Abstraction;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModulesController(ILessonService lessonService) : ControllerBase
{
    [HttpGet("{moduleId:guid}/lessons")]
    [Authorize(Policy = "StudentOnly")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status501NotImplemented)]
    public async Task<IActionResult> Get(Guid moduleId)
    {
        var lessons = await lessonService.GetByModuleAsync(moduleId, HttpContext.RequestAborted);

        return Ok(lessons);
    }
}
