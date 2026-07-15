namespace HungryUp.Application.Catalog.Dtos;

/// <summary>Cantidad a descontar del stock de un producto al crear un pedido.</summary>
public record AjusteStockDto(Guid ProductoId, int Cantidad);
