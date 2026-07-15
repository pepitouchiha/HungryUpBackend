using HungryUp.Domain.Orders;

namespace HungryUp.Application.Orders.Dtos;

public record DetallePedidoDto(Guid ProductoId, int Cantidad, decimal PrecioUnitario, decimal CostoUnitario);

public record PedidoDto(
    Guid Id,
    Guid? MesaId,
    DateTime FechaCreacion,
    DateTime? FechaEntrega,
    EstadoPreparacion EstadoPrep,
    EstadoFinanciero EstadoFin,
    TipoRestaurante Tipo,
    int NumeroTurno,
    List<DetallePedidoDto> Detalles
);
