using HungryUp.Domain.Billing;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Persistence.Billing;

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
