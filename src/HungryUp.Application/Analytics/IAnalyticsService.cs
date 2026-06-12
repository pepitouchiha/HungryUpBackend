using HungryUp.Application.Analytics.Dtos;

namespace HungryUp.Application.Analytics;

public interface IAnalyticsService
{
    Task<SalesSummaryDto> ObtenerResumenAsync(string periodo);
}
