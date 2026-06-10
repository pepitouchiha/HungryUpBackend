using HungryUpBackend.Modules.Catalog.Services;
using HungryUpBackend.Modules.Orders.Entities;
using Microsoft.EntityFrameworkCore;

namespace HungryUpBackend.Modules.Orders.Services;

public class OrdersService : IOrdersService
{
    private readonly OrdersDbContext _db;
    private readonly ICatalogService _catalog;

    public OrdersService(OrdersDbContext db, ICatalogService catalog)
    {
        _db = db;
        _catalog = catalog;
    }

    public async Task<Pedido> CrearPedidoAsync(CrearPedidoRequest request)
    {
        if (request.TipoRestaurante == TipoRestaurante.Gourmet && request.MesaId is null)
            throw new InvalidOperationException("MesaId es requerido para pedidos Gourmet.");

        if (request.TipoRestaurante == TipoRestaurante.FastFood && request.MesaId is not null)
            throw new InvalidOperationException("FastFood no puede tener Mesa asignada.");

        if (request.TipoRestaurante == TipoRestaurante.Gourmet && request.MesaId.HasValue)
        {
            var mesa = await _db.Mesas.FindAsync(request.MesaId.Value)
                ?? throw new KeyNotFoundException($"Mesa {request.MesaId} no encontrada.");
            mesa.Estado = EstadoMesa.Ocupada;
        }

        var turno = await _db.Pedidos.CountAsync() + 1;

        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            MesaId = request.MesaId,
            FechaCreacion = DateTime.UtcNow,
            EstadoPrep = EstadoPreparacion.Pendiente,
            EstadoFin = EstadoFinanciero.PorPagar,
            Tipo = request.TipoRestaurante,
            NumeroTurno = turno
        };

        foreach (var item in request.Items)
        {
            var producto = await _catalog.ObtenerProductoPorIdAsync(item.ProductoId)
                ?? throw new KeyNotFoundException($"Producto {item.ProductoId} no encontrado.");

            pedido.Detalles.Add(new DetallePedido
            {
                Id = Guid.NewGuid(),
                PedidoId = pedido.Id,
                ProductoId = item.ProductoId,
                Cantidad = item.Cantidad,
                PrecioUnitario = producto.Precio
            });
        }

        _db.Pedidos.Add(pedido);
        await _db.SaveChangesAsync();
        return pedido;
    }

    public async Task ActualizarEstadoPreparacionAsync(Guid pedidoId, EstadoPreparacion nuevoEstado)
    {
        var pedido = await _db.Pedidos.FindAsync(pedidoId)
            ?? throw new KeyNotFoundException($"Pedido {pedidoId} no encontrado.");
        pedido.EstadoPrep = nuevoEstado;
        await _db.SaveChangesAsync();
    }

    public Task<Pedido?> ObtenerPedidoPorIdAsync(Guid id) =>
        _db.Pedidos.Include(p => p.Detalles).FirstOrDefaultAsync(p => p.Id == id);

    public async Task MarcarComoPagadoAsync(Guid pedidoId)
    {
        var pedido = await _db.Pedidos.FindAsync(pedidoId)
            ?? throw new KeyNotFoundException($"Pedido {pedidoId} no encontrado.");
        pedido.EstadoFin = EstadoFinanciero.Pagado;
        await _db.SaveChangesAsync();
    }

    public async Task LiberarMesaAsync(Guid mesaId)
    {
        var mesa = await _db.Mesas.FindAsync(mesaId)
            ?? throw new KeyNotFoundException($"Mesa {mesaId} no encontrada.");
        mesa.Estado = EstadoMesa.Libre;
        await _db.SaveChangesAsync();
    }
}
