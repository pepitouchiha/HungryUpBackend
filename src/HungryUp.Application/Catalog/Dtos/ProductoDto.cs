namespace HungryUp.Application.Catalog.Dtos;

public record ProductoDto(Guid Id, string Nombre, decimal Precio, int StockActual, Guid CategoriaId);
