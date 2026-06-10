using HungryUpBackend.Modules.Catalog.Entities;

namespace HungryUpBackend.Modules.Catalog.Services;

public interface ICatalogService
{
    Task<List<Producto>> GetProductosActivosAsync();
    Task<Producto> CrearProductoAsync(string nombre, decimal precio, int stockInicial, Guid categoriaId);
    Task<Producto?> ObtenerProductoPorIdAsync(Guid id);
}
