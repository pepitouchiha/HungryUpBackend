namespace HungryUp.Application.Catalog.Dtos;

public record ProductoDto(Guid Id, string Nombre, decimal Precio, int StockActual, Guid CategoriaId, decimal TarifaIva, decimal CostoPromedio, string? ImagenUrl, bool Activo);
