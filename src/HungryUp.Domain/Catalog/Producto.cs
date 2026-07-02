namespace HungryUp.Domain.Catalog;

public class Producto
{
    public Guid Id { get; set; }
    public Guid CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Precio en pesos colombianos (COP). El peso colombiano no usa centavos: valor entero.</summary>
    public decimal Precio { get; set; }

    public int StockActual { get; set; }

    /// <summary>Ruta interna (/images/products/...) o URL de la imagen del producto. Opcional.</summary>
    public string? ImagenUrl { get; set; }

    /// <summary>Borrado lógico: false = eliminado/inactivo, no se muestra en listados.</summary>
    public bool Activo { get; set; } = true;

    public Categoria Categoria { get; set; } = null!;
}
