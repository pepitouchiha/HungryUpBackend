using HungryUpBackend.Modules.Orders.Entities;

namespace HungryUpBackend.Modules.Orders.Services;

public interface IOrdersService
{
    Task<Pedido> CrearPedidoAsync(CrearPedidoRequest request);
    Task ActualizarEstadoPreparacionAsync(Guid pedidoId, EstadoPreparacion nuevoEstado);
    Task<Pedido?> ObtenerPedidoPorIdAsync(Guid id);
    Task MarcarComoPagadoAsync(Guid pedidoId);
    Task LiberarMesaAsync(Guid mesaId);
}

public record CrearPedidoRequest(
    TipoRestaurante TipoRestaurante,
    Guid? MesaId,
    List<OrdenItemRequest> Items
);

public record OrdenItemRequest(Guid ProductoId, int Cantidad);
