# HungryUp Backend

> REST API para automatización de restaurantes — construida con **Clean Architecture** en .NET 9.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4?style=flat-square)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=flat-square&logo=sqlite&logoColor=white)
![Architecture](https://img.shields.io/badge/Architecture-Clean%20Architecture-blue?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)

---

## ¿Qué es HungryUp?

HungryUp es el backend para un sistema de gestión de restaurantes que soporta dos flujos operacionales:

| Flujo | Descripción |
|---|---|
| **FastFood** | Orden → Pago inmediato → Cocina. Sin mesa asignada. |
| **Gourmet** | Orden → Consumo incremental → Pago y cierre al final. Mesa requerida. |

---

## Arquitectura

El proyecto sigue **Clean Architecture** con 4 capas separadas en proyectos independientes. La regla de dependencia es estricta: las capas internas nunca conocen a las externas.

```
HungryUpBackend/
├── HungryUpBackend.sln
├── db-migrate.sh
└── src/
    ├── HungryUp.Domain/          # Entidades y enums (sin dependencias externas)
    │   ├── Common/
    │   │   └── BaseEntity.cs
    │   ├── Catalog/              # Producto, Categoria
    │   ├── Orders/               # Pedido, Mesa, DetallePedido, enums
    │   └── Billing/              # Pago, MetodoPago
    │
    ├── HungryUp.Persistence/     # DbContexts y migraciones (→ Domain)
    │   ├── Catalog/
    │   │   ├── CatalogDbContext.cs
    │   │   └── Migrations/
    │   ├── Orders/
    │   │   ├── OrdersDbContext.cs
    │   │   └── Migrations/
    │   └── Billing/
    │       ├── BillingDbContext.cs
    │       └── Migrations/
    │
    ├── HungryUp.Application/     # Servicios, interfaces y DTOs (→ Domain + Persistence)
    │   ├── Auth/
    │   ├── Catalog/
    │   ├── Orders/
    │   ├── Billing/
    │   └── Analytics/
    │
    └── HungryUp.Api/             # Controllers, Program.cs (→ Application + Persistence)
        ├── Auth/
        ├── Catalog/
        ├── Orders/
        ├── Billing/
        ├── Analytics/
        └── Properties/
            └── launchSettings.json
```

### Cadena de dependencias

```
Api  →  Application  →  Persistence  →  Domain
```

Cada dominio tiene su propio `DbContext` aislado con tabla de migraciones independiente (`__EFMigrationsHistory_Catalog`, `__EFMigrationsHistory_Orders`, `__EFMigrationsHistory_Billing`).

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core 9 — Minimal Hosting |
| ORM | Entity Framework Core 9 |
| Base de datos | SQLite — 3 DbContexts aislados |
| API Docs | Scalar UI (`/scalar/v1`) |
| Serialización | `System.Text.Json` + enums como strings |
| Auth | Credenciales hardcodeadas (sin JWT), token base64 |

---

## Endpoints

### Auth
```
POST /api/auth/login                   Iniciar sesión → devuelve sesión con token
```

### Catalog
```
GET  /api/v1/products                  Productos con stock disponible
POST /api/v1/products                  Crear producto
GET  /api/v1/products/{id}             Obtener producto por ID
GET  /api/v1/products/categorias       Listar todas las categorías
```

### Orders
```
GET  /api/v1/orders                    Listar pedidos (filtro ?estadoPrep=Pendiente|EnPreparacion|Entregado)
POST /api/v1/orders                    Crear orden (FastFood o Gourmet)
GET  /api/v1/orders/{id}              Obtener pedido por ID
PUT  /api/v1/orders/{id}/status        Cambiar estado de preparación
GET  /api/v1/orders/mesas              Listar mesas con su estado
POST /api/v1/orders/mesas/{id}/liberar Liberar una mesa
```

### Billing
```
POST /api/v1/billing/pay               Procesar pago de una orden
GET  /api/v1/billing/resumen           Resumen de ventas
```

### Analytics
```
GET  /api/v1/analytics/sales-summary?periodo=dia|semana|mes
```

---

## Inicio rápido

### Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [dotnet-ef CLI](https://learn.microsoft.com/ef/core/cli/dotnet): `dotnet tool install -g dotnet-ef`

### Ejecución

```bash
git clone https://github.com/pepitouchiha/HungryUpBackend.git
cd HungryUpBackend
dotnet run --project src/HungryUp.Api
```

Al arrancar el sistema aplica automáticamente las migraciones pendientes y carga datos de prueba (categorías, productos y mesas).

API disponible en: **http://localhost:5216**
Documentación interactiva: **http://localhost:5216/scalar/v1**

---

## Autenticación

El sistema usa credenciales hardcodeadas sin JWT. El endpoint `/api/auth/login` devuelve un objeto de sesión con token base64.

| Usuario | Contraseña | Rol |
|---|---|---|
| `admin` | `admin123` | Admin |
| `cajero` | `cajero123` | Cajero |
| `mesero` | `mesero123` | Mesero |

---

## Datos de prueba (seed)

Al correr por primera vez se insertan automáticamente:

**Categorías** — Bebidas, Comidas Rápidas, Postres

**Productos**
| Nombre | Precio | Stock |
|---|---|---|
| Agua Mineral 500ml | $2.50 | 100 |
| Coca-Cola 350ml | $3.50 | 50 |
| Hamburguesa Clásica | $12.90 | 20 |
| Pizza Margarita | $15.50 | 15 |
| Brownie de Chocolate | $6.00 | 30 |

**Mesas** — 3 mesas disponibles (estado: Libre)

---

## Conexión con el frontend

El frontend Angular en `D:\.NET\HungryUpFrontend` se conecta al backend via proxy en desarrollo.

**`proxy.conf.json`** (en la raíz del proyecto Angular):
```json
{
  "/api": {
    "target": "http://localhost:5216",
    "secure": false,
    "changeOrigin": true
  }
}
```

Para correr ambos en desarrollo:
```bash
# Terminal 1 — backend
dotnet run --project src/HungryUp.Api

# Terminal 2 — frontend
cd D:\.NET\HungryUpFrontend\frontendclient
npm start
```

---

## Migraciones

Para generar una migración en los 3 contextos simultáneamente:

```bash
bash db-migrate.sh NombreDeLaMigracion
```

Las migraciones se aplican automáticamente al arrancar con `dotnet run`.

---

## Flujo de ejemplo — FastFood

```bash
# 1. Login
POST /api/auth/login
{ "username": "cajero", "password": "cajero123" }

# 2. Ver productos disponibles
GET /api/v1/products

# 3. Crear orden
POST /api/v1/orders
{ "tipoRestaurante": "FastFood", "mesaId": null,
  "items": [{ "productoId": "...", "cantidad": 2 }] }

# 4. Cocina actualiza estado
PUT /api/v1/orders/{id}/status
{ "nuevoEstado": "EnPreparacion" }

# 5. Cliente paga
POST /api/v1/billing/pay
{ "pedidoId": "...", "metodo": "Efectivo", "montoPagado": 8.50 }

# 6. Ver resumen del día
GET /api/v1/analytics/sales-summary?periodo=dia
```

---

## Licencia

MIT — libre de usar, modificar y distribuir.
