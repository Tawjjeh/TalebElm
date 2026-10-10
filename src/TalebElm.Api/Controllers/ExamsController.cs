using Microsoft.AspNetCore.Mvc;

using NotImplementedException =
    TalebElm.Domain.Exceptions.NotImplementedException;

[ApiController]
[Route("api/[controller]")]
public class ExamsController : ControllerBase
{
    [HttpGet("{id}")]
    public IActionResult Get(Guid id)
        => throw new NotImplementedException("");

    [HttpPost]
    public IActionResult Post()
        => throw new NotImplementedException("");

    [HttpPost("{id}/submit")]
    public IActionResult Submit(Guid id)
        => throw new NotImplementedException("");
}