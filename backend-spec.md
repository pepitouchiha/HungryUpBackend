# ESPECIFICACIÓN TÉCNICA BACKEND - REST INTERNET API (.NET 9)
## Contexto del Sistema: Automatización de Restaurantes (MVP)

### 1. REGLAS DE ARQUITECTURA (ESTRICTAS)
- **Patrón:** Monolito Modular. Todo vive en la misma base de datos, pero los módulos NO se comunican entre sí directamente a nivel de base de datos o DbContext.
- **Comunicación Inter-módulos:** Solo a través de Interfaces/Servicios públicos compartidos.
- **Tipos de Datos Críticos:** Todos los precios, montos, subtotales y totales DEBEN usar el tipo `decimal` en C# y mapearse como `decimal(18,2)` en la persistencia. Prohibido usar `float` o `double`.
- **Estrategia de Flujo:** 
  - `FastFood`: Creación de orden -> Pago inmediato -> Estado inicial "Pendiente" en cocina. (MesaId es NULL).
  - `Gourmet`: Creación de orden -> Estado "Pendiente" -> Consumo incremental -> Pago y cierre al final. (MesaId es REQUERIDO).

---

### 2. ESTRUCTURA DE CARPETAS SUGERIDA
El proyecto debe inicializarse como una Web API limpia en .NET 9 con la siguiente distribución modular interna:

[Raíz del Proyecto]
├── Program.cs
├── appsettings.json
└── Modules/
├── Catalog/
│   ├── Controllers/
│   ├── Entities/ (Producto, Categoria)
│   └── Services/
├── Orders/
│   ├── Controllers/
│   ├── Entities/ (Pedido, DetallePedido, Mesa)
│   └── Services/
├── Billing/
│   ├── Controllers/
│   ├── Entities/ (Pago)
│   └── Services/
└── Analytics/
├── Controllers/
└── Services/

---

### 3. MODELO DE DATOS CENTRAL (ENTIDADES)

#### Módulo Catálogo
- **Categoria**: `Guid Id`, `string Nombre`, `bool Activo`
- **Producto**: `Guid Id`, `Guid CategoriaId`, `string Nombre`, `decimal Precio`, `int StockActual`

#### Módulo Órdenes
- **Mesa**: `Guid Id`, `int Numero`, `EstadoMesa Estado` (Libre, Ocupada)
- **Pedido**: `Guid Id`, `Guid? MesaId`, `DateTime FechaCreacion`, `EstadoPreparacion EstadoPrep` (Pendiente, EnPreparacion, Entregado), `EstadoFinanciero EstadoFin` (PorPagar, Pagado), `TipoRestaurante Tipo` (FastFood, Gourmet), `int NumeroTurno`
- **DetallePedido**: `Guid Id`, `Guid PedidoId`, `Guid ProductoId`, `int Cantidad`, `decimal PrecioUnitario` (Captura el precio estático del momento de compra).

#### Módulo Facturación
- **Pago**: `Guid Id`, `Guid PedidoId`, `decimal MontoTotal`, `MetodoPago Metodo` (Efectivo, Tarjeta, Transferencia), `DateTime FechaPago`

---

### 4. CONTRATO DE ENDPOINTS (CONTRATOS API)

#### Módulo Catálogo
- `GET /api/v1/products` -> Retorna productos activos con stock.
- `POST /api/v1/products` -> Crea producto. (Admin Only).
  - *Payload:* `{ "nombre": string, "precio": decimal, "stockInicial": int, "categoriaId": Guid }`

#### Módulo Órdenes
- `POST /api/v1/orders` -> Crea una orden.
  - *Payload:* `{ "tipoRestaurante": "FastFood"|"Gourmet", "mesaId": Guid?, "items": [{ "productoId": Guid, "cantidad": int }] }`
- `PUT /api/v1/orders/{id}/status` -> Cambia estado de preparación.
  - *Payload:* `{ "nuevoEstado": "EnPreparacion"|"Entregado" }`

#### Módulo Facturación
- `POST /api/v1/billing/pay` -> Procesa el pago de una orden. Cambia el `EstadoFinanciero` del pedido a `Pagado`. Si es Gourmet, libera la `Mesa` asociada (`EstadoMesa.Libre`).
  - *Payload:* `{ "pedidoId": Guid, "metodoPago": "Efectivo"|"Tarjeta", "montoPagado": decimal }`

#### Módulo Analítica
- `GET /api/v1/analytics/sales-summary?periodo=dia|semana|mes` -> Retorna `{ "ingresosTotales": decimal, "cantidadOrdenes": int }` de las órdenes con `EstadoFinanciero.Pagado`.

---

### 5. PASO A PASO PARA LA IA (EJECUCIÓN POR COMANDOS CLI)
1. Crear la Web API usando `dotnet new webapi`.
2. Instalar Entity Framework Core (InMemory para desarrollo rápido o SQL Server/PostgreSQL local según se decida).
3. Estructurar las carpetas por módulos.
4. Crear las entidades y los contextos de base de datos aislados por módulo.
5. Desarrollar la lógica de controladores y servicios usando inyección de dependencias.