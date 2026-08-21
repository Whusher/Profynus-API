using Microsoft.AspNetCore.Mvc;
using Profynus.Application.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.RateLimiting;

namespace Profynus.Api.Controllers.User;

[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UserController (UserService userServ) : ControllerBase
{
    [HttpGet("validateUsername/{username}")]
    [EnableRateLimiting("username-check")]
    [ProducesResponseType(StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> ValidateUsername(string username)
    {
        var result = await userServ.ValidateUsername(username);
        return Ok(result);
    }
    
    [HttpGet("me")]
    [Authorize]
    public async Task<IActionResult> GetUserDetails(CancellationToken ct)
    {
        var result = await userServ.GetUserDetails(HttpContext, ct);
        return Ok(result);
    }
}