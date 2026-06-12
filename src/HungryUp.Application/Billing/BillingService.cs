using HungryUp.Application.Billing.Dtos;
using HungryUp.Application.Orders;
using HungryUp.Domain.Billing;
using HungryUp.Domain.Orders;
using HungryUp.Persistence.Billing;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Application.Billing;

public class BillingService : IBillingService
{
    private readonly BillingDbContext _db;
    private readonly IOrdersService _orders;

    public BillingService(BillingDbContext db, IOrdersService orders)
    {
        _db = db;
        _orders = orders;
    }

    public async Task<PagoDto> ProcesarPagoAsync(ProcesarPagoDto dto)
    {
        var pedido = await _orders.ObtenerPedidoPorIdAsync(dto.PedidoId)
            ?? throw new KeyNotFoundException($"Pedido {dto.PedidoId} no encontrado.");

        if (pedido.EstadoFin == EstadoFinanciero.Pagado)
            throw new InvalidOperationException("El pedido ya se encuentra pagado.");

        var total = pedido.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);

        var pago = new Pago
        {
            Id = Guid.NewGuid(),
            PedidoId = dto.PedidoId,
            MontoTotal = total,
            Metodo = dto.Metodo,
            FechaPago = DateTime.UtcNow
        };

        _db.Pagos.Add(pago);
        await _db.SaveChangesAsync();

        await _orders.MarcarComoPagadoAsync(dto.PedidoId);

        if (pedido.Tipo == TipoRestaurante.Gourmet && pedido.MesaId.HasValue)
            await _orders.LiberarMesaAsync(pedido.MesaId.Value);

        return new PagoDto(pago.Id, pago.PedidoId, pago.MontoTotal, pago.Metodo, pago.FechaPago);
    }

    public async Task<ResumenVentasDto> ObtenerResumenVentasAsync(DateTime desde, DateTime hasta)
    {
        var pagos = await _db.Pagos
            .Where(p => p.FechaPago >= desde && p.FechaPago <= hasta)
            .ToListAsync();

        return new ResumenVentasDto(pagos.Sum(p => p.MontoTotal), pagos.Count);
    }
}
