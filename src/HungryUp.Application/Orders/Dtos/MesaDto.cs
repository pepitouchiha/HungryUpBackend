using HungryUp.Domain.Orders;

namespace HungryUp.Application.Orders.Dtos;

public record MesaDto(Guid Id, int Numero, EstadoMesa Estado);
