using HungryUp.Api.Authorization;
using HungryUp.Application.Analytics;
using HungryUp.Application.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace HungryUp.Api.Analytics;

[ApiController]
[Route("api/v1/analytics")]
[Authorize]
public class AnalyticsController : ControllerBase
{
    private readonly IAnalyticsService _service;

    public AnalyticsController(IAnalyticsService service) => _service = service;

    /// <summary>Resumen rápido por periodo predefinido (compatibilidad).</summary>
    [HttpGet("sales-summary")]
    [HasPermission(Permissions.Analytics.Read)]
    public async Task<IActionResult> GetSalesSummary([FromQuery] string periodo = "dia") =>
        Ok(await _service.ObtenerResumenAsync(periodo));

    /// <summary>Tarjetas del dashboard (ingresos, órdenes, ticket, inventario…) para un rango.</summary>
    [HttpGet("dashboard")]
    [HasPermission(Permissions.Analytics.Read)]
    public async Task<IActionResult> GetDashboard(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta, [FromQuery] int umbralStockBajo = 5)
    {
        var (d, h) = Rango(desde, hasta);
        return Ok(await _service.GetDashboardAsync(d, h, umbralStockBajo));
    }

    /// <summary>Serie temporal de ventas. Filtros: granularidad (dia|semana|mes) y opcional productoId.</summary>
    [HttpGet("sales-timeseries")]
    [HasPermission(Permissions.Analytics.Read)]
    public async Task<IActionResult> GetSalesTimeseries(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] string granularidad = "dia", [FromQuery] Guid? productoId = null)
    {
        var (d, h) = Rango(desde, hasta);
        return Ok(await _service.GetSerieVentasAsync(d, h, granularidad, productoId));
    }

    /// <summary>Productos más vendidos. orderBy = cantidad|ingresos.</summary>
    [HttpGet("top-products")]
    [HasPermission(Permissions.Analytics.Read)]
    public async Task<IActionResult> GetTopProducts(
        [FromQuery] DateTime? desde, [FromQuery] DateTime? hasta,
        [FromQuery] int top = 10, [FromQuery] string orderBy = "cantidad")
    {
        var (d, h) = Rango(desde, hasta);
        return Ok(await _service.GetTopProductosAsync(d, h, top, orderBy));
    }

    /// <summary>Ventas agrupadas por método de pago.</summary>
    [HttpGet("sales-by-payment-method")]
    [HasPermission(Permissions.Analytics.Read)]
    public async Task<IActionResult> GetSalesByPaymentMethod([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var (d, h) = Rango(desde, hasta);
        return Ok(await _service.GetVentasPorMetodoAsync(d, h));
    }

    /// <summary>Ventas agrupadas por tipo de restaurante (FastFood / Gourmet).</summary>
    [HttpGet("sales-by-type")]
    [HasPermission(Permissions.Analytics.Read)]
    public async Task<IActionResult> GetSalesByType([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var (d, h) = Rango(desde, hasta);
        return Ok(await _service.GetVentasPorTipoAsync(d, h));
    }

    /// <summary>Estado del inventario: valor total y productos bajo stock.</summary>
    [HttpGet("inventory")]
    [HasPermission(Permissions.Analytics.Read)]
    public async Task<IActionResult> GetInventory([FromQuery] int umbralStockBajo = 5) =>
        Ok(await _service.GetInventarioAsync(umbralStockBajo));

    /// <summary>Reporte de ganancias/pérdidas (utilidad de caja y operativa). Requiere permiso financiero.</summary>
    [HttpGet("profit-loss")]
    [HasPermission(Permissions.Analytics.ProfitLoss)]
    public async Task<IActionResult> GetProfitLoss([FromQuery] DateTime? desde, [FromQuery] DateTime? hasta)
    {
        var (d, h) = Rango(desde, hasta);
        return Ok(await _service.GetProfitLossAsync(d, h));
    }

    /// <summary>Resuelve el rango: por defecto el mes actual. 'hasta' sin hora incluye todo el día.</summary>
    private static (DateTime desde, DateTime hasta) Rango(DateTime? desde, DateTime? hasta)
    {
        var ahora = DateTime.UtcNow;
        var d = desde ?? new DateTime(ahora.Year, ahora.Month, 1, 0, 0, 0, DateTimeKind.Utc);
        var h = hasta ?? ahora;

        // Si 'hasta' viene como fecha sin hora (medianoche), incluir el día completo.
        if (hasta.HasValue && hasta.Value.TimeOfDay == TimeSpan.Zero)
            h = hasta.Value.Date.AddDays(1).AddTicks(-1);

        if (h < d)
            throw new ArgumentException("La fecha 'hasta' no puede ser anterior a 'desde'.");

        return (d, h);
    }
}
