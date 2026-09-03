using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace TalebElm.Api.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class UsersController : ControllerBase
    {
        [HttpGet] 
        public IActionResult Get() => throw new NotImplementedException();
        
        [HttpPost] 
        public IActionResult Post() => throw new NotImplementedException();
    }
}
