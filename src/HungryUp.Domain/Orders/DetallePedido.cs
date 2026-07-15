namespace HungryUp.Domain.Orders;

public class DetallePedido
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public Guid ProductoId { get; set; }
    public int Cantidad { get; set; }
    public decimal PrecioUnitario { get; set; }

    /// <summary>Costo unitario del producto al momento de la venta (foto del costo promedio). Base para el COGS.</summary>
    public decimal CostoUnitario { get; set; }

    public Pedido Pedido { get; set; } = null!;
}
