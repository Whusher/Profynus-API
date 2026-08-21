using Microsoft.AspNetCore.Mvc;
using Profynus.Application.DTO.Auth;
using Profynus.Application.Common.RefreshToken;
using Profynus.Application.Auth.Commands;

namespace Profynus.Api.Controllers.Auth;

[ApiController]
[Route("api/auth")]
[Produces("application/json")]
public class AuthController(
    RegisterHandler register,
    LoginHandler login,
    RefreshTokenHandler refresh) : ControllerBase
{
    // POST /api/auth/register
    [HttpPost("register")]
    [ProducesResponseType(typeof(RegisterResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status422UnprocessableEntity)]
    public async Task<IActionResult> Register(
        [FromBody] RegisterRequest request,
        CancellationToken ct)
    {
        var result = await register.HandleAsync(request, HttpContext, ct);
        return StatusCode(StatusCodes.Status201Created, result);
    }

    // POST /api/auth/login
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status423Locked)]
    public async Task<IActionResult> Login(
        [FromBody] LoginRequest request,
        CancellationToken ct)
    {
        var result = await login.HandleAsync(request, HttpContext, ct);
        return Ok(result);
    }

    // POST /api/auth/refresh
    // Web clients: send empty body — token is read from __Host-rt cookie.
    // Mobile/Desktop clients: send { "refreshToken": "..." } in body.
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(RefreshResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Refresh(
        [FromBody] RefreshRequest request,
        CancellationToken ct)
    {
        var result = await refresh.HandleAsync(request, HttpContext, ct);
        return Ok(result);
    }

    // POST /api/auth/logout
    [HttpPost("logout")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    public IActionResult Logout(
        [FromServices] RefreshTokenDelivery delivery)
    {
        delivery.ClearCookie(Response);
        return NoContent();
    }
}