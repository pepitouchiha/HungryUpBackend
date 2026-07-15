namespace HungryUp.Application.Analytics.Dtos;

/// <summary>Tarjetas de resumen del dashboard para un rango de fechas.</summary>
public record DashboardDto(
    DateTime Desde,
    DateTime Hasta,
    decimal Ingresos,
    int Ordenes,
    decimal TicketPromedio,
    int UnidadesVendidas,
    decimal TotalComprado,
    decimal ValorInventario,
    int ProductosBajoStock);

/// <summary>Punto de una serie temporal de ventas.</summary>
public record PuntoSerieDto(string Periodo, decimal Ingresos, int Ordenes, int Unidades);

public record TopProductoDto(Guid ProductoId, string Nombre, int Cantidad, decimal Ingresos);

public record VentaPorMetodoDto(string Metodo, decimal Ingresos, int Pagos);

public record VentaPorTipoDto(string Tipo, decimal Ingresos, int Ordenes, int Unidades);

public record ProductoStockDto(Guid ProductoId, string Nombre, int StockActual, decimal CostoPromedio, decimal ValorStock);

/// <summary>Estado del inventario: valor total (stock × costo promedio) y productos por debajo del umbral.</summary>
public record InventarioDto(decimal ValorTotal, int Productos, int UmbralStockBajo, List<ProductoStockDto> BajoStock);

/// <summary>
/// Reporte de ganancias/pérdidas con dos lecturas:
/// utilidad de caja (Ingresos − Compras del periodo − Salarios) y
/// utilidad operativa (Ingresos − COGS − Salarios).
/// </summary>
public record ProfitLossDto(
    DateTime Desde,
    DateTime Hasta,
    decimal Ingresos,
    decimal ComprasDelPeriodo,
    decimal Cogs,
    decimal Salarios,
    int Empleados,
    int DiasNomina,
    decimal UtilidadCaja,
    decimal UtilidadOperativa);
