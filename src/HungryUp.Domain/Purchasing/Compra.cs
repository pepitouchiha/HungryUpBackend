namespace HungryUp.Domain.Purchasing;

/// <summary>Factura de compra a un proveedor. Al confirmarse, aumenta el inventario de los productos de sus líneas.</summary>
public class Compra
{
    public Guid Id { get; set; }

    /// <summary>Número de la factura del proveedor.</summary>
    public string NumeroFactura { get; set; } = string.Empty;

    public string NombreProveedor { get; set; } = string.Empty;

    /// <summary>NIT/identificación del proveedor. Opcional.</summary>
    public string? NitProveedor { get; set; }

    /// <summary>Fecha de la factura de compra.</summary>
    public DateTime Fecha { get; set; }

    /// <summary>Notas u observaciones de la compra. Opcional.</summary>
    public string? Notas { get; set; }

    public EstadoCompra Estado { get; set; } = EstadoCompra.Borrador;

    // --- Retenciones aplicadas a nivel factura (Colombia) ---

    /// <summary>Retención en la fuente (%) sobre el subtotal (base gravable).</summary>
    public decimal ReteFuentePorc { get; set; }

    /// <summary>Retención de IVA (%) sobre el valor total del IVA.</summary>
    public decimal ReteIvaPorc { get; set; }

    /// <summary>Retención de ICA (por mil) sobre el subtotal.</summary>
    public decimal ReteIcaPorMil { get; set; }

    public DateTime FechaCreacion { get; set; }

    /// <summary>Momento (UTC) en que se confirmó la compra y se aumentó el inventario. Null si aún es borrador.</summary>
    public DateTime? FechaConfirmacion { get; set; }

    public ICollection<LineaCompra> Lineas { get; set; } = new List<LineaCompra>();
}
