using Microsoft.AspNetCore.Mvc;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ModulesController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => throw new NotImplementedException("Listing modules is not implemented yet.");
}
