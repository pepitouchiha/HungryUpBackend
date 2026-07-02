namespace HungryUp.Domain.Orders;

public class Mesa
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public EstadoMesa Estado { get; set; }

    /// <summary>Borrado lógico: false = eliminada/inactiva, no se muestra en listados.</summary>
    public bool Activo { get; set; } = true;

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
