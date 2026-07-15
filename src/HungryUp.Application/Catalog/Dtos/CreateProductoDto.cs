namespace HungryUp.Application.Catalog.Dtos;

public record CreateProductoDto(string Nombre, decimal Precio, int StockInicial, Guid CategoriaId, decimal TarifaIva = 19m, string? ImagenUrl = null);
