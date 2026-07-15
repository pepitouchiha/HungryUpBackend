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

Cada dominio tiene su propio `DbContext` aislado (6 en total: Catalog, Orders, Billing, Auth, Purchasing, Payroll) con tabla de migraciones independiente (`__EFMigrationsHistory_<Módulo>`).

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core 9 — Minimal Hosting |
| ORM | Entity Framework Core 9 |
| Base de datos | SQLite — 6 DbContexts aislados |
| API Docs | Scalar UI (`/scalar/v1`) |
| Serialización | `System.Text.Json` + enums como strings |
| Auth | **JWT (HMAC-SHA256)** access token 15 min + refresh token 7 días con rotación estricta; BCrypt |
| Autorización | **RBAC por permisos granulares** (`recurso:acción`) |

---

## Endpoints

> Todos los endpoints (salvo los de `/api/auth`) exigen `Authorization: Bearer <accessToken>` y
> el **permiso** correspondiente. Detalle para el frontend en [`docs/`](./docs).

### Auth
```
POST /api/auth/login                   Iniciar sesión → accessToken + refreshToken
POST /api/auth/refresh                 Rotar tokens
POST /api/auth/logout                  Revocar refresh token
GET  /api/auth/me                      Usuario actual + rol + permisos efectivos
```

### Catalog
```
GET    /api/v1/products                Listar productos (?activos=true)
POST   /api/v1/products                Crear producto (con tarifaIva)
GET    /api/v1/products/{id}           Obtener producto por ID
PUT    /api/v1/products/{id}           Actualizar producto
DELETE /api/v1/products/{id}           Borrado lógico
POST   /api/v1/products/{id}/image     Subir imagen (multipart)
POST   /api/v1/products/{id}/stock     Entrada de inventario { cantidad }
GET    /api/v1/categories              CRUD de categorías (+ POST/PUT/DELETE)
```

### Orders
```
GET  /api/v1/orders                    Listar pedidos (?estadoPrep=Pendiente|EnPreparacion|Entregado)
GET  /api/v1/orders/{id}               Obtener pedido por ID
GET  /api/v1/orders/delivered-today    Entregados de hoy
POST /api/v1/orders                    Crear orden (descuenta stock; 400 si insuficiente)
PUT  /api/v1/orders/{id}/status        Cambiar estado de preparación
GET  /api/v1/orders/mesas              Mesas activas
GET  /api/v1/mesas                     CRUD de mesas (+ POST/PUT/DELETE)
```

### Billing
```
POST /api/v1/billing/pay               Procesar pago de una orden (libera mesa si Gourmet)
```

### Purchasing (Compras)
```
GET    /api/v1/compras                 Listar (?estado=Borrador|Confirmada|Anulada)
POST   /api/v1/compras                 Crear factura de compra (Borrador)
PUT    /api/v1/compras/{id}            Editar (solo Borrador)
PATCH  /api/v1/compras/{id}/notas      Editar notas
POST   /api/v1/compras/{id}/confirmar  Confirmar → aumenta inventario y costo promedio
POST   /api/v1/compras/{id}/anular     Anular → revierte inventario
DELETE /api/v1/compras/{id}            Eliminar (solo Borrador)
```

### Payroll (Empleados)  ·  solo Admin
```
GET/POST/PUT/DELETE /api/v1/empleados  CRUD de empleados con salario mensual
```

### Analytics / Dashboard
```
GET /api/v1/analytics/dashboard                 Tarjetas resumen (rango de fechas)
GET /api/v1/analytics/sales-timeseries          Serie temporal (dia|semana|mes, ?productoId)
GET /api/v1/analytics/top-products              Más vendidos (?orderBy=cantidad|ingresos)
GET /api/v1/analytics/sales-by-payment-method   Ventas por método de pago
GET /api/v1/analytics/sales-by-type             FastFood vs Gourmet
GET /api/v1/analytics/inventory                 Valor de inventario y bajo stock
GET /api/v1/analytics/profit-loss               Ganancias/pérdidas (utilidad caja + operativa) · solo Admin
GET /api/v1/analytics/sales-summary?periodo=    Resumen rápido por periodo
```

### Users  ·  solo Admin
```
GET/POST/PUT/DELETE /api/v1/users      Gestión de usuarios (+ PUT {id}/password)
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

El sistema usa **JWT** (HMAC-SHA256) con contraseñas hasheadas con **BCrypt**. `POST /api/auth/login`
devuelve un **access token** (15 min) y un **refresh token** (7 días, con rotación estricta y detección
de reuso). La autorización es **RBAC por permisos granulares** (`recurso:acción`): cada endpoint exige
un permiso concreto vía `[HasPermission(...)]`, y el mapeo rol→permisos está centralizado en `RolePermissions`.
Consulta `GET /api/auth/me` para obtener el rol y los permisos efectivos del usuario.

| Usuario | Contraseña | Rol | Alcance |
|---|---|---|---|
| `admin` | `admin123` | Admin | Todo (incluye compras, empleados y ganancias/pérdidas) |
| `cajero` | `cajero123` | Cashier | Pedidos, cobro, dashboard general, ver compras |
| `mesero` | `mesero123` | Waiter | Pedidos y estado de mesas |

> Documentación de integración para el frontend en [`docs/`](./docs):
> auth, permisos, catálogo, pedidos, compras y dashboard.

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
