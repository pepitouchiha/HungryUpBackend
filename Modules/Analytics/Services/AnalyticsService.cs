using HungryUpBackend.Modules.Billing.Services;

namespace HungryUpBackend.Modules.Analytics.Services;

public class AnalyticsService : IAnalyticsService
{
    private readonly IBillingService _billing;

    public AnalyticsService(IBillingService billing) => _billing = billing;

    public async Task<SalesSummaryResult> ObtenerResumenAsync(string periodo)
    {
        var ahora = DateTime.UtcNow;
        var desde = periodo.ToLower() switch
        {
            "dia"    => ahora.Date,
            "semana" => ahora.Date.AddDays(-(int)ahora.DayOfWeek),
            "mes"    => new DateTime(ahora.Year, ahora.Month, 1),
            _        => throw new ArgumentException($"Periodo inválido: '{periodo}'. Use 'dia', 'semana' o 'mes'.")
        };

        var (ingresos, cantidad) = await _billing.ObtenerResumenVentasAsync(desde, ahora);
        return new SalesSummaryResult(ingresos, cantidad);
    }
}
