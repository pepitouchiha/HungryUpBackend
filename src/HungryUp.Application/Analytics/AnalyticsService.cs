using HungryUp.Application.Analytics.Dtos;
using HungryUp.Application.Billing;

namespace HungryUp.Application.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IBillingService _billing;

    public AnalyticsService(IBillingService billing) => _billing = billing;

    public async Task<SalesSummaryDto> ObtenerResumenAsync(string periodo)
    {
        var ahora = DateTime.UtcNow;
        var desde = periodo.ToLower() switch
        {
            "dia"    => ahora.Date,
            "semana" => ahora.Date.AddDays(-(int)ahora.DayOfWeek),
            "mes"    => new DateTime(ahora.Year, ahora.Month, 1),
            _        => throw new ArgumentException($"Periodo inválido: '{periodo}'. Use 'dia', 'semana' o 'mes'.")
        };

        var resumen = await _billing.ObtenerResumenVentasAsync(desde, ahora);
        return new SalesSummaryDto(resumen.Ingresos, resumen.Cantidad);
    }
}
