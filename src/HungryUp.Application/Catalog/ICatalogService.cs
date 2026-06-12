using HungryUp.Application.Catalog.Dtos;

namespace HungryUp.Application.Catalog;

public interface ICatalogService
{
    Task<List<CategoriaDto>> GetCategoriasAsync();
    Task<List<ProductoDto>> GetProductosActivosAsync();
    Task<ProductoDto> CrearProductoAsync(CreateProductoDto dto);
    Task<ProductoDto?> ObtenerProductoPorIdAsync(Guid id);
}
