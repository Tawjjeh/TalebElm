using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class LessonsController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => throw new NotImplementedException();
}