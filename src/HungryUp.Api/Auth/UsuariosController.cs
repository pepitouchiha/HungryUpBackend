using HungryUp.Api.Authorization;
using HungryUp.Application.Auth;
using HungryUp.Application.Auth.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Auth;

[ApiController]
[Route("api/v1/users")]
[Authorize]
public class UsuariosController : ControllerBase
{
    private readonly IUsuarioService _service;

    public UsuariosController(IUsuarioService service) => _service = service;

    [HttpGet]
    [HasPermission(Permissions.Users.Read)]
    public async Task<IActionResult> GetAll() =>
        Ok(await _service.GetAllAsync());

    [HttpGet("{id:int}")]
    [HasPermission(Permissions.Users.Read)]
    public async Task<IActionResult> GetById(int id)
    {
        var usuario = await _service.GetByIdAsync(id);
        return usuario is null ? NotFound() : Ok(usuario);
    }

    [HttpPost]
    [HasPermission(Permissions.Users.Create)]
    public async Task<IActionResult> Create([FromBody] CreateUsuarioDto dto)
    {
        var usuario = await _service.CrearAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = usuario.Id }, usuario);
    }

    [HttpPut("{id:int}")]
    [HasPermission(Permissions.Users.Update)]
    public async Task<IActionResult> Update(int id, [FromBody] UpdateUsuarioDto dto) =>
        Ok(await _service.ActualizarAsync(id, dto));

    [HttpPut("{id:int}/password")]
    [HasPermission(Permissions.Users.Update)]
    public async Task<IActionResult> ChangePassword(int id, [FromBody] ChangePasswordDto dto)
    {
        await _service.CambiarPasswordAsync(id, dto);
        return NoContent();
    }

    [HttpDelete("{id:int}")]
    [HasPermission(Permissions.Users.Delete)]
    public async Task<IActionResult> Delete(int id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
