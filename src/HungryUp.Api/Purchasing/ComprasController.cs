using HungryUp.Api.Authorization;
using HungryUp.Application.Auth;
using HungryUp.Application.Purchasing;
using HungryUp.Application.Purchasing.Dtos;
using HungryUp.Domain.Purchasing;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Purchasing;

[ApiController]
[Route("api/v1/compras")]
[Authorize]
public class ComprasController : ControllerBase
{
    private readonly IPurchasingService _service;

    public ComprasController(IPurchasingService service) => _service = service;

    [HttpGet]
    [HasPermission(Permissions.Purchasing.Read)]
    public async Task<IActionResult> GetCompras([FromQuery] EstadoCompra? estado = null) =>
        Ok(await _service.GetComprasAsync(estado));

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Purchasing.Read)]
    public async Task<IActionResult> GetCompra(Guid id)
    {
        var compra = await _service.ObtenerPorIdAsync(id);
        return compra is null ? NotFound() : Ok(compra);
    }

    [HttpPost]
    [HasPermission(Permissions.Purchasing.Create)]
    public async Task<IActionResult> CrearCompra([FromBody] CreateCompraDto dto)
    {
        var compra = await _service.CrearAsync(dto);
        return CreatedAtAction(nameof(GetCompra), new { id = compra.Id }, compra);
    }

    [HttpPut("{id:guid}")]
    [HasPermission(Permissions.Purchasing.Update)]
    public async Task<IActionResult> ActualizarCompra(Guid id, [FromBody] UpdateCompraDto dto) =>
        Ok(await _service.ActualizarAsync(id, dto));

    [HttpPatch("{id:guid}/notas")]
    [HasPermission(Permissions.Purchasing.Update)]
    public async Task<IActionResult> ActualizarNotas(Guid id, [FromBody] UpdateNotasCompraDto dto) =>
        Ok(await _service.ActualizarNotasAsync(id, dto));

    [HttpPost("{id:guid}/confirmar")]
    [HasPermission(Permissions.Purchasing.Confirm)]
    public async Task<IActionResult> Confirmar(Guid id) =>
        Ok(await _service.ConfirmarAsync(id));

    [HttpPost("{id:guid}/anular")]
    [HasPermission(Permissions.Purchasing.Anular)]
    public async Task<IActionResult> Anular(Guid id) =>
        Ok(await _service.AnularAsync(id));

    [HttpDelete("{id:guid}")]
    [HasPermission(Permissions.Purchasing.Delete)]
    public async Task<IActionResult> Eliminar(Guid id)
    {
        await _service.EliminarAsync(id);
        return NoContent();
    }
}
