using HungryUp.Domain.Catalog;
using Microsoft.EntityFrameworkCore;

namespace HungryUp.Persistence.Catalog;

public class CatalogDbContext : DbContext
{
    public CatalogDbContext(DbContextOptions<CatalogDbContext> options) : base(options) { }

    public DbSet<Categoria> Categorias => Set<Categoria>();
    public DbSet<Producto> Productos => Set<Producto>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Precio en pesos colombianos (COP): moneda sin centavos, se almacena como valor entero.
        modelBuilder.Entity<Producto>()
            .Property(p => p.Precio)
            .HasPrecision(18, 0);

        // Imagen opcional del producto.
        modelBuilder.Entity<Producto>()
            .Property(p => p.ImagenUrl)
            .HasMaxLength(2048);

        // Tarifa de IVA en porcentaje (ej. 19.00).
        modelBuilder.Entity<Producto>()
            .Property(p => p.TarifaIva)
            .HasPrecision(5, 2);

        // Costo promedio ponderado de adquisición.
        modelBuilder.Entity<Producto>()
            .Property(p => p.CostoPromedio)
            .HasPrecision(18, 2);
    }
}
