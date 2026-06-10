using Microsoft.EntityFrameworkCore;

namespace HungryUpBackend.Modules.Orders.Entities;

public class OrdersDbContext : DbContext
{
    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options) { }

    public DbSet<Mesa> Mesas => Set<Mesa>();
    public DbSet<Pedido> Pedidos => Set<Pedido>();
    public DbSet<DetallePedido> DetallesPedido => Set<DetallePedido>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<DetallePedido>()
            .Property(d => d.PrecioUnitario)
            .HasPrecision(18, 2);
    }
}
