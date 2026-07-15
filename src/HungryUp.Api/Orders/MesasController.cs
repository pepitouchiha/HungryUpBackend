using HungryUp.Api.Authorization;
using HungryUp.Application.Auth;
using HungryUp.Application.Orders;
using HungryUp.Application.Orders.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Orders;

[ApiController]
[Route("api/v1/mesas")]
[Authorize]
public class MesasController : ControllerBase
{
    private readonly IMesaService _service;

    public MesasController(IMesaService service) => _service = service;

    [HttpGet]
    [HasPermission(Permissions.Mesas.Read)]
    public async Task<IActionResult> GetMesas([FromQuery] bool activos = false) =>
        Ok(await _service.GetMesasAsync(activos));

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Mesas.Read)]
    public async Task<IActionResult> GetMesa(Guid id)
    {
        var mesa = await _service.ObtenerPorIdAsync(id);
        return mesa is null ? NotFound() : Ok(mesa);
    }

    [HttpPost]
    [HasPermission(Permissions.Mesas.Create)]
    public async Task<IActionResult> CreateMesa([FromBody] CreateMesaDto dto)
    {
        var mesa = await _service.CrearAsync(dto);
        return CreatedAtAction(nameof(GetMesa), new { id = mesa.Id }, mesa);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Mesas.Update)]
    public async Task<IActionResult> UpdateMesa(Guid id, [FromBody] UpdateMesaDto dto) =>
        Ok(await _service.ActualizarAsync(id, dto));

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Mesas.Delete)]
    public async Task<IActionResult> DeleteMesa(Guid id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
