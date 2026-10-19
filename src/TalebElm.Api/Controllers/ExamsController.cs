using Microsoft.AspNetCore.Mvc;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ExamsController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult Get(Guid id)
        => throw new NotImplementedException($"Retrieving exam '{id}' is not implemented yet.");

    [HttpPost]
    public IActionResult Post()
        => throw new NotImplementedException("Creating an exam via this endpoint is not implemented yet.");

    [HttpPost("{id}/submit")]
    public IActionResult Submit(Guid id)
        => throw new NotImplementedException($"Submitting exam '{id}' is not implemented yet.");
}
