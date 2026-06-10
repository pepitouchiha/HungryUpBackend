using HungryUpBackend.Modules.Catalog.Entities;
using HungryUpBackend.Modules.Orders.Entities;

namespace HungryUpBackend;

public static class DataSeeder
{
    public static async Task SeedAsync(IServiceProvider services)
    {
        using var scope = services.CreateScope();
        var sp = scope.ServiceProvider;

        await SeedCatalogAsync(sp.GetRequiredService<CatalogDbContext>());
        await SeedOrdersAsync(sp.GetRequiredService<OrdersDbContext>());
    }

    private static async Task SeedCatalogAsync(CatalogDbContext db)
    {
        if (db.Categorias.Any()) return;

        var bebidas  = new Categoria { Id = new Guid("10000000-0000-0000-0000-000000000001"), Nombre = "Bebidas",         Activo = true };
        var rapidas  = new Categoria { Id = new Guid("10000000-0000-0000-0000-000000000002"), Nombre = "Comidas Rápidas", Activo = true };
        var postres  = new Categoria { Id = new Guid("10000000-0000-0000-0000-000000000003"), Nombre = "Postres",         Activo = true };

        db.Categorias.AddRange(bebidas, rapidas, postres);

        db.Productos.AddRange(
            new Producto { Id = Guid.NewGuid(), Nombre = "Agua Mineral 500ml",   CategoriaId = bebidas.Id, Precio =  2.50m, StockActual = 100 },
            new Producto { Id = Guid.NewGuid(), Nombre = "Coca-Cola 350ml",      CategoriaId = bebidas.Id, Precio =  3.50m, StockActual =  50 },
            new Producto { Id = Guid.NewGuid(), Nombre = "Hamburguesa Clásica",  CategoriaId = rapidas.Id, Precio = 12.90m, StockActual =  20 },
            new Producto { Id = Guid.NewGuid(), Nombre = "Pizza Margarita",      CategoriaId = rapidas.Id, Precio = 15.50m, StockActual =  15 },
            new Producto { Id = Guid.NewGuid(), Nombre = "Brownie de Chocolate", CategoriaId = postres.Id, Precio =  6.00m, StockActual =  30 }
        );

        await db.SaveChangesAsync();
    }

    private static async Task SeedOrdersAsync(OrdersDbContext db)
    {
        if (db.Mesas.Any()) return;

        db.Mesas.AddRange(
            new Mesa { Id = Guid.NewGuid(), Numero = 1, Estado = EstadoMesa.Libre },
            new Mesa { Id = Guid.NewGuid(), Numero = 2, Estado = EstadoMesa.Libre },
            new Mesa { Id = Guid.NewGuid(), Numero = 3, Estado = EstadoMesa.Libre }
        );

        await db.SaveChangesAsync();
    }
}
