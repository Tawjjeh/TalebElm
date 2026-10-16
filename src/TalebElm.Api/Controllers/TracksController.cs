using Microsoft.AspNetCore.Mvc;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TracksController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => throw new NotImplementedException("Listing tracks is not implemented yet.");

    [HttpPost]
    public IActionResult Post() => throw new NotImplementedException("Creating a track via this endpoint is not implemented yet.");
}
