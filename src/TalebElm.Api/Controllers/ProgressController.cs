using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TalebElm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class ProgressController : ControllerBase
    {
        [HttpGet("me")] 
        public IActionResult GetMyProgress() => StatusCode(StatusCodes.Status501NotImplemented); 
        
        [HttpGet("me/tracks/{trackId}")]
        public IActionResult GetByTrack(Guid trackId) => StatusCode(StatusCodes.Status501NotImplemented);
    }
}
