# Roles y Permisos (RBAC) — Guía para el Frontend

El backend pasó de proteger endpoints por **rol** a hacerlo por **permisos granulares**
(`recurso:acción`, p. ej. `products:create`). Cada rol tiene un conjunto de permisos; el frontend
debe **leer los permisos del usuario y mostrar/ocultar la UI** en consecuencia.

Complementa a [`FRONTEND_AUTH.md`](./FRONTEND_AUTH.md).

---

## 1. `GET /api/auth/me` — quién soy y qué puedo hacer  *(requiere token)*

Llamar **justo después del login** (y del refresh si recargas la app) para conocer el rol y los permisos.

**Response 200**
```json
{
  "id": "1",
  "username": "admin",
  "email": "admin@hungryup.com",
  "rol": "Admin",
  "permisos": ["analytics:read", "billing:pay", "categories:create", "..."]
}
```

> El **access token (JWT) NO contiene la lista de permisos**, solo el rol. Por eso se consulta `/me`.
> Si un admin cambia lo que puede un rol, basta con que el usuario vuelva a pedir `/me` (no requiere re-login).

---

## 2. Catálogo de permisos por rol

| Permiso | Admin | Cashier (Cajero) | Waiter (Mesero) |
|---|:---:|:---:|:---:|
| `products:read` | ✅ | ✅ | ✅ |
| `products:create` `:update` `:delete` `:stock` `:image` | ✅ | — | — |
| `categories:read` | ✅ | ✅ | ✅ |
| `categories:create` `:update` `:delete` | ✅ | — | — |
| `orders:read` `orders:create` `orders:update-status` | ✅ | ✅ | ✅ |
| `mesas:read` `mesas:update` | ✅ | ✅ | ✅ |
| `mesas:create` `mesas:delete` | ✅ | — | — |
| `billing:pay` | ✅ | ✅ | — |
| `analytics:read` (dashboard general) | ✅ | ✅ | — |
| `analytics:profit-loss` (ganancias/pérdidas) | ✅ | — | — |
| `purchasing:read` | ✅ | ✅ | — |
| `purchasing:create` `:update` `:confirm` `:anular` `:delete` | ✅ | — | — |
| `users:*` (gestión de usuarios) | ✅ | — | — |
| `employees:*` (empleados/nómina) | ✅ | — | — |

> **Datos financieros sensibles** (`analytics:profit-loss`, `employees:*`) son **solo Admin**.
> El Cajero ve el dashboard general pero **no** el reporte de ganancias ni los salarios.

---

## 3. Cómo reaccionar en el frontend

- **Ocultar/deshabilitar** botones y menús según `permisos` (no muestres "Crear producto" a quien no tiene `products:create`).
- El backend es la fuente de verdad: aunque ocultes un botón, si se llama un endpoint sin permiso responde **403**.
- **403 ≠ 401.** El `401` dispara refresh (ver auth). El **403** significa "autenticado pero sin permiso":
  mostrar mensaje "No autorizado" y **no** reintentar ni cerrar sesión.

---

## 4. Ejemplo Angular (servicio + guard + directiva)

```ts
// permissions.service.ts
import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';

interface Me { id: string; username: string; email: string; rol: string; permisos: string[]; }

@Injectable({ providedIn: 'root' })
export class PermissionsService {
  private readonly perms = signal<Set<string>>(new Set());
  readonly rol = signal<string | null>(null);

  constructor(private http: HttpClient) {}

  /** Llamar tras login y al iniciar la app si hay sesión. */
  load() {
    return this.http.get<Me>('/api/auth/me').pipe(tap(me => {
      this.perms.set(new Set(me.permisos));
      this.rol.set(me.rol);
    }));
  }

  can(permiso: string): boolean { return this.perms().has(permiso); }
  clear() { this.perms.set(new Set()); this.rol.set(null); }
}
```

```ts
// has-permission.directive.ts  → <button *hasPermission="'products:create'">Crear</button>
import { Directive, Input, TemplateRef, ViewContainerRef, inject } from '@angular/core';
import { PermissionsService } from './permissions.service';

@Directive({ selector: '[hasPermission]', standalone: true })
export class HasPermissionDirective {
  private tpl = inject(TemplateRef<unknown>);
  private vcr = inject(ViewContainerRef);
  private perms = inject(PermissionsService);

  @Input() set hasPermission(permiso: string) {
    this.vcr.clear();
    if (this.perms.can(permiso)) this.vcr.createEmbeddedView(this.tpl);
  }
}
```

```ts
// permission.guard.ts  → proteger rutas (data: { permiso: 'analytics:profit-loss' })
import { CanActivateFn, Router } from '@angular/router';
import { inject } from '@angular/core';
import { PermissionsService } from './permissions.service';

export const permissionGuard: CanActivateFn = (route) => {
  const perms = inject(PermissionsService);
  const router = inject(Router);
  const requerido = route.data['permiso'] as string;
  return !requerido || perms.can(requerido) ? true : router.createUrlTree(['/no-autorizado']);
};
```

---

## 5. Checklist de migración

- [ ] Llamar `GET /api/auth/me` tras login (y al recargar con sesión activa) y guardar `permisos`.
- [ ] Ocultar/deshabilitar UI con una directiva/guard basada en permisos.
- [ ] Distinguir **403** (sin permiso → "no autorizado", no reintentar) de **401** (refresh).
- [ ] Restringir vistas de **ganancias/pérdidas** y **empleados** a Admin (`analytics:profit-loss`, `employees:read`).
- [ ] No asumir el rol por su nombre: usar la lista de `permisos` (es lo que el backend evalúa).
