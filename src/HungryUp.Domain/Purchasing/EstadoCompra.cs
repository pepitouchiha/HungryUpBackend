namespace HungryUp.Domain.Purchasing;

/// <summary>
/// Estado de una factura de compra.
/// Borrador: editable, no afecta inventario. Confirmada: sumó stock, bloqueada. Anulada: revirtió el stock.
/// </summary>
public enum EstadoCompra
{
    Borrador,
    Confirmada,
    Anulada
}
