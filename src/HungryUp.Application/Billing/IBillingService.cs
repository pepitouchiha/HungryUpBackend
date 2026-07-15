using HungryUp.Application.Billing.Dtos;

namespace HungryUp.Application.Billing;

public interface IBillingService
{
    Task<PagoDto> ProcesarPagoAsync(ProcesarPagoDto dto);
    Task<ResumenVentasDto> ObtenerResumenVentasAsync(DateTime desde, DateTime hasta);

    /// <summary>Pagos registrados en el rango [desde, hasta] (por fecha de pago). Para analítica.</summary>
    Task<List<PagoDto>> GetPagosAsync(DateTime desde, DateTime hasta);
}
