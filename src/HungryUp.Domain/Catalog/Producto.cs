namespace HungryUp.Domain.Catalog;

public class Producto
{
    public Guid Id { get; set; }
    public Guid CategoriaId { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public decimal Precio { get; set; }
    public int StockActual { get; set; }

    public Categoria Categoria { get; set; } = null!;
}
