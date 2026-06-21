namespace HungryUp.Application.Catalog.Dtos;

public record CreateProductoDto(string Nombre, decimal Precio, int StockInicial, Guid CategoriaId, string? ImagenUrl = null);
