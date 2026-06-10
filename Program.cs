using System.Text.Json.Serialization;
using HungryUpBackend;
using Scalar.AspNetCore;
using HungryUpBackend.Modules.Analytics.Services;
using HungryUpBackend.Modules.Billing.Entities;
using HungryUpBackend.Modules.Billing.Services;
using HungryUpBackend.Modules.Catalog.Entities;
using HungryUpBackend.Modules.Catalog.Services;
using HungryUpBackend.Modules.Orders.Entities;
using HungryUpBackend.Modules.Orders.Services;
using Microsoft.EntityFrameworkCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opt.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });

// Un solo archivo SQLite, tres DbContexts aislados con historial de migraciones independiente
var connStr = builder.Configuration.GetConnectionString("HungryUpDb")!;

builder.Services.AddDbContext<CatalogDbContext>(opt =>
    opt.UseSqlite(connStr, x => x.MigrationsHistoryTable("__EFMigrationsHistory_Catalog")));

builder.Services.AddDbContext<OrdersDbContext>(opt =>
    opt.UseSqlite(connStr, x => x.MigrationsHistoryTable("__EFMigrationsHistory_Orders")));

builder.Services.AddDbContext<BillingDbContext>(opt =>
    opt.UseSqlite(connStr, x => x.MigrationsHistoryTable("__EFMigrationsHistory_Billing")));

// Servicios de dominio
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

// Manejo global de errores
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

// Aplica migraciones pendientes y carga datos de prueba
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    services.GetRequiredService<CatalogDbContext>().Database.Migrate();
    services.GetRequiredService<OrdersDbContext>().Database.Migrate();
    services.GetRequiredService<BillingDbContext>().Database.Migrate();
}
await DataSeeder.SeedAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.MapScalarApiReference(opt =>
    {
        opt.Title = "HungryUp API";
        opt.Theme = ScalarTheme.DeepSpace;
    });
}

app.UseExceptionHandler();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
