namespace HungryUp.Domain.Catalog;

public class Producto
{
    public Guid Id { get; set; }
    public Guid CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;

    /// <summary>Precio en pesos colombianos (COP). El peso colombiano no usa centavos: valor entero.</summary>
    public decimal Precio { get; set; }

    public int StockActual { get; set; }

    /// <summary>URL de la imagen del producto. Opcional.</summary>
    public string? ImagenUrl { get; set; }

    public Categoria Categoria { get; set; } = null!;
}
