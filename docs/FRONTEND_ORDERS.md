# Pedidos y Monitor — Guía para el Frontend

Endpoints de **pedidos** y soporte para la pantalla **Monitor** (incluida la lista
"Entregados del día"). Todos requieren `Authorization: Bearer <accessToken>`.

---

## 1. Endpoints

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET`  | `/api/v1/orders` | Lista pedidos. Filtro opcional `?estadoPrep=Pendiente\|EnPreparacion\|Entregado` |
| `GET`  | `/api/v1/orders/delivered-today` | **Entregados de HOY** (para el panel "Entregados" del monitor) |
| `GET`  | `/api/v1/orders/mesas` | Mesas activas (para asignar una al crear pedido) |
| `POST` | `/api/v1/orders` | Crear pedido |
| `PUT`  | `/api/v1/orders/{id}/status` | Cambiar estado de preparación |

---

## 2. Shapes

### `PedidoDto` (respuesta)
```json
{
  "id": "guid",
  "mesaId": "guid | null",
  "fechaCreacion": "2026-06-21T03:40:00Z",
  "estadoPrep": "Pendiente",            // Pendiente | EnPreparacion | Entregado
  "estadoFin": "PorPagar",              // PorPagar | Pagado
  "tipo": "FastFood",                   // FastFood | Gourmet
  "numeroTurno": 12,
  "detalles": [
    { "productoId": "guid", "cantidad": 2, "precioUnitario": 18000 }
  ]
}
```

### Crear pedido — `CreatePedidoDto`
`POST /api/v1/orders`
```json
{
  "tipoRestaurante": "Gourmet",         // FastFood | Gourmet
  "mesaId": "guid | null",              // requerido si Gourmet; null si FastFood
  "items": [ { "productoId": "guid", "cantidad": 2 } ]
}
```

### Cambiar estado — `PUT /api/v1/orders/{id}/status`
```json
{ "nuevoEstado": "Entregado" }          // Pendiente | EnPreparacion | Entregado
```

---

## 3. Panel "Entregados" del Monitor

`GET /api/v1/orders/delivered-today` devuelve los pedidos con `estadoPrep = "Entregado"`
**del día de hoy**, ordenados del más reciente al más antiguo. Es justo lo que alimenta la
lista del apartado **Entregados** (la columna a la derecha del monitor).

**Flujo sugerido del monitor:**
- Columnas izquierda/centro: pedidos en `Pendiente` y `EnPreparacion`
  (de `GET /api/v1/orders?estadoPrep=...`).
- Columna derecha **"Entregados"**: `GET /api/v1/orders/delivered-today`.
- Al marcar un pedido como entregado (`PUT …/status` con `Entregado`), refrescar ambas listas
  (o moverlo en el cliente y revalidar). Recomendado **poll cada 10–15 s** o usar el refresco
  que ya tenga el monitor.

**Ejemplo Angular**
```ts
getEntregadosHoy() {
  return this.http.get<Pedido[]>('/api/v1/orders/delivered-today');
}
```

```html
<section class="entregados">
  <h3>Entregados ({{ entregados.length }})</h3>
  <ul>
    <li *ngFor="let p of entregados">
      Turno #{{ p.numeroTurno }} — {{ p.detalles.length }} ítem(s)
      <small>{{ p.fechaCreacion | date:'shortTime' }}</small>
    </li>
  </ul>
</section>
```

> ⚠️ **Caveat de zona horaria.** El backend define "hoy" usando la **fecha UTC**
> (`FechaCreacion` en UTC). Como Colombia es **UTC−5**, el corte del día ocurre a las
> **19:00 hora Colombia** (medianoche UTC): después de esa hora, los pedidos cuentan ya
> como del "día siguiente". Si necesitas que el corte sea a medianoche hora Colombia,
> hay que ajustarlo en el backend (avísame y lo cambio).
>
> Además, el filtro usa `FechaCreacion` (cuándo se **creó** el pedido), no la hora exacta de
> entrega — no se guarda un timestamp de entrega. Para restaurantes con servicio del mismo día
> es equivalente; si se requiere "entregado hoy" exacto, habría que añadir un campo `FechaEntrega`.
