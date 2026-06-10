namespace HungryUpBackend.Modules.Analytics.Services;

public interface IAnalyticsService
{
    Task<SalesSummaryResult> ObtenerResumenAsync(string periodo);
}

public record SalesSummaryResult(decimal IngresosTotales, int CantidadOrdenes);
