using HungryUp.Application.Catalog.Dtos;

namespace HungryUp.Application.Catalog;

public interface ICatalogService
{
    // Categorías
    Task<List<CategoriaDto>> GetCategoriasAsync(bool soloActivos = false);
    Task<CategoriaDto?> ObtenerCategoriaPorIdAsync(Guid id);
    Task<CategoriaDto> CrearCategoriaAsync(CreateCategoriaDto dto);
    Task<CategoriaDto> ActualizarCategoriaAsync(Guid id, UpdateCategoriaDto dto);
    Task EliminarCategoriaAsync(Guid id);

    // Productos
    Task<List<ProductoDto>> GetProductosAsync(bool soloActivos = false);
    Task<ProductoDto?> ObtenerProductoPorIdAsync(Guid id);
    Task<ProductoDto> CrearProductoAsync(CreateProductoDto dto);
    Task<ProductoDto> ActualizarProductoAsync(Guid id, UpdateProductoDto dto);
    Task EliminarProductoAsync(Guid id);
    Task<ProductoDto> ActualizarImagenProductoAsync(Guid id, string rutaImagen);
}
