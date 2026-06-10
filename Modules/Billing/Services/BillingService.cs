using HungryUpBackend.Modules.Billing.Entities;
using HungryUpBackend.Modules.Orders.Entities;
using HungryUpBackend.Modules.Orders.Services;
using Microsoft.EntityFrameworkCore;

namespace HungryUpBackend.Modules.Billing.Services;

public class BillingService : IBillingService
{
    private readonly BillingDbContext _db;
    private readonly IOrdersService _orders;

    public BillingService(BillingDbContext db, IOrdersService orders)
    {
        _db = db;
        _orders = orders;
    }

    public async Task<Pago> ProcesarPagoAsync(Guid pedidoId, MetodoPago metodo, decimal montoPagado)
    {
        var pedido = await _orders.ObtenerPedidoPorIdAsync(pedidoId)
            ?? throw new KeyNotFoundException($"Pedido {pedidoId} no encontrado.");

        if (pedido.EstadoFin == EstadoFinanciero.Pagado)
            throw new InvalidOperationException("El pedido ya se encuentra pagado.");

        var total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            PedidoId = pedidoId,
            MontoTotal = total,
            Metodo = metodo,
            FechaPago = DateTime.UtcNow
        };

        _db.Pagos.Add(pago);
        await _db.SaveChangesAsync();

        await _orders.MarcarComoPagadoAsync(pedidoId);

        if (pedido.Tipo == TipoRestaurante.Gourmet && pedido.MesaId.HasValue)
            await _orders.LiberarMesaAsync(pedido.MesaId.Value);

        return pago;
    }

    public async Task<(decimal Ingresos, int Cantidad)> ObtenerResumenVentasAsync(DateTime desde, DateTime hasta)
    {
        var pagos = await _db.Pagos
            .Where(p => p.FechaPago >= desde && p.FechaPago <= hasta)
            .ToListAsync();

        return (pagos.Sum(p => p.MontoTotal), pagos.Count);
    }
}
