using HungryUp.Application.Analytics.Dtos;

namespace HungryUp.Application.Analytics;

public interface IAnalyticsService
{
    Task<SalesSummaryDto> ObtenerResumenAsync(string periodo);

    Task<DashboardDto> GetDashboardAsync(DateTime desde, DateTime hasta, int umbralStockBajo);

    Task<List<PuntoSerieDto>> GetSerieVentasAsync(DateTime desde, DateTime hasta, string granularidad, Guid? productoId);

    Task<List<TopProductoDto>> GetTopProductosAsync(DateTime desde, DateTime hasta, int top, string orderBy);

    Task<List<VentaPorMetodoDto>> GetVentasPorMetodoAsync(DateTime desde, DateTime hasta);

    Task<List<VentaPorTipoDto>> GetVentasPorTipoAsync(DateTime desde, DateTime hasta);

    Task<InventarioDto> GetInventarioAsync(int umbralStockBajo);

    Task<ProfitLossDto> GetProfitLossAsync(DateTime desde, DateTime hasta);
}
