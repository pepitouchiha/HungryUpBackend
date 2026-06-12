using HungryUp.Domain.Billing;

namespace HungryUp.Application.Billing.Dtos;

public record PagoDto(Guid Id, Guid PedidoId, decimal MontoTotal, MetodoPago Metodo, DateTime FechaPago);
