using HungryUp.Application.Catalog.Dtos;
using HungryUp.Domain.Catalog;
using HungryUp.Persistence.Catalog;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Application.Catalog;

public class CatalogService : ICatalogService
{
    private readonly CatalogDbContext _db;

    public CatalogService(CatalogDbContext db) => _db = db;

    public Task<List<CategoriaDto>> GetCategoriasAsync() =>
        _db.Categorias
            .Select(c => new CategoriaDto(c.Id, c.Nombre, c.Activo))
            .ToListAsync();

    public Task<List<ProductoDto>> GetProductosActivosAsync() =>
        _db.Productos
            .Where(p => p.StockActual > 0)
            .Select(p => new ProductoDto(p.Id, p.Nombre, p.Precio, p.StockActual, p.CategoriaId))
            .ToListAsync();

    public async Task<ProductoDto> CrearProductoAsync(CreateProductoDto dto)
    {
        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre,
            Precio = dto.Precio,
            StockActual = dto.StockInicial,
            CategoriaId = dto.CategoriaId
        };
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();
        return new ProductoDto(producto.Id, producto.Nombre, producto.Precio, producto.StockActual, producto.CategoriaId);
    }

    public async Task<ProductoDto?> ObtenerProductoPorIdAsync(Guid id)
    {
        var p = await _db.Productos.FirstOrDefaultAsync(p => p.Id == id);
        return p is null ? null : new ProductoDto(p.Id, p.Nombre, p.Precio, p.StockActual, p.CategoriaId);
    }
}
