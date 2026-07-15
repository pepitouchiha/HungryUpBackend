namespace HungryUp.Domain.Catalog;

public class Producto
{
    public Guid Id { get; set; }
    public Guid CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Precio en pesos colombianos (COP). El peso colombiano no usa centavos: valor entero.</summary>
    public decimal Precio { get; set; }

    public int StockActual { get; set; }

    /// <summary>Tarifa de IVA del producto en porcentaje (Colombia: 0 exento, 5, 19). Se usa como valor por defecto en las compras.</summary>
    public decimal TarifaIva { get; set; }

    /// <summary>Costo promedio ponderado de adquisición. Se recalcula al confirmar compras y se usa para el COGS.</summary>
    public decimal CostoPromedio { get; set; }

    /// <summary>Ruta interna (/images/products/...) o URL de la imagen del producto. Opcional.</summary>
    public string? ImagenUrl { get; set; }

    /// <summary>Borrado lógico: false = eliminado/inactivo, no se muestra en listados.</summary>
    public bool Activo { get; set; } = true;

    public Categoria Categoria { get; set; } = null!;
}
