using Microsoft.AspNetCore.Mvc;
using NotImplementedException = TalebElm.Domain.Exceptions.NotImplementedException;

namespace TalebElm.Api.Controllers;

[Route("api/[controller]")]
[ApiController]
public class UsersController : ControllerBase
{
    [HttpGet]
    public IActionResult Get() => throw new NotImplementedException("Listing users is not implemented yet.");

    [HttpPost]
    public IActionResult Post() => throw new NotImplementedException("Creating a user via this endpoint is not implemented yet.");
}
