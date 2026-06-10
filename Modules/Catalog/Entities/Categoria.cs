namespace HungryUpBackend.Modules.Catalog.Entities;

public class Categoria
{
    public Guid Id { get; set; }
    public string Nombre { get; set; } = string.Empty;
    public bool Activo { get; set; }

    public ICollection<Producto> Productos { get; set; } = new List<Producto>();
}
