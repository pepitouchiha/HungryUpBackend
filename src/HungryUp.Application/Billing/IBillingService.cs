using HungryUp.Application.Billing.Dtos;

namespace HungryUp.Application.Billing;

public interface IBillingService
{
    Task<PagoDto> ProcesarPagoAsync(ProcesarPagoDto dto);
    Task<ResumenVentasDto> ObtenerResumenVentasAsync(DateTime desde, DateTime hasta);
}
