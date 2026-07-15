using HungryUp.Domain.Purchasing;

namespace HungryUp.Application.Purchasing.Dtos;

/// <summary>Vista completa de una factura de compra con sus totales e impuestos ya calculados.</summary>
public record CompraDto(
    Guid Id,
    string NumeroFactura,
    string NombreProveedor,
    string? NitProveedor,
    DateTime Fecha,
    string? Notas,
    EstadoCompra Estado,
    decimal ReteFuentePorc,
    decimal ReteIvaPorc,
    decimal ReteIcaPorMil,
    DateTime FechaCreacion,
    DateTime? FechaConfirmacion,
    List<LineaCompraDto> Lineas,
    // Totales calculados
    decimal Subtotal,
    decimal IvaTotal,
    decimal ReteFuenteValor,
    decimal ReteIvaValor,
    decimal ReteIcaValor,
    decimal TotalRetenciones,
    decimal TotalBruto,
    decimal TotalAPagar);

public record LineaCompraDto(
    Guid Id,
    Guid ProductoId,
    string ProductoNombre,
    int Cantidad,
    decimal CostoUnitario,
    decimal TarifaIva,
    decimal Subtotal,
    decimal IvaValor,
    decimal Total);
