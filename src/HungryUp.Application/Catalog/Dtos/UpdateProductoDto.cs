namespace HungryUp.Application.Catalog.Dtos;

public record UpdateProductoDto(string Nombre, decimal Precio, int StockActual, Guid CategoriaId, string? ImagenUrl);
