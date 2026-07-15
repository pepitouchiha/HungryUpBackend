namespace HungryUp.Domain.Purchasing;

/// <summary>Línea de una factura de compra: un producto, su cantidad y el costo unitario de adquisición.</summary>
public class LineaCompra
{
    public Guid Id { get; set; }
    public Guid CompraId { get; set; }

    /// <summary>Producto del catálogo cuyo inventario se aumentará al confirmar la compra.</summary>
    public Guid ProductoId { get; set; }

    /// <summary>Nombre del producto capturado al momento de la compra (histórico).</summary>
    public string ProductoNombre { get; set; } = string.Empty;

    public int Cantidad { get; set; }

    /// <summary>Costo unitario de compra (sin IVA).</summary>
    public decimal CostoUnitario { get; set; }

    /// <summary>Tarifa de IVA (%) aplicada a esta línea. Por defecto se toma del producto.</summary>
    public decimal TarifaIva { get; set; }

    public Compra Compra { get; set; } = null!;
}
