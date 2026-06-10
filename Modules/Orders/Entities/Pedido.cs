namespace HungryUpBackend.Modules.Orders.Entities;

public class Pedido
{
    public Guid Id { get; set; }
    public Guid? MesaId { get; set; }
    public DateTime FechaCreacion { get; set; }
    public EstadoPreparacion EstadoPrep { get; set; }
    public EstadoFinanciero EstadoFin { get; set; }
    public TipoRestaurante Tipo { get; set; }
    public int NumeroTurno { get; set; }

    public Mesa? Mesa { get; set; }
    public ICollection<DetallePedido> Detalles { get; set; } = new List<DetallePedido>();
}
