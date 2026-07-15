using System.Globalization;
using HungryUp.Application.Analytics.Dtos;
using HungryUp.Application.Billing;
using HungryUp.Application.Catalog;
using HungryUp.Application.Orders;
using HungryUp.Application.Orders.Dtos;
using HungryUp.Application.Payroll;
using HungryUp.Application.Purchasing;

namespace HungryUp.Application.Analytics;

public class AnalyticsService : IAnalyticsService
{
    private readonly IBillingService _billing;
    private readonly IOrdersService _orders;
    private readonly ICatalogService _catalog;
    private readonly IPurchasingService _purchasing;
    private readonly IPayrollService _payroll;

    public AnalyticsService(
        IBillingService billing,
        IOrdersService orders,
        ICatalogService catalog,
        IPurchasingService purchasing,
        IPayrollService payroll)
    {
        _billing = billing;
        _orders = orders;
        _catalog = catalog;
        _purchasing = purchasing;
        _payroll = payroll;
    }

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

    public async Task<DashboardDto> GetDashboardAsync(DateTime desde, DateTime hasta, int umbralStockBajo)
    {
        var pagados = await _orders.GetPagadosAsync(desde, hasta);
        var ingresos = Round(pagados.Sum(Ingreso));
        var unidades = pagados.Sum(Unidades);
        var ordenes = pagados.Count;

        var totalComprado = await _purchasing.GetTotalCompradoAsync(desde, hasta);

        var productos = await _catalog.GetProductosAsync();
        var activos = productos.Where(p => p.Activo).ToList();
        var valorInventario = Round(activos.Sum(p => p.StockActual * p.CostoPromedio));
        var bajoStock = activos.Count(p => p.StockActual < umbralStockBajo);

        return new DashboardDto(
            desde, hasta,
            ingresos,
            ordenes,
            ordenes > 0 ? Round(ingresos / ordenes) : 0m,
            unidades,
            totalComprado,
            valorInventario,
            bajoStock);
    }

    public async Task<List<PuntoSerieDto>> GetSerieVentasAsync(
        DateTime desde, DateTime hasta, string granularidad, Guid? productoId)
    {
        var pagados = await _orders.GetPagadosAsync(desde, hasta);

        return pagados
            .Select(p => new
            {
                Etiqueta = Bucket(p.FechaCreacion, granularidad),
                Detalles = productoId is null ? p.Detalles : p.Detalles.Where(d => d.ProductoId == productoId).ToList()
            })
            .Where(x => x.Detalles.Count > 0) // con filtro de producto, ignora pedidos que no lo contienen
            .GroupBy(x => x.Etiqueta)
            .Select(g => new PuntoSerieDto(
                g.Key,
                Round(g.Sum(x => x.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario))),
                g.Count(),
                g.Sum(x => x.Detalles.Sum(d => d.Cantidad))))
            .OrderBy(x => x.Periodo)
            .ToList();
    }

    public async Task<List<TopProductoDto>> GetTopProductosAsync(
        DateTime desde, DateTime hasta, int top, string orderBy)
    {
        var pagados = await _orders.GetPagadosAsync(desde, hasta);
        var nombres = (await _catalog.GetProductosAsync()).ToDictionary(p => p.Id, p => p.Nombre);

        var agrupado = pagados
            .SelectMany(p => p.Detalles)
            .GroupBy(d => d.ProductoId)
            .Select(g => new TopProductoDto(
                g.Key,
                nombres.GetValueOrDefault(g.Key, "(desconocido)"),
                g.Sum(d => d.Cantidad),
                Round(g.Sum(d => d.Cantidad * d.PrecioUnitario))));

        agrupado = orderBy.ToLower() == "ingresos"
            ? agrupado.OrderByDescending(x => x.Ingresos)
            : agrupado.OrderByDescending(x => x.Cantidad);

        return agrupado.Take(top < 1 ? 10 : top).ToList();
    }

    public async Task<List<VentaPorMetodoDto>> GetVentasPorMetodoAsync(DateTime desde, DateTime hasta)
    {
        var pagos = await _billing.GetPagosAsync(desde, hasta);

        return pagos
            .GroupBy(p => p.Metodo)
            .Select(g => new VentaPorMetodoDto(g.Key.ToString(), Round(g.Sum(p => p.MontoTotal)), g.Count()))
            .OrderByDescending(x => x.Ingresos)
            .ToList();
    }

    public async Task<List<VentaPorTipoDto>> GetVentasPorTipoAsync(DateTime desde, DateTime hasta)
    {
        var pagados = await _orders.GetPagadosAsync(desde, hasta);

        return pagados
            .GroupBy(p => p.Tipo)
            .Select(g => new VentaPorTipoDto(
                g.Key.ToString(),
                Round(g.Sum(Ingreso)),
                g.Count(),
                g.Sum(Unidades)))
            .OrderByDescending(x => x.Ingresos)
            .ToList();
    }

    public async Task<InventarioDto> GetInventarioAsync(int umbralStockBajo)
    {
        var activos = (await _catalog.GetProductosAsync()).Where(p => p.Activo).ToList();

        var valorTotal = Round(activos.Sum(p => p.StockActual * p.CostoPromedio));

        var bajoStock = activos
            .Where(p => p.StockActual < umbralStockBajo)
            .OrderBy(p => p.StockActual)
            .Select(p => new ProductoStockDto(p.Id, p.Nombre, p.StockActual, p.CostoPromedio, Round(p.StockActual * p.CostoPromedio)))
            .ToList();

        return new InventarioDto(valorTotal, activos.Count, umbralStockBajo, bajoStock);
    }

    public async Task<ProfitLossDto> GetProfitLossAsync(DateTime desde, DateTime hasta)
    {
        var pagados = await _orders.GetPagadosAsync(desde, hasta);
        var ingresos = Round(pagados.Sum(Ingreso));
        var cogs = Round(pagados.Sum(p => p.Detalles.Sum(d => d.Cantidad * d.CostoUnitario)));

        var comprasDelPeriodo = await _purchasing.GetTotalCompradoAsync(desde, hasta);
        var nomina = await _payroll.CalcularNominaAsync(desde, hasta);

        return new ProfitLossDto(
            desde, hasta,
            ingresos,
            comprasDelPeriodo,
            cogs,
            nomina.Total,
            nomina.Empleados,
            nomina.Dias,
            Round(ingresos - comprasDelPeriodo - nomina.Total),
            Round(ingresos - cogs - nomina.Total));
    }

    // ---------- Helpers ----------

    private static decimal Ingreso(PedidoDto p) => p.Detalles.Sum(d => d.Cantidad * d.PrecioUnitario);
    private static int Unidades(PedidoDto p) => p.Detalles.Sum(d => d.Cantidad);

    private static decimal Round(decimal v) => Math.Round(v, 2, MidpointRounding.AwayFromZero);

    private static string Bucket(DateTime fecha, string granularidad) => granularidad.ToLower() switch
    {
        "semana" => fecha.Date.AddDays(-(int)fecha.DayOfWeek).ToString("yyyy-MM-dd"), // inicio de semana (domingo)
        "mes"    => fecha.ToString("yyyy-MM", CultureInfo.InvariantCulture),
        "dia"    => fecha.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture),
        _        => throw new ArgumentException($"Granularidad inválida: '{granularidad}'. Use 'dia', 'semana' o 'mes'.")
    };
}
