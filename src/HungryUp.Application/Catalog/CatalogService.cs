using HungryUp.Application.Catalog.Dtos;
using HungryUp.Domain.Catalog;
using HungryUp.Persistence.Catalog;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Application.Catalog;

public class CatalogService : ICatalogService
{
    private readonly CatalogDbContext _db;

    public CatalogService(CatalogDbContext db) => _db = db;

    // ---------- Categorías ----------

    public Task<List<CategoriaDto>> GetCategoriasAsync(bool soloActivos = false)
    {
        var query = _db.Categorias.AsQueryable();
        if (soloActivos)
            query = query.Where(c => c.Activo);
        return query
            .OrderBy(c => c.Nombre)
            .Select(c => new CategoriaDto(c.Id, c.Nombre, c.Activo))
            .ToListAsync();
    }

    public async Task<CategoriaDto?> ObtenerCategoriaPorIdAsync(Guid id)
    {
        var c = await _db.Categorias.FindAsync(id);
        return c is null ? null : new CategoriaDto(c.Id, c.Nombre, c.Activo);
    }

    public async Task<CategoriaDto> CrearCategoriaAsync(CreateCategoriaDto dto)
    {
        if (string.IsNullOrWhiteSpace(dto.Nombre))
            throw new ArgumentException("El nombre de la categoría es obligatorio.");

        var categoria = new Categoria
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Activo = true
        };
        _db.Categorias.Add(categoria);
        await _db.SaveChangesAsync();
        return new CategoriaDto(categoria.Id, categoria.Nombre, categoria.Activo);
    }

    public async Task<CategoriaDto> ActualizarCategoriaAsync(Guid id, UpdateCategoriaDto dto)
    {
        var categoria = await _db.Categorias.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe la categoría {id}.");

        categoria.Nombre = dto.Nombre.Trim();
        categoria.Activo = dto.Activo;
        await _db.SaveChangesAsync();
        return new CategoriaDto(categoria.Id, categoria.Nombre, categoria.Activo);
    }

    public async Task EliminarCategoriaAsync(Guid id)
    {
        var categoria = await _db.Categorias.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe la categoría {id}.");

        categoria.Activo = false; // borrado lógico
        await _db.SaveChangesAsync();
    }

    // ---------- Productos ----------

    public Task<List<ProductoDto>> GetProductosAsync(bool soloActivos = false)
    {
        var query = _db.Productos.AsQueryable();
        if (soloActivos)
            query = query.Where(p => p.Activo);
        return query
            .OrderBy(p => p.Nombre)
            .Select(p => new ProductoDto(p.Id, p.Nombre, p.Precio, p.StockActual, p.CategoriaId, p.ImagenUrl, p.Activo))
            .ToListAsync();
    }

    public async Task<ProductoDto?> ObtenerProductoPorIdAsync(Guid id)
    {
        var p = await _db.Productos.FindAsync(id);
        return p is null ? null : Map(p);
    }

    public async Task<ProductoDto> CrearProductoAsync(CreateProductoDto dto)
    {
        await ValidarCategoriaAsync(dto.CategoriaId);

        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Precio = dto.Precio,
            StockActual = dto.StockInicial,
            CategoriaId = dto.CategoriaId,
            ImagenUrl = dto.ImagenUrl,
            Activo = true
        };
        _db.Productos.Add(producto);
        await _db.SaveChangesAsync();
        return Map(producto);
    }

    public async Task<ProductoDto> ActualizarProductoAsync(Guid id, UpdateProductoDto dto)
    {
        var producto = await _db.Productos.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe el producto {id}.");

        await ValidarCategoriaAsync(dto.CategoriaId);

        producto.Nombre = dto.Nombre.Trim();
        producto.Precio = dto.Precio;
        producto.StockActual = dto.StockActual;
        producto.CategoriaId = dto.CategoriaId;
        producto.ImagenUrl = dto.ImagenUrl;
        await _db.SaveChangesAsync();
        return Map(producto);
    }

    public async Task EliminarProductoAsync(Guid id)
    {
        var producto = await _db.Productos.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe el producto {id}.");

        producto.Activo = false; // borrado lógico
        await _db.SaveChangesAsync();
    }

    public async Task<ProductoDto> ActualizarImagenProductoAsync(Guid id, string rutaImagen)
    {
        var producto = await _db.Productos.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe el producto {id}.");

        producto.ImagenUrl = rutaImagen;
        await _db.SaveChangesAsync();
        return Map(producto);
    }

    // ---------- Helpers ----------

    private async Task ValidarCategoriaAsync(Guid categoriaId)
    {
        var existe = await _db.Categorias.AnyAsync(c => c.Id == categoriaId && c.Activo);
        if (!existe)
            throw new InvalidOperationException($"La categoría {categoriaId} no existe o está inactiva.");
    }

    private static ProductoDto Map(Producto p) =>
        new(p.Id, p.Nombre, p.Precio, p.StockActual, p.CategoriaId, p.ImagenUrl, p.Activo);
}
