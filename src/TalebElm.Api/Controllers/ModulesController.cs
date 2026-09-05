using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class ModulesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => throw new NotImplementedException();
}