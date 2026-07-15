namespace HungryUp.Application.Catalog.Dtos;

public record UpdateProductoDto(string Nombre, decimal Precio, int StockActual, Guid CategoriaId, decimal TarifaIva, string? ImagenUrl);
