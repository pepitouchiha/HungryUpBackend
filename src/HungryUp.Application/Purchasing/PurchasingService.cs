using HungryUp.Application.Catalog;
using HungryUp.Application.Catalog.Dtos;
using HungryUp.Application.Purchasing.Dtos;
using HungryUp.Domain.Purchasing;
using HungryUp.Persistence.Purchasing;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Application.Purchasing;

public class PurchasingService : IPurchasingService
{
    private readonly PurchasingDbContext _db;
    private readonly ICatalogService _catalog;

    public PurchasingService(PurchasingDbContext db, ICatalogService catalog)
    {
        _db = db;
        _catalog = catalog;
    }

    public async Task<List<CompraDto>> GetComprasAsync(EstadoCompra? estado = null)
    {
        var query = _db.Compras.Include(c => c.Lineas).AsQueryable();
        if (estado.HasValue)
            query = query.Where(c => c.Estado == estado.Value);
        var compras = await query.OrderByDescending(c => c.FechaCreacion).ToListAsync();
        return compras.Select(MapToDto).ToList();
    }

    public async Task<CompraDto?> ObtenerPorIdAsync(Guid id)
    {
        var compra = await CargarAsync(id);
        return compra is null ? null : MapToDto(compra);
    }

    public async Task<CompraDto> CrearAsync(CreateCompraDto dto)
    {
        ValidarCabecera(dto.NumeroFactura, dto.NombreProveedor, dto.ReteFuentePorc, dto.ReteIvaPorc, dto.ReteIcaPorMil);

        var compra = new Compra
        {
            Id = Guid.NewGuid(),
            NumeroFactura = dto.NumeroFactura.Trim(),
            NombreProveedor = dto.NombreProveedor.Trim(),
            NitProveedor = string.IsNullOrWhiteSpace(dto.NitProveedor) ? null : dto.NitProveedor.Trim(),
            Fecha = dto.Fecha ?? DateTime.UtcNow,
            Notas = string.IsNullOrWhiteSpace(dto.Notas) ? null : dto.Notas.Trim(),
            Estado = EstadoCompra.Borrador,
            ReteFuentePorc = dto.ReteFuentePorc,
            ReteIvaPorc = dto.ReteIvaPorc,
            ReteIcaPorMil = dto.ReteIcaPorMil,
            FechaCreacion = DateTime.UtcNow
        };

        compra.Lineas = await ConstruirLineasAsync(compra.Id, dto.Items);

        _db.Compras.Add(compra);
        await _db.SaveChangesAsync();
        return MapToDto(compra);
    }

    public async Task<CompraDto> ActualizarAsync(Guid id, UpdateCompraDto dto)
    {
        var compra = await CargarAsync(id)
            ?? throw new KeyNotFoundException($"No existe la compra {id}.");

        if (compra.Estado != EstadoCompra.Borrador)
            throw new InvalidOperationException("Solo se pueden editar compras en estado Borrador.");

        ValidarCabecera(dto.NumeroFactura, dto.NombreProveedor, dto.ReteFuentePorc, dto.ReteIvaPorc, dto.ReteIcaPorMil);

        // Se construyen y validan las nuevas líneas ANTES de tocar las existentes,
        // para no dejar la compra sin líneas si la validación falla.
        var nuevasLineas = await ConstruirLineasAsync(compra.Id, dto.Items);

        compra.NumeroFactura = dto.NumeroFactura.Trim();
        compra.NombreProveedor = dto.NombreProveedor.Trim();
        compra.NitProveedor = string.IsNullOrWhiteSpace(dto.NitProveedor) ? null : dto.NitProveedor.Trim();
        compra.Fecha = dto.Fecha ?? compra.Fecha;
        compra.Notas = string.IsNullOrWhiteSpace(dto.Notas) ? null : dto.Notas.Trim();
        compra.ReteFuentePorc = dto.ReteFuentePorc;
        compra.ReteIvaPorc = dto.ReteIvaPorc;
        compra.ReteIcaPorMil = dto.ReteIcaPorMil;

        // Reemplazo total de líneas: se eliminan las actuales y se persiste antes de insertar las nuevas
        // (borrar e insertar hijos en el mismo SaveChanges provoca excepciones de tracking en EF).
        _db.LineasCompra.RemoveRange(compra.Lineas);
        await _db.SaveChangesAsync();

        _db.LineasCompra.AddRange(nuevasLineas);
        await _db.SaveChangesAsync();

        compra.Lineas = nuevasLineas;
        return MapToDto(compra);
    }

    public async Task<CompraDto> ActualizarNotasAsync(Guid id, UpdateNotasCompraDto dto)
    {
        var compra = await CargarAsync(id)
            ?? throw new KeyNotFoundException($"No existe la compra {id}.");

        if (compra.Estado == EstadoCompra.Anulada)
            throw new InvalidOperationException("No se pueden editar las notas de una compra anulada.");

        compra.Notas = string.IsNullOrWhiteSpace(dto.Notas) ? null : dto.Notas.Trim();
        await _db.SaveChangesAsync();
        return MapToDto(compra);
    }

    public async Task<CompraDto> ConfirmarAsync(Guid id)
    {
        var compra = await CargarAsync(id)
            ?? throw new KeyNotFoundException($"No existe la compra {id}.");

        if (compra.Estado != EstadoCompra.Borrador)
            throw new InvalidOperationException("Solo se pueden confirmar compras en estado Borrador.");

        if (compra.Lineas.Count == 0)
            throw new InvalidOperationException("La compra no tiene líneas para confirmar.");

        // Aumenta el inventario de cada producto y recalcula su costo promedio ponderado
        // (comunicación con el módulo Catalog vía su servicio).
        foreach (var linea in compra.Lineas)
            await _catalog.AumentarStockConCostoAsync(linea.ProductoId, linea.Cantidad, linea.CostoUnitario);

        compra.Estado = EstadoCompra.Confirmada;
        compra.FechaConfirmacion = DateTime.UtcNow;
        await _db.SaveChangesAsync();
        return MapToDto(compra);
    }

    public async Task<CompraDto> AnularAsync(Guid id)
    {
        var compra = await CargarAsync(id)
            ?? throw new KeyNotFoundException($"No existe la compra {id}.");

        if (compra.Estado == EstadoCompra.Anulada)
            throw new InvalidOperationException("La compra ya está anulada.");

        // Si estaba confirmada, revierte el inventario que había sumado.
        // DescontarStockAsync lanza si el stock ya se consumió (no permite dejarlo negativo).
        if (compra.Estado == EstadoCompra.Confirmada)
        {
            var items = compra.Lineas
                .Select(l => new AjusteStockDto(l.ProductoId, l.Cantidad))
                .ToList();
            await _catalog.DescontarStockAsync(items);
        }

        compra.Estado = EstadoCompra.Anulada;
        await _db.SaveChangesAsync();
        return MapToDto(compra);
    }

    public async Task EliminarAsync(Guid id)
    {
        var compra = await CargarAsync(id)
            ?? throw new KeyNotFoundException($"No existe la compra {id}.");

        if (compra.Estado != EstadoCompra.Borrador)
            throw new InvalidOperationException("Solo se pueden eliminar compras en estado Borrador. Use anular para las confirmadas.");

        _db.Compras.Remove(compra);
        await _db.SaveChangesAsync();
    }

    public async Task<decimal> GetTotalCompradoAsync(DateTime desde, DateTime hasta)
    {
        var compras = await _db.Compras
            .Include(c => c.Lineas)
            .Where(c => c.Estado == EstadoCompra.Confirmada
                        && c.FechaConfirmacion != null
                        && c.FechaConfirmacion >= desde && c.FechaConfirmacion <= hasta)
            .ToListAsync();

        return compras.Sum(c => MapToDto(c).TotalAPagar);
    }

    // ---------- Helpers ----------

    private Task<Compra?> CargarAsync(Guid id) =>
        _db.Compras.Include(c => c.Lineas).FirstOrDefaultAsync(c => c.Id == id);

    private async Task<List<LineaCompra>> ConstruirLineasAsync(Guid compraId, List<CreateLineaCompraDto> items)
    {
        if (items is null || items.Count == 0)
            throw new InvalidOperationException("La compra debe tener al menos una línea.");

        var lineas = new List<LineaCompra>();
        foreach (var item in items)
        {
            if (item.Cantidad <= 0)
                throw new InvalidOperationException("La cantidad de cada línea debe ser mayor a cero.");

            if (item.CostoUnitario < 0)
                throw new InvalidOperationException("El costo unitario no puede ser negativo.");

            var producto = await _catalog.ObtenerProductoPorIdAsync(item.ProductoId)
                ?? throw new KeyNotFoundException($"No existe el producto {item.ProductoId}.");

            var tarifaIva = item.TarifaIva ?? producto.TarifaIva;
            if (tarifaIva < 0 || tarifaIva > 100)
                throw new ArgumentException("La tarifa de IVA de la línea debe estar entre 0 y 100.");

            lineas.Add(new LineaCompra
            {
                Id = Guid.NewGuid(),
                CompraId = compraId,
                ProductoId = producto.Id,
                ProductoNombre = producto.Nombre,
                Cantidad = item.Cantidad,
                CostoUnitario = item.CostoUnitario,
                TarifaIva = tarifaIva
            });
        }
        return lineas;
    }

    private static void ValidarCabecera(string numeroFactura, string nombreProveedor,
        decimal reteFuentePorc, decimal reteIvaPorc, decimal reteIcaPorMil)
    {
        if (string.IsNullOrWhiteSpace(numeroFactura))
            throw new ArgumentException("El número de factura es obligatorio.");

        if (string.IsNullOrWhiteSpace(nombreProveedor))
            throw new ArgumentException("El nombre del proveedor es obligatorio.");

        if (reteFuentePorc is < 0 or > 100)
            throw new ArgumentException("La retención en la fuente (%) debe estar entre 0 y 100.");

        if (reteIvaPorc is < 0 or > 100)
            throw new ArgumentException("La retención de IVA (%) debe estar entre 0 y 100.");

        if (reteIcaPorMil is < 0 or > 1000)
            throw new ArgumentException("La retención de ICA (por mil) debe estar entre 0 y 1000.");
    }

    private static decimal Redondear(decimal valor) => Math.Round(valor, 2, MidpointRounding.AwayFromZero);

    private static CompraDto MapToDto(Compra c)
    {
        var lineasDto = c.Lineas
            .OrderBy(l => l.ProductoNombre)
            .Select(l =>
            {
                var subtotal = l.Cantidad * l.CostoUnitario;
                var ivaValor = Redondear(subtotal * l.TarifaIva / 100m);
                return new LineaCompraDto(
                    l.Id, l.ProductoId, l.ProductoNombre, l.Cantidad, l.CostoUnitario, l.TarifaIva,
                    Redondear(subtotal), ivaValor, Redondear(subtotal + ivaValor));
            })
            .ToList();

        var subtotalTotal = Redondear(lineasDto.Sum(l => l.Subtotal));
        var ivaTotal = Redondear(lineasDto.Sum(l => l.IvaValor));

        var reteFuenteValor = Redondear(subtotalTotal * c.ReteFuentePorc / 100m);
        var reteIvaValor = Redondear(ivaTotal * c.ReteIvaPorc / 100m);
        var reteIcaValor = Redondear(subtotalTotal * c.ReteIcaPorMil / 1000m);
        var totalRetenciones = reteFuenteValor + reteIvaValor + reteIcaValor;
        var totalBruto = subtotalTotal + ivaTotal;

        return new CompraDto(
            c.Id, c.NumeroFactura, c.NombreProveedor, c.NitProveedor, c.Fecha, c.Notas, c.Estado,
            c.ReteFuentePorc, c.ReteIvaPorc, c.ReteIcaPorMil, c.FechaCreacion, c.FechaConfirmacion,
            lineasDto,
            subtotalTotal, ivaTotal, reteFuenteValor, reteIvaValor, reteIcaValor,
            totalRetenciones, totalBruto, totalBruto - totalRetenciones);
    }
}
