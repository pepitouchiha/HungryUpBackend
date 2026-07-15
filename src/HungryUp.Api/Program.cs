using System.Text;
using System.Text.Json.Serialization;
using HungryUp.Api;
using HungryUp.Api.Authorization;
using Microsoft.AspNetCore.Authorization;
using HungryUp.Application.Analytics;
using HungryUp.Application.Auth;
using HungryUp.Application.Billing;
using HungryUp.Application.Catalog;
using HungryUp.Application.Orders;
using HungryUp.Application.Payroll;
using HungryUp.Application.Purchasing;
using HungryUp.Persistence.Auth;
using HungryUp.Persistence.Billing;
using HungryUp.Persistence.Catalog;
using HungryUp.Persistence.Orders;
using HungryUp.Persistence.Payroll;
using HungryUp.Persistence.Purchasing;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
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

builder.Services.AddDbContext<AuthDbContext>(opt =>
    opt.UseSqlite(connStr, x => x
        .MigrationsHistoryTable("__EFMigrationsHistory_Auth")
        .MigrationsAssembly("HungryUp.Persistence")));

builder.Services.AddDbContext<PurchasingDbContext>(opt =>
    opt.UseSqlite(connStr, x => x
        .MigrationsHistoryTable("__EFMigrationsHistory_Purchasing")
        .MigrationsAssembly("HungryUp.Persistence")));

builder.Services.AddDbContext<PayrollDbContext>(opt =>
    opt.UseSqlite(connStr, x => x
        .MigrationsHistoryTable("__EFMigrationsHistory_Payroll")
        .MigrationsAssembly("HungryUp.Persistence")));

builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>()!;

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(opt =>
    {
        opt.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.Key)),
            ClockSkew = TimeSpan.Zero
        };
    });
builder.Services.AddAuthorization();
// Autorización basada en permisos: políticas creadas al vuelo + handler que resuelve permisos por rol.
builder.Services.AddSingleton<IAuthorizationPolicyProvider, PermissionPolicyProvider>();
builder.Services.AddScoped<IAuthorizationHandler, PermissionAuthorizationHandler>();

builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICatalogService, CatalogService>();
builder.Services.AddScoped<IOrdersService, OrdersService>();
builder.Services.AddScoped<IMesaService, MesaService>();
builder.Services.AddScoped<IBillingService, BillingService>();
builder.Services.AddScoped<IAnalyticsService, AnalyticsService>();
builder.Services.AddScoped<IPurchasingService, PurchasingService>();
builder.Services.AddScoped<IPayrollService, PayrollService>();

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
    services.GetRequiredService<AuthDbContext>().Database.Migrate();
    services.GetRequiredService<PurchasingDbContext>().Database.Migrate();
    services.GetRequiredService<PayrollDbContext>().Database.Migrate();
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

// HSTS solo fuera de desarrollo: en localhost/HTTP rompería las pruebas.
// En producción detrás de un proxy, recuerda configurar ForwardedHeaders para
// que la API detecte correctamente el esquema HTTPS original.
if (!app.Environment.IsDevelopment())
    app.UseHsts();

app.UseCors();
app.UseHttpsRedirection();
app.UseStaticFiles(); // sirve las imágenes de productos desde wwwroot
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.Run();
