namespace HungryUpBackend.Modules.Billing.Entities;

public class Pago
{
    public Guid Id { get; set; }
    public Guid PedidoId { get; set; }
    public decimal MontoTotal { get; set; }
    public MetodoPago Metodo { get; set; }
    public DateTime FechaPago { get; set; }
}
