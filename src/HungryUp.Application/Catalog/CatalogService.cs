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
            .Select(p => new ProductoDto(p.Id, p.Nombre, p.Precio, p.StockActual, p.CategoriaId, p.TarifaIva, p.CostoPromedio, p.ImagenUrl, p.Activo))
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
        ValidarTarifaIva(dto.TarifaIva);

        var producto = new Producto
        {
            Id = Guid.NewGuid(),
            Nombre = dto.Nombre.Trim(),
            Precio = dto.Precio,
            StockActual = dto.StockInicial,
            CategoriaId = dto.CategoriaId,
            TarifaIva = dto.TarifaIva,
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
        ValidarTarifaIva(dto.TarifaIva);

        producto.Nombre = dto.Nombre.Trim();
        producto.Precio = dto.Precio;
        producto.StockActual = dto.StockActual;
        producto.CategoriaId = dto.CategoriaId;
        producto.TarifaIva = dto.TarifaIva;
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

    public async Task<ProductoDto> AumentarStockAsync(Guid id, int cantidad)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad a ingresar debe ser mayor a cero.");

        var producto = await _db.Productos.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe el producto {id}.");

        producto.StockActual += cantidad;
        await _db.SaveChangesAsync();
        return Map(producto);
    }

    public async Task<ProductoDto> AumentarStockConCostoAsync(Guid id, int cantidad, decimal costoUnitario)
    {
        if (cantidad <= 0)
            throw new ArgumentException("La cantidad a ingresar debe ser mayor a cero.");

        if (costoUnitario < 0)
            throw new ArgumentException("El costo unitario no puede ser negativo.");

        var producto = await _db.Productos.FindAsync(id)
            ?? throw new KeyNotFoundException($"No existe el producto {id}.");

        var stockPrevio = producto.StockActual;
        var stockNuevo = stockPrevio + cantidad;

        // Costo promedio ponderado. Si no había stock (o era negativo), el costo pasa a ser el de la compra.
        producto.CostoPromedio = stockPrevio > 0
            ? Math.Round((stockPrevio * producto.CostoPromedio + cantidad * costoUnitario) / stockNuevo, 2, MidpointRounding.AwayFromZero)
            : costoUnitario;
        producto.StockActual = stockNuevo;

        await _db.SaveChangesAsync();
        return Map(producto);
    }

    public async Task DescontarStockAsync(IReadOnlyCollection<AjusteStockDto> items)
    {
        if (items is null || items.Count == 0)
            throw new ArgumentException("No hay items para descontar del stock.");

        // Un mismo producto puede venir en varios items: se agrupan las cantidades.
        var requerido = items
            .GroupBy(i => i.ProductoId)
            .ToDictionary(g => g.Key, g => g.Sum(i => i.Cantidad));

        var ids = requerido.Keys.ToList();
        var productos = await _db.Productos
            .Where(p => ids.Contains(p.Id))
            .ToListAsync();

        foreach (var (productoId, cantidad) in requerido)
        {
            if (cantidad <= 0)
                throw new ArgumentException($"La cantidad del producto {productoId} debe ser mayor a cero.");

            var producto = productos.FirstOrDefault(p => p.Id == productoId)
                ?? throw new KeyNotFoundException($"No existe el producto {productoId}.");

            if (!producto.Activo)
                throw new InvalidOperationException($"El producto '{producto.Nombre}' está inactivo y no puede venderse.");

            if (producto.StockActual < cantidad)
                throw new InvalidOperationException(
                    $"Stock insuficiente para '{producto.Nombre}': disponible {producto.StockActual}, solicitado {cantidad}.");

            producto.StockActual -= cantidad;
        }

        // Se persiste solo si todas las validaciones pasaron (la iteración lanza antes de llegar aquí).
        await _db.SaveChangesAsync();
    }

    // ---------- Helpers ----------

    private async Task ValidarCategoriaAsync(Guid categoriaId)
    {
        var existe = await _db.Categorias.AnyAsync(c => c.Id == categoriaId && c.Activo);
        if (!existe)
            throw new InvalidOperationException($"La categoría {categoriaId} no existe o está inactiva.");
    }

    private static void ValidarTarifaIva(decimal tarifaIva)
    {
        if (tarifaIva < 0 || tarifaIva > 100)
            throw new ArgumentException("La tarifa de IVA debe estar entre 0 y 100.");
    }

    private static ProductoDto Map(Producto p) =>
        new(p.Id, p.Nombre, p.Precio, p.StockActual, p.CategoriaId, p.TarifaIva, p.CostoPromedio, p.ImagenUrl, p.Activo);
}
