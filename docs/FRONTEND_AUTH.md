# Integración de Autenticación — Guía para el Frontend (Angular)

Esta guía describe **todo lo que el frontend debe cambiar** tras la migración del backend a un
esquema de **access token corto + refresh token con rotación estricta**.

> ⚠️ **Cambio incompatible (breaking change).** La respuesta de `POST /api/auth/login` cambió:
> ya **no** existe el campo `token`. Ahora hay `accessToken` y `refreshToken`. Hay que actualizar
> el servicio de auth y el interceptor.

---

## 1. Conceptos clave

| Token | Duración | Para qué sirve | Dónde va |
|-------|----------|----------------|----------|
| **Access token** (JWT) | **15 minutos** | Autorizar cada request a la API | Header `Authorization: Bearer <accessToken>` |
| **Refresh token** | **7 días** | Obtener un nuevo par de tokens cuando el access expira | Body JSON al llamar `/api/auth/refresh` |

**Rotación estricta:** cada vez que usas el refresh token en `/api/auth/refresh`, el backend
**te devuelve un refresh token NUEVO e invalida el anterior**. Debes **reemplazar** siempre el
refresh token guardado por el que viene en la respuesta.

**Detección de reuso:** si se vuelve a usar un refresh token ya rotado (señal de robo), el backend
**revoca TODAS las sesiones activas de ese usuario**. El usuario tendrá que volver a iniciar sesión.

---

## 2. Endpoints

Base URL (desarrollo): `http://localhost:5216` · Producción: **siempre HTTPS**.

### 2.1 Login — `POST /api/auth/login`  *(público)*

**Request**
```json
{ "username": "admin", "password": "admin123" }
```

**Response 200**
```json
{
  "id": 1,
  "username": "admin",
  "email": "admin@hungryup.com",
  "fullName": "Administrador",
  "role": "Admin",
  "enterpriseId": 1,
  "enterpriseName": "HungryUp Restaurant",
  "accessToken": "eyJhbGciOiJIUzI1Ni...",
  "accessTokenExpiration": "2026-06-21T10:15:00.0000000Z",
  "refreshToken": "v3Jq...base64...==",
  "refreshTokenExpiration": "2026-06-28T10:00:00.0000000Z"
}
```

**Response 401** → credenciales inválidas.
```json
{ "message": "Credenciales inválidas." }
```

### 2.2 Refresh — `POST /api/auth/refresh`  *(público)*

Se llama cuando el access token expiró (típicamente al recibir un **401**).

**Request**
```json
{ "refreshToken": "v3Jq...base64...==" }
```

**Response 200** → **mismo shape que login** (con un `accessToken` y un `refreshToken` NUEVOS).
**Response 401** → el refresh token es inválido, expiró o fue revocado ⇒ **cerrar sesión y redirigir a login**.

### 2.3 Logout — `POST /api/auth/logout`  *(público)*

Revoca el refresh token actual (cierra la sesión de **este** dispositivo).

**Request**
```json
{ "refreshToken": "v3Jq...base64...==" }
```

**Response 204** (sin cuerpo). Siempre responde 204 aunque el token ya no exista.

### 2.4 Resto de la API  *(protegida)*

Todos los endpoints (`/api/v1/products`, `/orders`, `/billing`, `/analytics`, `/users`, …) exigen
`Authorization: Bearer <accessToken>`. Sin él → **401**. Con rol insuficiente → **403**
(p. ej. `/api/v1/users` requiere rol `Admin`).

---

## 3. Almacenamiento de tokens (recomendación)

- **Access token:** guardarlo **en memoria** (un servicio/`BehaviorSubject`). Es de vida corta.
- **Refresh token:** guardarlo en `localStorage` para sobrevivir recargas de página.

> 🔒 **Nota de seguridad (XSS):** `localStorage` es legible por JavaScript, por lo que es vulnerable
> a XSS. La mitigación definitiva es mover el refresh token a una **cookie `HttpOnly` + `Secure`**
> (pendiente de acordar con backend, porque implica CORS con credenciales y protección CSRF).
> Mientras tanto: **sanitiza toda entrada, usa `DomSanitizer` y una CSP estricta.**

---

## 4. Flujo que debe implementar el frontend

```
1. Login  → guardar { accessToken (memoria), refreshToken (localStorage) }.
2. Cada request → adjuntar  Authorization: Bearer <accessToken>.
3. Si una respuesta es 401:
      a. Llamar /api/auth/refresh con el refreshToken guardado.
      b. Si 200 → REEMPLAZAR ambos tokens y REINTENTAR la request original.
      c. Si 401 → limpiar tokens y redirigir a /login.
4. Logout → llamar /api/auth/logout con el refreshToken y limpiar el almacenamiento.
```

**Regla de oro de la rotación:** tras cada `/refresh` exitoso, **sobrescribe** el refresh token
guardado con el nuevo. Si guardas el viejo, el siguiente refresh fallará y, peor aún, disparará la
detección de reuso (revoca todas las sesiones).

---

## 5. Ejemplo de Interceptor en Angular (standalone, `HttpInterceptorFn`)

```ts
// auth.interceptor.ts
import { HttpInterceptorFn, HttpErrorResponse } from '@angular/common/http';
import { inject } from '@angular/core';
import { BehaviorSubject, catchError, filter, switchMap, take, throwError } from 'rxjs';
import { AuthService } from './auth.service';

let refreshing = false;
const refreshed$ = new BehaviorSubject<string | null>(null);

export const authInterceptor: HttpInterceptorFn = (req, next) => {
  const auth = inject(AuthService);

  // No adjuntar el token a los endpoints públicos de auth.
  const isAuthCall = req.url.includes('/api/auth/');
  const access = auth.accessToken;
  const authReq = (access && !isAuthCall)
    ? req.clone({ setHeaders: { Authorization: `Bearer ${access}` } })
    : req;

  return next(authReq).pipe(
    catchError((err: HttpErrorResponse) => {
      if (err.status !== 401 || isAuthCall) return throwError(() => err);

      // Un solo refresh concurrente; el resto espera el token nuevo.
      if (refreshing) {
        return refreshed$.pipe(
          filter(t => t !== null),
          take(1),
          switchMap(t => next(req.clone({ setHeaders: { Authorization: `Bearer ${t}` } }))),
        );
      }

      refreshing = true;
      refreshed$.next(null);

      return auth.refresh().pipe(
        switchMap(res => {
          refreshing = false;
          refreshed$.next(res.accessToken);
          return next(req.clone({ setHeaders: { Authorization: `Bearer ${res.accessToken}` } }));
        }),
        catchError(refreshErr => {
          refreshing = false;
          auth.logoutLocal();        // limpiar tokens + redirigir a /login
          return throwError(() => refreshErr);
        }),
      );
    }),
  );
};
```

```ts
// auth.service.ts (extracto)
import { HttpClient } from '@angular/common/http';
import { Injectable, signal } from '@angular/core';
import { tap } from 'rxjs';

interface AuthResponse {
  id: number; username: string; email: string; fullName: string;
  role: string; enterpriseId: number; enterpriseName: string;
  accessToken: string; accessTokenExpiration: string;
  refreshToken: string; refreshTokenExpiration: string;
}

@Injectable({ providedIn: 'root' })
export class AuthService {
  private readonly base = '/api/auth';
  readonly accessTokenSig = signal<string | null>(null);

  constructor(private http: HttpClient) {}

  get accessToken() { return this.accessTokenSig(); }
  private get refreshToken() { return localStorage.getItem('refreshToken'); }

  login(username: string, password: string) {
    return this.http.post<AuthResponse>(`${this.base}/login`, { username, password })
      .pipe(tap(res => this.store(res)));
  }

  refresh() {
    return this.http.post<AuthResponse>(`${this.base}/refresh`, { refreshToken: this.refreshToken })
      .pipe(tap(res => this.store(res)));          // ← rotación: sobrescribe SIEMPRE
  }

  logout() {
    const rt = this.refreshToken;
    return this.http.post(`${this.base}/logout`, { refreshToken: rt })
      .pipe(tap(() => this.logoutLocal()));
  }

  logoutLocal() {
    this.accessTokenSig.set(null);
    localStorage.removeItem('refreshToken');
    // this.router.navigate(['/login']);
  }

  private store(res: AuthResponse) {
    this.accessTokenSig.set(res.accessToken);
    localStorage.setItem('refreshToken', res.refreshToken);   // ← clave de la rotación estricta
  }
}
```

Registro del interceptor (Angular standalone):
```ts
// app.config.ts
provideHttpClient(withInterceptors([authInterceptor]))
```

---

## 6. HTTPS

- En **producción todo el tráfico debe ir por HTTPS** (el backend activa HSTS fuera de desarrollo).
  Configura la `baseUrl` del frontend con `https://` y no mezcles contenido HTTP.
- En **desarrollo** la API escucha en `http://localhost:5216` y `https://localhost:7190`.

---

## 7. Tabla de errores

| Código | Cuándo | Acción del frontend |
|--------|--------|---------------------|
| **401** en endpoint normal | Access token ausente/expirado/ inválido | Intentar `/refresh` y reintentar |
| **401** en `/refresh` | Refresh inválido/expirado/revocado (o **reuso detectado**) | Limpiar tokens → redirigir a `/login` |
| **403** | Autenticado pero **sin rol** suficiente | Mostrar "no autorizado"; no reintentar |
| **400** | Validación (p. ej. crear usuario duplicado) | Mostrar `detail` del ProblemDetails |

---

## 8. Checklist de migración para el front

- [ ] Cambiar la lectura de `token` → `accessToken` en la respuesta de login.
- [ ] Guardar y enviar el `refreshToken`.
- [ ] Implementar el interceptor con refresh + reintento y manejo de concurrencia.
- [ ] **Sobrescribir siempre** el refresh token tras cada `/refresh` (rotación estricta).
- [ ] Implementar `logout` llamando a `/api/auth/logout`.
- [ ] Redirigir a login cuando `/refresh` devuelva 401.
- [ ] Usar HTTPS en producción.
