namespace HungryUp.Domain.Orders;

public class Mesa
{
    public Guid Id { get; set; }
    public int Numero { get; set; }
    public EstadoMesa Estado { get; set; }

    public ICollection<Pedido> Pedidos { get; set; } = new List<Pedido>();
}
