# Cambios en Catálogo (Productos) — Guía para el Frontend

Cambios en el módulo de **productos** que el frontend debe tener en cuenta. Complementa a
[`FRONTEND_AUTH.md`](./FRONTEND_AUTH.md) (recuerda: estos endpoints ahora requieren
`Authorization: Bearer <accessToken>`).

---

## 1. Precio en pesos colombianos (COP) — sin decimales

El campo `precio` de los productos representa **pesos colombianos**, que **no usan centavos**.
En la base de datos se almacena con precisión entera (`HasPrecision(18, 0)`).

**Implicaciones para el frontend:**

- **Enviar** el precio como **número entero** (ej. `18000`, no `18000.50`). Los decimales se truncan.
- **Mostrar** formateado como COP, sin decimales:
  ```ts
  new Intl.NumberFormat('es-CO', {
    style: 'currency', currency: 'COP', maximumFractionDigits: 0
  }).format(18000);   // => "$ 18.000"
  ```
  O con el `CurrencyPipe` de Angular:
  ```html
  {{ producto.precio | currency:'COP':'symbol':'1.0-0':'es-CO' }}  <!-- $18.000 -->
  ```
- Validar en formularios que el precio sea un **entero ≥ 0** (sin separador decimal).

---

## 2. Nuevo campo `imagenUrl` (opcional)

Los productos tienen un campo **`imagenUrl`** opcional (string, máx. 2048 caracteres).
Puede venir **`null`** si el producto no tiene imagen.

**Implicaciones para el frontend:**

- Al **listar/mostrar** productos, manejar el caso `null` con una **imagen placeholder**.
  ```html
  <img [src]="producto.imagenUrl || 'assets/img/producto-placeholder.png'" alt="{{ producto.nombre }}">
  ```
- Al **crear** un producto, el campo es **opcional**: se puede omitir o enviar `null`.

---

## 3. Shapes actualizados

### Producto (respuesta) — `ProductoDto`
```json
{
  "id": "a1b2c3d4-...",          // Guid
  "nombre": "Hamburguesa Clásica",
  "precio": 18000,               // entero COP, sin decimales
  "stockActual": 50,
  "categoriaId": "10000000-0000-0000-0000-000000000002",
  "imagenUrl": "https://cdn.hungryup.co/hamburguesa.jpg"   // o null
}
```

### Crear producto (request) — `CreateProductoDto`
`POST /api/v1/products`
```json
{
  "nombre": "Hamburguesa Clásica",
  "precio": 18000,
  "stockInicial": 50,
  "categoriaId": "10000000-0000-0000-0000-000000000002",
  "imagenUrl": "https://cdn.hungryup.co/hamburguesa.jpg"   // opcional, puede omitirse
}
```

> `imagenUrl` es opcional; si se omite, el producto se crea sin imagen (`null`).

---

## 4. Endpoints de productos (recordatorio)

| Método | Ruta | Notas |
|--------|------|-------|
| `GET`  | `/api/v1/products` | Lista productos con stock > 0. **Requiere Bearer token.** |
| `POST` | `/api/v1/products` | Crea producto. **Requiere Bearer token.** |

---

## 5. Checklist de migración para el front

- [ ] Tratar `precio` como **entero** (envío y validación), sin decimales.
- [ ] Formatear el precio como **COP** (`$18.000`).
- [ ] Soportar `imagenUrl` **nullable** con imagen placeholder.
- [ ] Permitir `imagenUrl` opcional en el formulario de creación.
- [ ] Adjuntar `Authorization: Bearer <accessToken>` a las llamadas de productos.
