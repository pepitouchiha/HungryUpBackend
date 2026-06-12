using System.Text.Json.Serialization;
using HungryUp.Api;
using HungryUp.Application.Analytics;
using HungryUp.Application.Auth;
using HungryUp.Application.Billing;
using HungryUp.Application.Catalog;
using HungryUp.Application.Orders;
using HungryUp.Persistence.Billing;
using HungryUp.Persistence.Catalog;
using HungryUp.Persistence.Orders;
using Microsoft.EntityFrameworkCore;
using Scalar.AspNetCore;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi();

builder.Services.AddControllers()
    .AddJsonOptions(opt =>
    {
        opt.JsonSerializerOptions.Converters.Add(new JsonStringEnumConverter());
        opt.JsonSerializerOptions.ReferenceHandler = ReferenceHandler.IgnoreCycles;
    });

var connStr = builder.Configuration.GetConnectionString("HungryUpDb")!;

builder.Services.AddDbContext<CatalogDbContext>(opt =>
    opt.UseSqlite(connStr, x => x
        .MigrationsHistoryTable("__EFMigrationsHistory_Catalog")
        .MigrationsAssembly("HungryUp.Persistence")));

builder.Services.AddDbContext<OrdersDbContext>(opt =>
    opt.UseSqlite(connStr, x => x
        .MigrationsHistoryTable("__EFMigrationsHistory_Orders")
        .MigrationsAssembly("HungryUp.Persistence")));

builder.Services.AddDbContext<BillingDbContext>(opt =>
    opt.UseSqlite(connStr, x => x
        .MigrationsHistoryTable("__EFMigrationsHistory_Billing")
        .MigrationsAssembly("HungryUp.Persistence")));

builder.Services.AddSingleton<IAuthService, AuthService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();

builder.Services.AddCors(opts => opts.AddDefaultPolicy(policy =>
    policy.WithOrigins("http://localhost:4200")
          .AllowAnyHeader()
          .AllowAnyMethod()));

builder.Services.AddExceptionHandler<GlobalExceptionHandler>();
builder.Services.AddProblemDetails();

var app = builder.Build();

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
app.UseCors();
app.UseHttpsRedirection();
app.MapControllers();
app.Run();
