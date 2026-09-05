using Microsoft.AspNetCore.Mvc;

[ApiController]
[Route("api/[controller]")]
public class TracksController : ControllerBase
{
    [HttpGet] 
    public IActionResult Get() => throw new NotImplementedException(); 
    
    [HttpPost]
    public IActionResult Post() => throw new NotImplementedException();
}