using HungryUpBackend.Modules.Catalog.Entities;
using Microsoft.EntityFrameworkCore;

namespace HungryUpBackend.Modules.Catalog.Services;

public class CatalogService : ICatalogService
{
    private readonly CatalogDbContext _db;

    public CatalogService(CatalogDbContext db) => _db = db;

    public Task<List<Producto>> GetProductosActivosAsync() =>
        _db.Productos.Where(p => p.StockActual > 0).ToListAsync();

    public async Task<Producto> CrearProductoAsync(string nombre, decimal precio, int stockInicial, Guid categoriaId)
    {
        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = nombre,
            Precio = precio,
            StockActual = stockInicial,
            CategoriaId = categoriaId
        };
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();
        return producto;
    }

    public Task<Producto?> ObtenerProductoPorIdAsync(Guid id) =>
        _db.Productos.FirstOrDefaultAsync(p => p.Id == id);
}
