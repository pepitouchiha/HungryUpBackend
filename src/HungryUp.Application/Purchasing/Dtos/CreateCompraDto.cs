namespace HungryUp.Application.Purchasing.Dtos;

/// <summary>Datos para crear una factura de compra (nace en estado Borrador).</summary>
public record CreateCompraDto(
    string NumeroFactura,
    string NombreProveedor,
    string? NitProveedor,
    DateTime? Fecha,
    string? Notas,
    decimal ReteFuentePorc,
    decimal ReteIvaPorc,
    decimal ReteIcaPorMil,
    List<CreateLineaCompraDto> Items);

/// <summary>
/// Línea de compra. Si <see cref="TarifaIva"/> es null, se toma la tarifa de IVA configurada en el producto.
/// </summary>
public record CreateLineaCompraDto(
    Guid ProductoId,
    int Cantidad,
    decimal CostoUnitario,
    decimal? TarifaIva = null);
