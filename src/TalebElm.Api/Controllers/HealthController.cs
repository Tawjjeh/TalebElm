using Microsoft.AspNetCore.Mvc;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => Ok("healthy");
}
