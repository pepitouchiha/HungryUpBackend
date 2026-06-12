using HungryUp.Application.Auth;
using HungryUp.Application.Auth.Dtos;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Auth;

[ApiController]
[Route("api/auth")]
public class AuthController : ControllerBase
{
    private readonly IAuthService _auth;

    public AuthController(IAuthService auth) => _auth = auth;

    [HttpPost("login")]
    public IActionResult Login([FromBody] LoginRequestDto dto)
    {
        var session = _auth.Login(dto);
        if (session is null)
            return Unauthorized(new { message = "Credenciales inválidas." });
        return Ok(session);
    }
}
