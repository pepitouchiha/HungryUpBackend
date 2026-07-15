using System.Security.Claims;
using HungryUp.Application.Auth;
using HungryUp.Application.Auth.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [AllowAnonymous]
    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequestDto dto)
    {
        var result = await _auth.LoginAsync(dto, Ip(), UserAgent());
        if (result is null)
            return Unauthorized(new { message = "Credenciales inválidas." });
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("refresh")]
    public async Task<IActionResult> Refresh([FromBody] RefreshRequestDto dto)
    {
        var result = await _auth.RefreshAsync(dto.RefreshToken, Ip(), UserAgent());
        if (result is null)
            return Unauthorized(new { message = "Refresh token inválido o expirado. Inicia sesión nuevamente." });
        return Ok(result);
    }

    [AllowAnonymous]
    [HttpPost("logout")]
    public async Task<IActionResult> Logout([FromBody] RefreshRequestDto dto)
    {
        await _auth.LogoutAsync(dto.RefreshToken);
        return NoContent();
    }

    /// <summary>Devuelve el usuario autenticado junto con su rol y la lista de permisos efectivos.</summary>
    [Authorize]
    [HttpGet("me")]
    public IActionResult Me()
    {
        var rol = User.FindFirstValue(ClaimTypes.Role);
        return Ok(new
        {
            id = User.FindFirstValue(ClaimTypes.NameIdentifier),
            username = User.FindFirstValue(ClaimTypes.Name),
            email = User.FindFirstValue(ClaimTypes.Email),
            rol,
            permisos = RolePermissions.ForRole(rol).OrderBy(p => p).ToList()
        });
    }

    private string? Ip() => HttpContext.Connection.RemoteIpAddress?.ToString();
    private string? UserAgent() => Request.Headers.UserAgent.ToString();
}
