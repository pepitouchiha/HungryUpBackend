using HungryUp.Domain.Purchasing;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Persistence.Purchasing;

public class PurchasingDbContext : DbContext
{
    public PurchasingDbContext(DbContextOptions<PurchasingDbContext> options) : base(options) { }

    public DbSet<Compra> Compras => Set<Compra>();
    public DbSet<LineaCompra> LineasCompra => Set<LineaCompra>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Compra>(e =>
        {
            e.Property(c => c.NumeroFactura).HasMaxLength(60).IsRequired();
            e.Property(c => c.NombreProveedor).HasMaxLength(200).IsRequired();
            e.Property(c => c.NitProveedor).HasMaxLength(40);
            e.Property(c => c.Notas).HasMaxLength(2000);
            e.Property(c => c.ReteFuentePorc).HasPrecision(5, 2);
            e.Property(c => c.ReteIvaPorc).HasPrecision(5, 2);
            e.Property(c => c.ReteIcaPorMil).HasPrecision(6, 3);

            e.HasMany(c => c.Lineas)
                .WithOne(l => l.Compra)
                .HasForeignKey(l => l.CompraId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<LineaCompra>(e =>
        {
            e.Property(l => l.ProductoNombre).HasMaxLength(200);
            e.Property(l => l.CostoUnitario).HasPrecision(18, 2);
            e.Property(l => l.TarifaIva).HasPrecision(5, 2);
        });
    }
}
