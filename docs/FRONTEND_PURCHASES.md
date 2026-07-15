# Compras e Inventario — Guía para el Frontend

Nuevo **módulo de compras** (facturas de compra que aumentan el inventario) y cambios en
**productos/stock**. Requiere `Authorization: Bearer <accessToken>` y los permisos indicados.
Complementa a [`FRONTEND_CATALOG.md`](./FRONTEND_CATALOG.md) y [`FRONTEND_PERMISSIONS.md`](./FRONTEND_PERMISSIONS.md).

---

## 0. ⚠️ Cambios en el Producto (breaking)

`ProductoDto` ahora incluye **dos campos nuevos**:

```json
{
  "id": "guid", "nombre": "Bandeja Paisa", "precio": 20000, "stockActual": 40,
  "categoriaId": "guid",
  "tarifaIva": 5,          // NUEVO — % de IVA del producto (0 | 5 | 19)
  "costoPromedio": 8000,   // NUEVO — costo promedio ponderado (lo calcula el backend)
  "imagenUrl": null, "activo": true
}
```

- **Crear** (`CreateProductoDto`): `{ nombre, precio, stockInicial, categoriaId, tarifaIva?, imagenUrl? }`
  — `tarifaIva` por defecto **19** si no se envía. Agregar el campo al formulario de producto.
- **Actualizar** (`UpdateProductoDto`): `{ nombre, precio, stockActual, categoriaId, tarifaIva, imagenUrl }`.
- `costoPromedio` es **solo lectura**: se recalcula al **confirmar compras**. No se envía al crear/editar.

### Stock
| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| `POST` | `/api/v1/products/{id}/stock` | `products:stock` | **Entrada manual** de inventario |

Body: `{ "cantidad": 50 }` (entero > 0) → devuelve el `ProductoDto` con el stock sumado.
**No cambia el costo promedio** (para eso se usan las compras). Ideal para ajustes rápidos.

> **Al crear un pedido** el backend **descuenta stock** y valida disponibilidad: si no hay
> suficiente responde **400** (`"Stock insuficiente para '...': disponible X, solicitado Y"`).
> El detalle del pedido ahora también trae `costoUnitario` (foto del costo al vender).

---

## 1. Compras — `/api/v1/compras`

| Método | Ruta | Permiso | Descripción |
|--------|------|---------|-------------|
| `GET`    | `/api/v1/compras?estado=` | `purchasing:read` | Listar (`?estado=Borrador\|Confirmada\|Anulada`) |
| `GET`    | `/api/v1/compras/{id}` | `purchasing:read` | Una por id (con totales calculados) |
| `POST`   | `/api/v1/compras` | `purchasing:create` | Crear (nace **Borrador**) |
| `PUT`    | `/api/v1/compras/{id}` | `purchasing:update` | Editar (**solo Borrador**) |
| `PATCH`  | `/api/v1/compras/{id}/notas` | `purchasing:update` | Editar notas (cualquier estado salvo Anulada) |
| `POST`   | `/api/v1/compras/{id}/confirmar` | `purchasing:confirm` | **Suma inventario** y recalcula costo |
| `POST`   | `/api/v1/compras/{id}/anular` | `purchasing:anular` | Revierte inventario |
| `DELETE` | `/api/v1/compras/{id}` | `purchasing:delete` | Eliminar (**solo Borrador**) |

### Flujo de estados
```
Borrador  ──confirmar──►  Confirmada  ──anular──►  Anulada
   │  (editable, NO toca stock)   (stock +, bloqueada)   (stock revertido)
   └── DELETE (solo aquí)
```

- **Borrador:** editable con `PUT`. No afecta el inventario todavía.
- **Confirmar:** aumenta el stock de cada producto y **recalcula su `costoPromedio`** (promedio ponderado). Queda bloqueada.
- **Anular:** revierte el stock que había sumado. Falla (**400**) si ese stock ya se vendió.
- **Notas** editables siempre (salvo Anulada) vía `PATCH …/notas` con `{ "notas": "..." }`.

### `CompraDto` (respuesta) — los totales los calcula el backend
```json
{
  "id": "guid",
  "numeroFactura": "FC-001",
  "nombreProveedor": "Distribuidora XYZ",
  "nitProveedor": "900123456-7",
  "fecha": "2026-07-15T00:00:00Z",
  "notas": "Compra mensual",
  "estado": "Confirmada",
  "reteFuentePorc": 2.5, "reteIvaPorc": 0, "reteIcaPorMil": 0,
  "fechaCreacion": "2026-07-15T03:31:00Z",
  "fechaConfirmacion": "2026-07-15T03:35:00Z",
  "lineas": [
    { "id": "guid", "productoId": "guid", "productoNombre": "Bandeja Paisa",
      "cantidad": 100, "costoUnitario": 8000, "tarifaIva": 5,
      "subtotal": 800000, "ivaValor": 40000, "total": 840000 }
  ],
  "subtotal": 800000,
  "ivaTotal": 40000,
  "reteFuenteValor": 20000, "reteIvaValor": 0, "reteIcaValor": 0,
  "totalRetenciones": 20000,
  "totalBruto": 840000,
  "totalAPagar": 820000
}
```

### Crear/editar (`CreateCompraDto` / `UpdateCompraDto`)
```json
{
  "numeroFactura": "FC-001",
  "nombreProveedor": "Distribuidora XYZ",
  "nitProveedor": "900123456-7",     // opcional
  "fecha": null,                      // opcional (por defecto ahora)
  "notas": "Compra mensual",          // opcional
  "reteFuentePorc": 2.5,              // % sobre el subtotal
  "reteIvaPorc": 0,                   // % sobre el IVA total
  "reteIcaPorMil": 0,                 // por mil sobre el subtotal
  "items": [
    { "productoId": "guid", "cantidad": 100, "costoUnitario": 8000, "tarifaIva": null }
  ]
}
```
- **IVA por línea:** si `tarifaIva` va `null`, se toma la del producto. Se puede sobrescribir por línea (ej. `0`).
- **Retenciones a nivel factura** (Colombia): ReteFuente y ReteIVA en **%**, ReteICA en **por mil**.
- Al editar (`PUT`) se **reemplazan todas las líneas** por las enviadas.
- `totalAPagar = subtotal + IVA − retenciones`.

---

## 2. Checklist de migración

- [ ] Añadir **`tarifaIva`** al formulario de producto (default 19) y mostrar **`costoPromedio`** como solo lectura.
- [ ] Usar `POST /products/{id}/stock` para entradas rápidas de inventario.
- [ ] Manejar **400 por stock insuficiente** al crear pedidos.
- [ ] Pantalla de compras con el flujo Borrador → Confirmar → (Anular), respetando permisos.
- [ ] Mostrar los totales del `CompraDto` (subtotal, IVA, retenciones, total a pagar) sin recalcular en el front.
- [ ] Editar compras solo si `estado === "Borrador"`; notas editables salvo Anulada.
