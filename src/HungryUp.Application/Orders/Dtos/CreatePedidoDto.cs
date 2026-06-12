using HungryUp.Domain.Orders;

namespace HungryUp.Application.Orders.Dtos;

public record CreatePedidoDto(TipoRestaurante TipoRestaurante, Guid? MesaId, List<OrdenItemDto> Items);
public record OrdenItemDto(Guid ProductoId, int Cantidad);
