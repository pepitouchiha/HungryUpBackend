# 🍔 HungryUp Backend

> REST API para automatización de restaurantes — construida como un **Monolito Modular** en .NET 9.

![.NET](https://img.shields.io/badge/.NET-9.0-512BD4?style=flat-square&logo=dotnet)
![EF Core](https://img.shields.io/badge/EF%20Core-9.0-512BD4?style=flat-square)
![SQLite](https://img.shields.io/badge/SQLite-003B57?style=flat-square&logo=sqlite&logoColor=white)
![Architecture](https://img.shields.io/badge/Architecture-Modular%20Monolith-blue?style=flat-square)
![License](https://img.shields.io/badge/license-MIT-green?style=flat-square)

---

## ¿Qué es HungryUp?

HungryUp es el backend para un sistema de gestión de restaurantes que soporta dos flujos operacionales:

| Flujo | Descripción |
|---|---|
| 🏃 **FastFood** | Orden → Pago inmediato → Cocina. Sin mesa asignada. |
| 🍷 **Gourmet** | Orden → Consumo incremental → Pago y cierre al final. Mesa requerida. |

---

## Arquitectura

El proyecto sigue el patrón de **Monolito Modular**: un único proceso y una única base de datos, pero con módulos internamente aislados que se comunican **exclusivamente a través de interfaces públicas**, nunca via DbContext cruzados.

```
HungryUpBackend/
│
├── Modules/
│   ├── Catalog/          # Gestión de productos y categorías
│   │   ├── Controllers/
│   │   ├── Entities/     ← CatalogDbContext aislado
│   │   └── Services/     ← ICatalogService
│   │
│   ├── Orders/           # Pedidos, detalles y mesas
│   │   ├── Controllers/
│   │   ├── Entities/     ← OrdersDbContext aislado
│   │   └── Services/     ← IOrdersService
│   │
│   ├── Billing/          # Pagos y facturación
│   │   ├── Controllers/
│   │   ├── Entities/     ← BillingDbContext aislado
│   │   └── Services/     ← IBillingService
│   │
│   └── Analytics/        # Reportes de ventas
│       ├── Controllers/
│       └── Services/     ← IAnalyticsService
│
├── Program.cs            # DI, middlewares, auto-migrate + seed
├── DataSeeder.cs         # Datos de prueba al arrancar
├── GlobalExceptionHandler.cs
├── check-architecture.sh # Validador de reglas arquitectónicas
└── db-migrate.sh         # Generador de migraciones por módulo
```

### Comunicación entre módulos

```
AnalyticsService  ──▶  IBillingService
BillingService    ──▶  IOrdersService
OrdersService     ──▶  ICatalogService   (captura precio estático)
```

Ningún módulo toca el `DbContext` de otro. Punto.

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Framework | ASP.NET Core 9 — Minimal Hosting |
| ORM | Entity Framework Core 9 |
| Base de datos | SQLite (un solo archivo `hungryup.db`) |
| API Docs | Scalar UI (`/scalar/v1`) |
| Serialización | `System.Text.Json` + enums como strings |

---

## Endpoints

### 📦 Catalog
```
GET  /api/v1/products                  Productos con stock disponible
POST /api/v1/products                  Crear producto
```

### 🧾 Orders
```
POST /api/v1/orders                    Crear orden (FastFood o Gourmet)
PUT  /api/v1/orders/{id}/status        Cambiar estado de preparación
```

### 💳 Billing
```
POST /api/v1/billing/pay               Procesar pago de una orden
```

### 📊 Analytics
```
GET  /api/v1/analytics/sales-summary?periodo=dia|semana|mes
```

---

## Inicio rápido

### Requisitos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [dotnet-ef CLI](https://learn.microsoft.com/ef/core/cli/dotnet) — `dotnet tool install -g dotnet-ef`

### Instalación y ejecución

```bash
git clone https://github.com/pepitouchiha/HungryUpBackend.git
cd HungryUpBackend
dotnet run
```

Al arrancar, el sistema automáticamente:
1. Aplica las migraciones pendientes en `hungryup.db`
2. Carga datos de prueba (categorías, productos y mesas)

Abre la UI interactiva en: **http://localhost:5216/scalar/v1**

---

## Datos de prueba (seed)

Al correr por primera vez se insertan automáticamente:

**Categorías**
- Bebidas · Comidas Rápidas · Postres

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

## Scripts de utilidad

### `check-architecture.sh`
Valida las 4 reglas de arquitectura definidas en la spec:

```bash
bash check-architecture.sh
```

```
── Regla: sin float ni double en Modules/ ───────────────
  ✓ Ningún uso de float o double detectado.

── Regla: decimal(18,2) — HasPrecision en cada DbContext ─
  ✓ CatalogDbContext  → HasPrecision encontrado
  ✓ OrdersDbContext   → HasPrecision encontrado
  ✓ BillingDbContext  → HasPrecision encontrado

── Regla: aislamiento de DbContexts entre módulos ────────
  ✓ Modules/Catalog/  no referencia otros DbContexts
  ...

Resultado: 11 reglas OK  |  0 violaciones
```

### `db-migrate.sh`
Genera una migración en los 3 módulos con persistencia simultáneamente:

```bash
bash db-migrate.sh NombreDeLaMigracion
```

Las migraciones se aplican automáticamente en el próximo `dotnet run`.

---

## Reglas de arquitectura (estrictas)

Definidas en [`backend-spec.md`](./backend-spec.md):

- ❌ **Prohibido** usar `float` o `double` para precios — solo `decimal`
- ❌ **Prohibido** acceder al `DbContext` de otro módulo directamente
- ✅ Todos los precios se mapean como `decimal(18,2)` (`HasPrecision`)
- ✅ La comunicación inter-módulo es siempre a través de `IXxxService`
- ✅ El precio unitario en `DetallePedido` es una **captura estática** al momento de la orden

---

## Flujo de ejemplo — FastFood

```bash
# 1. Ver productos disponibles
GET /api/v1/products

# 2. Crear orden
POST /api/v1/orders
{ "tipoRestaurante": "FastFood", "mesaId": null,
  "items": [{ "productoId": "...", "cantidad": 2 }] }

# 3. Cocina actualiza estado
PUT /api/v1/orders/{id}/status
{ "nuevoEstado": "EnPreparacion" }

# 4. Cliente paga
POST /api/v1/billing/pay
{ "pedidoId": "...", "metodoPago": "Efectivo", "montoPagado": 8.50 }

# 5. Ver resumen del día
GET /api/v1/analytics/sales-summary?periodo=dia
```

---

## Licencia

MIT — libre de usar, modificar y distribuir.
