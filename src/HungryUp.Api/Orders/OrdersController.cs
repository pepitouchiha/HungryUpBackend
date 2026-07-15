using HungryUp.Api.Authorization;
using HungryUp.Application.Auth;
using HungryUp.Application.Orders;
using HungryUp.Application.Orders.Dtos;
using HungryUp.Domain.Orders;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Orders;

[ApiController]
[Route("api/v1/orders")]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _service;
    private readonly IMesaService _mesaService;

    public OrdersController(IOrdersService service, IMesaService mesaService)
    {
        _service = service;
        _mesaService = mesaService;
    }

    [HttpGet]
    [HasPermission(Permissions.Orders.Read)]
    public async Task<IActionResult> GetOrders([FromQuery] EstadoPreparacion? estadoPrep = null) =>
        Ok(await _service.GetPedidosAsync(estadoPrep));

    [HttpGet("mesas")]
    [HasPermission(Permissions.Orders.Read)]
    public async Task<IActionResult> GetMesas() =>
        Ok(await _mesaService.GetMesasAsync(soloActivos: true));

    [HttpGet("delivered-today")]
    [HasPermission(Permissions.Orders.Read)]
    public async Task<IActionResult> GetDeliveredToday() =>
        Ok(await _service.GetEntregadosHoyAsync());

    [HttpGet("{id:guid}")]
    [HasPermission(Permissions.Orders.Read)]
    public async Task<IActionResult> GetOrder(Guid id)
    {
        var pedido = await _service.ObtenerPedidoPorIdAsync(id);
        return pedido is null ? NotFound() : Ok(pedido);
    }

    [HttpPost]
    [HasPermission(Permissions.Orders.Create)]
    public async Task<IActionResult> CreateOrder([FromBody] CreatePedidoDto dto)
    {
        var pedido = await _service.CrearPedidoAsync(dto);
        return CreatedAtAction(nameof(GetOrder), new { id = pedido.Id }, pedido);
    }

    [HttpPut("{id:guid}/status")]
    [HasPermission(Permissions.Orders.UpdateStatus)]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateEstadoRequest request)
    {
        await _service.ActualizarEstadoPreparacionAsync(id, request.NuevoEstado);
        return NoContent();
    }
}

public record UpdateEstadoRequest(EstadoPreparacion NuevoEstado);
