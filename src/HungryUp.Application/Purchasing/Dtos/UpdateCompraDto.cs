namespace HungryUp.Application.Purchasing.Dtos;

/// <summary>Datos para editar una factura de compra. Solo permitido mientras esté en Borrador.</summary>
public record UpdateCompraDto(
    string NumeroFactura,
    string NombreProveedor,
    string? NitProveedor,
    DateTime? Fecha,
    string? Notas,
    decimal ReteFuentePorc,
    decimal ReteIvaPorc,
    decimal ReteIcaPorMil,
    List<CreateLineaCompraDto> Items);

/// <summary>Actualización aislada de las notas de una compra (permitido en cualquier estado salvo Anulada).</summary>
public record UpdateNotasCompraDto(string? Notas);
