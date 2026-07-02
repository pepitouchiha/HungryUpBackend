using HungryUp.Application.Catalog;
using HungryUp.Application.Orders.Dtos;
using HungryUp.Domain.Orders;
using HungryUp.Persistence.Orders;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Application.Orders;

public class OrdersService : IOrdersService
{
    private readonly OrdersDbContext _db;
    private readonly ICatalogService _catalog;

    public OrdersService(OrdersDbContext db, ICatalogService catalog)
    {
        _db = db;
        _catalog = catalog;
    }

    public async Task<List<PedidoDto>> GetPedidosAsync(EstadoPreparacion? estadoPrep = null)
    {
        var query = _db.Pedidos.Include(p => p.Detalles).AsQueryable();
        if (estadoPrep.HasValue)
            query = query.Where(p => p.EstadoPrep == estadoPrep.Value);
        var pedidos = await query.OrderByDescending(p => p.FechaCreacion).ToListAsync();
        return pedidos.Select(MapToDto).ToList();
    }

    // Colombia es UTC-5 fijo (no aplica horario de verano).
    private static readonly TimeSpan ColombiaOffset = TimeSpan.FromHours(-5);

    public async Task<List<PedidoDto>> GetEntregadosHoyAsync()
    {
        // "Hoy" según la hora local de Colombia, convertido a UTC para filtrar FechaEntrega.
        var inicioDiaColombia = (DateTime.UtcNow + ColombiaOffset).Date;
        var inicioUtc = inicioDiaColombia - ColombiaOffset;
        var finUtc = inicioUtc.AddDays(1);

        var pedidos = await _db.Pedidos
            .Include(p => p.Detalles)
            .Where(p => p.EstadoPrep == EstadoPreparacion.Entregado
                        && p.FechaEntrega != null
                        && p.FechaEntrega >= inicioUtc
                        && p.FechaEntrega < finUtc)
            .OrderByDescending(p => p.FechaEntrega)
            .ToListAsync();
        return pedidos.Select(MapToDto).ToList();
    }

    public async Task<PedidoDto> CrearPedidoAsync(CreatePedidoDto dto)
    {
        if (dto.TipoRestaurante == TipoRestaurante.Gourmet && dto.MesaId is null)
            throw new InvalidOperationException("MesaId es requerido para pedidos Gourmet.");

        if (dto.TipoRestaurante == TipoRestaurante.FastFood && dto.MesaId is not null)
            throw new InvalidOperationException("FastFood no puede tener Mesa asignada.");

        if (dto.TipoRestaurante == TipoRestaurante.Gourmet && dto.MesaId.HasValue)
        {
            var mesa = await _db.Mesas.FindAsync(dto.MesaId.Value)
                ?? throw new KeyNotFoundException($"Mesa {dto.MesaId} no encontrada.");
            mesa.Estado = EstadoMesa.Ocupada;
        }

        var turno = await _db.Pedidos.CountAsync() + 1;

        var pedido = new Pedido
        {
            Id = Guid.NewGuid(),
            MesaId = dto.MesaId,
            FechaCreacion = DateTime.UtcNow,
            EstadoPrep = EstadoPreparacion.Pendiente,
            EstadoFin = EstadoFinanciero.PorPagar,
            Tipo = dto.TipoRestaurante,
            NumeroTurno = turno
        };

        foreach (var item in dto.Items)
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
        return MapToDto(pedido);
    }

    public async Task ActualizarEstadoPreparacionAsync(Guid pedidoId, EstadoPreparacion nuevoEstado)
    {
        var pedido = await _db.Pedidos.FindAsync(pedidoId)
            ?? throw new KeyNotFoundException($"Pedido {pedidoId} no encontrado.");
        pedido.EstadoPrep = nuevoEstado;
        // Registra (o limpia) el momento de entrega según el nuevo estado.
        pedido.FechaEntrega = nuevoEstado == EstadoPreparacion.Entregado ? DateTime.UtcNow : null;
        await _db.SaveChangesAsync();
    }

    public async Task<PedidoDto?> ObtenerPedidoPorIdAsync(Guid id)
    {
        var pedido = await _db.Pedidos.Include(p => p.Detalles).FirstOrDefaultAsync(p => p.Id == id);
        return pedido is null ? null : MapToDto(pedido);
    }

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

    private static PedidoDto MapToDto(Pedido p) => new(
        p.Id,
        p.MesaId,
        p.FechaCreacion,
        p.FechaEntrega,
        p.EstadoPrep,
        p.EstadoFin,
        p.Tipo,
        p.NumeroTurno,
        p.Detalles.Select(d => new DetallePedidoDto(d.ProductoId, d.Cantidad, d.PrecioUnitario)).ToList()
    );
}
