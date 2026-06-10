# ESTADO DEL PROYECTO: SISTEMA DE RESTAURANTES MVP

## 1. Lo que ya está construido (Backend)
- **Arquitectura:** .NET 9 Monolito Modular.
- **Módulos:** Catalog, Orders, Billing, Analytics.
- **Regla Crítica:** Separación estricta entre flujo `FastFood` (pago inmediato, sin mesa) y `Gourmet` (cuenta abierta, asignación de mesa).
- **Puerto de API Local:** `http://localhost:5216`.

## 2. Fase Actual: Desarrollo del Frontend
- **Stack:** Angular (versión reciente, componentes Standalone recomendados), TypeScript, RxJS.
- **Objetivo Inmediato:** Construir las interfaces consumiendo la API construida en la fase anterior.