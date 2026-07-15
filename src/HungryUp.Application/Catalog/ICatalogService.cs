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

    /// <summary>Suma <paramref name="cantidad"/> unidades al stock del producto (entrada de inventario).</summary>
    Task<ProductoDto> AumentarStockAsync(Guid id, int cantidad);

    /// <summary>
    /// Suma stock recalculando el costo promedio ponderado con el costo de la compra (usado al confirmar compras).
    /// </summary>
    Task<ProductoDto> AumentarStockConCostoAsync(Guid id, int cantidad, decimal costoUnitario);

    /// <summary>
    /// Valida disponibilidad y descuenta el stock de los productos indicados en una sola operación.
    /// Lanza excepción (sin persistir cambios) si algún producto no existe, está inactivo o no tiene stock suficiente.
    /// </summary>
    Task DescontarStockAsync(IReadOnlyCollection<AjusteStockDto> items);
}
