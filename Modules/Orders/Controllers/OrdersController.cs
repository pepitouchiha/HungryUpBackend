using HungryUpBackend.Modules.Orders.Entities;
using HungryUpBackend.Modules.Orders.Services;
using Microsoft.AspNetCore.Mvc;

namespace HungryUpBackend.Modules.Orders.Controllers;

[ApiController]
[Route("api/v1/orders")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersService _service;

    public OrdersController(IOrdersService service) => _service = service;

    [HttpPost]
    public async Task<IActionResult> CreateOrder([FromBody] CreateOrderRequest request)
    {
        var pedido = await _service.CrearPedidoAsync(new CrearPedidoRequest(
            request.TipoRestaurante,
            request.MesaId,
            request.Items.Select(i => new OrdenItemRequest(i.ProductoId, i.Cantidad)).ToList()
        ));
        return CreatedAtAction(nameof(CreateOrder), new { id = pedido.Id }, pedido);
    }

    [HttpPut("{id:guid}/status")]
    public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateStatusRequest request)
    {
        await _service.ActualizarEstadoPreparacionAsync(id, request.NuevoEstado);
        return NoContent();
    }
}

public record CreateOrderRequest(TipoRestaurante TipoRestaurante, Guid? MesaId, List<OrderItemRequest> Items);
public record OrderItemRequest(Guid ProductoId, int Cantidad);
public record UpdateStatusRequest(EstadoPreparacion NuevoEstado);
