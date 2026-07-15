# Catálogo y Administración (CRUD) — Guía para el Frontend

Cambios en **productos, categorías, mesas y usuarios**. Complementa a
[`FRONTEND_AUTH.md`](./FRONTEND_AUTH.md). **Todos** estos endpoints requieren
`Authorization: Bearer <accessToken>`.

---

## 0. Conceptos transversales

### Borrado lógico (soft delete)
`DELETE` **nunca elimina** físicamente: marca el registro como `activo = false`.
- Los **listados** (`GET` de productos, categorías y mesas) devuelven **TODOS** los registros
  (activos e inactivos) por defecto. Cada item trae su flag `activo` para que el front decida
  cómo mostrarlo (p. ej. atenuado o con badge "Inactivo").
- Filtro opcional **`?activos=true`** para traer solo los activos (útil en vistas operativas).
- El `GET /{id}` devuelve el registro aunque esté inactivo (útil para edición/restauración).
- **Restaurar:** hacer `PUT` con `activo: true` (productos vía update no exponen `activo`;
  categorías y mesas sí — ver shapes).
- **Excepción operativa:** `GET /api/v1/orders/mesas` (asignar mesa a un pedido) devuelve
  **solo activas**, no se puede asignar un pedido a una mesa eliminada.

### Precio en COP
`precio` es **entero** (pesos colombianos, sin decimales). Ver detalle en la versión anterior:
formatear con `Intl.NumberFormat('es-CO', { style:'currency', currency:'COP', maximumFractionDigits:0 })`.

---

## 1. Productos — `/api/v1/products`

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET`    | `/api/v1/products` | Lista **todos** (`?activos=true` filtra solo activos) |
| `GET`    | `/api/v1/products/{id}` | Uno por id (activo o no) |
| `POST`   | `/api/v1/products` | Crear |
| `PUT`    | `/api/v1/products/{id}` | Actualizar |
| `DELETE` | `/api/v1/products/{id}` | Borrado lógico (204) |
| `POST`   | `/api/v1/products/{id}/image` | **Subir imagen** (multipart) |
| `POST`   | `/api/v1/products/{id}/stock` | **Entrada de inventario** `{ cantidad }` |

**`ProductoDto` (respuesta)** — ⚠️ incluye **`tarifaIva`** y **`costoPromedio`** (nuevos):
```json
{
  "id": "guid",
  "nombre": "Limonada",
  "precio": 6000,
  "stockActual": 40,
  "categoriaId": "guid",
  "tarifaIva": 19,          // % de IVA (0 | 5 | 19)
  "costoPromedio": 4200,    // solo lectura, lo calcula el backend al confirmar compras
  "imagenUrl": "/images/products/ab12...png",
  "activo": true
}
```

**Crear** (`CreateProductoDto`): `{ nombre, precio, stockInicial, categoriaId, tarifaIva?, imagenUrl? }` (`tarifaIva` default 19)
**Actualizar** (`UpdateProductoDto`): `{ nombre, precio, stockActual, categoriaId, tarifaIva, imagenUrl }`

> El costeo de inventario, la entrada de stock y el descuento automático al vender se detallan en
> [`FRONTEND_PURCHASES.md`](./FRONTEND_PURCHASES.md). El módulo de **compras** aumenta el inventario
> y recalcula `costoPromedio`.

### Imágenes con ruta interna
La imagen ya **no es solo una URL externa**: se sube el archivo y el backend lo guarda
internamente, devolviendo una **ruta interna** (`/images/products/{archivo}`) en `imagenUrl`.

- **Endpoint:** `POST /api/v1/products/{id}/image`
- **Body:** `multipart/form-data` con el campo **`file`**.
- **Restricciones:** extensiones `.jpg .jpeg .png .webp .gif`, máx. **5 MB**.
- **Respuesta:** el `ProductoDto` actualizado (con el nuevo `imagenUrl`).
- **Mostrar la imagen:** anteponer la base de la API a la ruta:
  `src = apiBaseUrl + producto.imagenUrl` (ej. `http://localhost:5216/images/products/ab12...png`).
  Las imágenes se sirven **públicamente** (no requieren token).

**Ejemplo Angular**
```ts
subirImagen(productoId: string, file: File) {
  const form = new FormData();
  form.append('file', file);
  return this.http.post<Producto>(`/api/v1/products/${productoId}/image`, form);
  // No fijar Content-Type manualmente: Angular pone el boundary del multipart.
}
```

---

## 2. Categorías — `/api/v1/categories`

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET`    | `/api/v1/categories` | Lista **todas** (`?activos=true` filtra solo activas) |
| `GET`    | `/api/v1/categories/{id}` | Una por id |
| `POST`   | `/api/v1/categories` | Crear |
| `PUT`    | `/api/v1/categories/{id}` | Actualizar |
| `DELETE` | `/api/v1/categories/{id}` | Borrado lógico (204) |

**`CategoriaDto`**: `{ id, nombre, activo }`
**Crear**: `{ nombre }` · **Actualizar**: `{ nombre, activo }` (poner `activo:true` para restaurar)

---

## 3. Mesas — `/api/v1/mesas`

| Método | Ruta | Descripción |
|--------|------|-------------|
| `GET`    | `/api/v1/mesas` | Lista **todas** (`?activos=true` filtra solo activas) |
| `GET`    | `/api/v1/mesas/{id}` | Una por id |
| `POST`   | `/api/v1/mesas` | Crear |
| `PUT`    | `/api/v1/mesas/{id}` | Actualizar |
| `DELETE` | `/api/v1/mesas/{id}` | Borrado lógico (204) |

**`MesaDto`**: `{ id, numero, estado, activo }` — `estado` ∈ `"Libre" | "Ocupada"`.
**Crear**: `{ numero }` (nace `Libre` y activa) · **Actualizar**: `{ numero, estado, activo }`
- No se permiten dos mesas **activas** con el mismo `numero` (→ 400).
- El endpoint antiguo `GET /api/v1/orders/mesas` sigue funcionando (lista activas).

---

## 4. Usuarios — `/api/v1/users` (solo rol `Admin`)

Sin cambios en los existentes; **nuevo**:

| Método | Ruta | Descripción |
|--------|------|-------------|
| `DELETE` | `/api/v1/users/{id}` | Borrado lógico (`activo=false`, 204) |

> El usuario desactivado **no puede iniciar sesión** (login → 401), pero **sigue apareciendo**
> en `GET /api/v1/users` (el admin puede reactivarlo con `PUT … { activo:true }`).

---

## 5. Checklist de migración para el front

- [ ] Tratar `DELETE` como desactivación (no esperar que el registro desaparezca físicamente).
- [ ] Asumir que los listados traen **todos** los registros; usar el flag `activo` de cada item
      (o `?activos=true`) para decidir qué mostrar.
- [ ] Usar el `GET /{id}` para pantallas de edición/restauración.
- [ ] Implementar subida de imagen con `FormData` (campo `file`) y mostrarla con `apiBaseUrl + imagenUrl`.
- [ ] Manejar `precio` como entero COP.
- [ ] CRUD de mesas con control de número duplicado (400).
