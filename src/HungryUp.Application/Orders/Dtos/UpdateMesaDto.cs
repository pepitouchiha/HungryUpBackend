using HungryUp.Domain.Orders;

namespace HungryUp.Application.Orders.Dtos;

public record UpdateMesaDto(int Numero, EstadoMesa Estado, bool Activo);
