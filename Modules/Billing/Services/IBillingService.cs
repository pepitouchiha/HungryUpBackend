using HungryUpBackend.Modules.Billing.Entities;

namespace HungryUpBackend.Modules.Billing.Services;

public interface IBillingService
{
    Task<Pago> ProcesarPagoAsync(Guid pedidoId, MetodoPago metodo, decimal montoPagado);
    Task<(decimal Ingresos, int Cantidad)> ObtenerResumenVentasAsync(DateTime desde, DateTime hasta);
}
