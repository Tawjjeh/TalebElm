using Microsoft.AspNetCore.Mvc;
using TalebElm.Application.Services.Abstraction;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TracksController(ITrackService trackService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> Get(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        CancellationToken ct = default)
    {
        var response = await trackService.GetPagedAsync(page, pageSize, ct);
        return Ok(response);
    }

    [HttpPost]
    public IActionResult Post() => throw new NotImplementedException("Creating a track via this endpoint is not implemented yet.");
}
