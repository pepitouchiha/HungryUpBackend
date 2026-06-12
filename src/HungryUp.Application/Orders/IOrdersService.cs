using HungryUp.Application.Orders.Dtos;
using HungryUp.Domain.Orders;

namespace HungryUp.Application.Orders;

public interface IOrdersService
{
    Task<List<PedidoDto>> GetPedidosAsync(EstadoPreparacion? estadoPrep = null);
    Task<List<MesaDto>> GetMesasAsync();
    Task<PedidoDto> CrearPedidoAsync(CreatePedidoDto dto);
    Task ActualizarEstadoPreparacionAsync(Guid pedidoId, EstadoPreparacion nuevoEstado);
    Task<PedidoDto?> ObtenerPedidoPorIdAsync(Guid id);
    Task MarcarComoPagadoAsync(Guid pedidoId);
    Task LiberarMesaAsync(Guid mesaId);
}
