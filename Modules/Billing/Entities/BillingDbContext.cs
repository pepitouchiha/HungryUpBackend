using Microsoft.EntityFrameworkCore;

namespace HungryUpBackend.Modules.Billing.Entities;

public class BillingDbContext : DbContext
{
    public BillingDbContext(DbContextOptions<BillingDbContext> options) : base(options) { }

    public DbSet<Pago> Pagos => Set<Pago>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Pago>()
            .Property(p => p.MontoTotal)
            .HasPrecision(18, 2);
    }
}
