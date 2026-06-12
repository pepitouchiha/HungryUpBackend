using HungryUp.Domain.Billing;

namespace HungryUp.Application.Billing.Dtos;

public record ProcesarPagoDto(Guid PedidoId, MetodoPago Metodo, decimal MontoPagado);
