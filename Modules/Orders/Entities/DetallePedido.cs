namespace HungryUpBackend.Modules.Orders.Entities;

public class DetallePedido
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    public Pedido Pedido { get; set; } = null!;
}
