# Dashboard, Empleados y Ganancias/Pérdidas — Guía para el Frontend

Endpoints de **análisis flexibles** para el dashboard, el **módulo de empleados** (nómina) y el
reporte de **ganancias/pérdidas**. Requieren `Authorization: Bearer <accessToken>` y los permisos indicados.
Ver [`FRONTEND_PERMISSIONS.md`](./FRONTEND_PERMISSIONS.md).

---

## 0. Rango de fechas (común a analytics)

Casi todos los endpoints aceptan `?desde=&hasta=` (ISO 8601).

- Si se omiten → por defecto **el mes actual** (día 1 → ahora).
- `hasta` **sin hora** (solo fecha) incluye **todo el día** (ej. `hasta=2026-07-15` = hasta 23:59:59).
- Las fechas se manejan en **UTC**.

Ejemplo: `GET /api/v1/analytics/dashboard?desde=2026-07-01&hasta=2026-07-31`

---

## 1. Analytics — `/api/v1/analytics` (permiso `analytics:read`, salvo P&L)

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| `GET` | `/analytics/dashboard?desde&hasta&umbralStockBajo=5` | `analytics:read` | Tarjetas resumen |
| `GET` | `/analytics/sales-timeseries?desde&hasta&granularidad=dia&productoId=` | `analytics:read` | Serie temporal (gráficas) |
| `GET` | `/analytics/top-products?desde&hasta&top=10&orderBy=cantidad` | `analytics:read` | Productos más vendidos |
| `GET` | `/analytics/sales-by-payment-method?desde&hasta` | `analytics:read` | Ventas por método de pago |
| `GET` | `/analytics/sales-by-type?desde&hasta` | `analytics:read` | FastFood vs Gourmet |
| `GET` | `/analytics/inventory?umbralStockBajo=5` | `analytics:read` | Estado del inventario |
| `GET` | `/analytics/profit-loss?desde&hasta` | **`analytics:profit-loss`** | Ganancias/pérdidas |
| `GET` | `/analytics/sales-summary?periodo=dia` | `analytics:read` | Resumen rápido (compat) |

### `dashboard`
```json
{
  "desde": "2026-07-01T00:00:00", "hasta": "2026-07-31T23:59:59",
  "ingresos": 1200000, "ordenes": 1, "ticketPromedio": 1200000,
  "unidadesVendidas": 60, "totalComprado": 840000,
  "valorInventario": 320000, "productosBajoStock": 2
}
```

### `sales-timeseries` → array (para gráfica de líneas/barras)
`granularidad = dia | semana | mes`. `productoId` opcional para filtrar un producto.
```json
[ { "periodo": "2026-07-15", "ingresos": 1200000, "ordenes": 1, "unidades": 60 } ]
```

### `top-products` → array
`orderBy = cantidad | ingresos`, `top = N`.
```json
[ { "productoId": "guid", "nombre": "Bandeja Paisa", "cantidad": 60, "ingresos": 1200000 } ]
```

### `sales-by-payment-method` → array
```json
[ { "metodo": "Efectivo", "ingresos": 1200000, "pagos": 1 } ]
```

### `sales-by-type` → array
```json
[ { "tipo": "FastFood", "ingresos": 1200000, "ordenes": 1, "unidades": 60 } ]
```

### `inventory`
`valorTotal = Σ stock × costoPromedio`. `bajoStock` = productos por debajo del umbral.
```json
{
  "valorTotal": 320000, "productos": 10, "umbralStockBajo": 5,
  "bajoStock": [
    { "productoId": "guid", "nombre": "Pizza", "stockActual": 3, "costoPromedio": 9000, "valorStock": 27000 }
  ]
}
```

---

## 2. Ganancias/Pérdidas — `GET /analytics/profit-loss`  *(solo `analytics:profit-loss` → Admin)*

Devuelve **dos lecturas de utilidad**:

```json
{
  "desde": "2026-07-01T00:00:00", "hasta": "2026-07-31T23:59:59",
  "ingresos": 1200000,
  "comprasDelPeriodo": 840000,   // compras confirmadas en el rango (caja)
  "cogs": 480000,                // costo de lo realmente vendido
  "salarios": 750000,            // nómina prorrateada del periodo
  "empleados": 1, "diasNomina": 15,
  "utilidadCaja": -390000,       // Ingresos − ComprasDelPeriodo − Salarios
  "utilidadOperativa": -30000    // Ingresos − COGS − Salarios
}
```

- **Utilidad de caja** = cuánto dinero neto entró/salió (útil para flujo de caja). Comprar inventario por adelantado la baja.
- **Utilidad operativa** = ganancia real de vender (empareja cada venta con su costo). Es la cifra "contable".
- La diferencia entre ambas ≈ inventario comprado pero **aún no vendido** (+ IVA de compras).
- **Sensible:** solo Admin. Ocultar la vista si el usuario no tiene `analytics:profit-loss`.

> **Nota sobre datos históricos:** los pedidos creados **antes** de activar el costeo tienen
> `costoUnitario = 0`, así que su COGS es 0. Los pedidos nuevos capturan el costo correctamente.

---

## 3. Empleados / Nómina — `/api/v1/empleados`  *(solo `employees:*` → Admin)*

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| `GET`    | `/api/v1/empleados?activos=true` | `employees:read` | Listar (todos, o solo activos) |
| `GET`    | `/api/v1/empleados/{id}` | `employees:read` | Uno por id |
| `POST`   | `/api/v1/empleados` | `employees:create` | Crear |
| `PUT`    | `/api/v1/empleados/{id}` | `employees:update` | Actualizar |
| `DELETE` | `/api/v1/empleados/{id}` | `employees:delete` | Borrado lógico (204) |

**`EmpleadoDto`**
```json
{
  "id": "guid", "nombre": "Juan Pérez", "documento": "123",
  "cargo": "Cocinero", "salarioMensual": 1500000,
  "fechaIngreso": "2026-07-01T00:00:00Z", "activo": true
}
```
- **Crear**: `{ nombre, documento?, cargo, salarioMensual, fechaIngreso? }` (fecha por defecto = ahora).
- **Actualizar**: `{ nombre, documento?, cargo, salarioMensual, fechaIngreso, activo }` (`activo:true` para reactivar).
- `DELETE` es **borrado lógico** (`activo=false`); el empleado inactivo **no cuenta** en la nómina del P&L.
- **Cálculo de nómina** (lo hace el P&L): por empleado activo, `salarioMensual ÷ 30 × días del rango`.

---

## 4. Checklist de migración

- [ ] Construir el dashboard consumiendo `/analytics/dashboard` + gráficas con `sales-timeseries`.
- [ ] Selector de rango de fechas; recordar que `hasta` (solo fecha) incluye el día completo.
- [ ] Vistas de `top-products`, `sales-by-payment-method`, `sales-by-type`, `inventory`.
- [ ] CRUD de empleados (solo Admin) con soft-delete.
- [ ] Vista de **ganancias/pérdidas** mostrando **ambas utilidades** (caja y operativa) y explicando la diferencia.
- [ ] Ocultar P&L y empleados si el usuario no tiene los permisos (`analytics:profit-loss`, `employees:read`).
- [ ] Formatear montos como COP entero (`Intl.NumberFormat('es-CO', { style:'currency', currency:'COP', maximumFractionDigits:0 })`).
