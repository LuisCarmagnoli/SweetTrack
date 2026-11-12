# SweetTrack: Backend y Microservicios (.NET 10)

Este directorio alberga el ecosistema de servicios de backend para la plataforma B2B **SweetTrack**. El sistema adopta una arquitectura de **microservicios desacoplados** bajo el patrón *Database-per-service*, implementando **Clean Architecture** en cuatro capas estandarizadas, concurrencia optimista, procesamiento reactivo en segundo plano y comunicación en tiempo real.

---

## 🏛️ Principios y Estándares de Arquitectura

1. **Clean Architecture en 4 Capas:**
   - **`Domain / Core`:** Entidades de negocio, Value Objects, contratos de interfaces y DTOs agnósticos a frameworks.
   - **`Data`:** Persistencia, configuración de Entity Framework Core 10, mapeo relacional fluente (`IEntityTypeConfiguration<T>`) y migraciones reversibles.
   - **`Application / Services`:** Casos de uso, orquestación de servicios, validación de reglas de negocio y trabajadores en segundo plano.
   - **`API`:** Controladores RESTful, autenticación/autorización JWT, endpoints WebSockets (SignalR), filtros y configuración de dependencias (`Program.cs`).
   - **`Tests`:** Cobertura de pruebas unitarias y de integración sobre reglas de dominio y flujos de servicio.
2. **Database-per-Service:** Tres bases de datos lógicas aisladas (`SweetTrack_Auth`, `SweetTrack_Catalog`, `SweetTrack_Orders`) alojadas en Microsoft SQL Server 2022.
3. **Concurrencia Optimista:** Mapeo de `byte[] RowVersion` con detección de colisiones simultáneas mediante `DbUpdateConcurrencyException`.
4. **Cero Secretos en Repositorio:** Cadenas de conexión y llaves JWT gestionadas de forma estricta mediante `dotnet user-secrets` en desarrollo.

---

## 📦 Microservicios del Ecosistema

```text
backend/
├── SweetTrack.slnx                     # Solución maestra unificada para desarrollo simultáneo
├── SweetTrack-AuthAPI/                 # Microservicio de Identidad y Roles (Puerto 5010)
│   ├── SweetTrack.Auth.API/            # Controladores REST, middleware y Swagger
│   ├── SweetTrack.Auth.Core/           # DTOs, interfaces de autenticación y tokens
│   ├── SweetTrack.Auth.Data/           # DbContext con ASP.NET Core Identity y migraciones
│   ├── SweetTrack.Auth.Services/       # Generador de JWT (HMAC-SHA256) y AuthService
│   └── SweetTrack.Auth.Tests/          # 6 Pruebas unitarias (xUnit / Moq)
├── SweetTrack-CatalogAPI/              # Microservicio de Catálogo e Inventario (Puerto 5020)
│   ├── SweetTrack.Catalog.API/         # Controladores de productos, stock y carga masiva
│   ├── SweetTrack.Catalog.Application/ # Pipeline con System.Threading.Channels y EPPlus Worker
│   ├── SweetTrack.Catalog.Data/        # CatalogDbContext, índices y Kardex de stock
│   ├── SweetTrack.Catalog.Domain/      # Entidades Product, Brand, Category, StockMovement
│   └── SweetTrack.Catalog.Tests/       # 13 Pruebas unitarias (xUnit / Moq)
└── SweetTrack-OrderingAPI/             # Microservicio de Pedidos y Logística (Puerto 5030)
    ├── SweetTrack.Ordering.API/        # Controladores REST, SignalR Hub y endpoints de despacho
    ├── SweetTrack.Ordering.Application/# OrderStateMachine, PdfSharpCore, MailKit, Quartz Jobs
    ├── SweetTrack.Ordering.Data/       # OrderingDbContext, RowVersion y migración inicial
    ├── SweetTrack.Ordering.Domain/     # Entidades Order, OrderItem, StatusHistory, DeliveryRoute
    └── SweetTrack.Ordering.Tests/      # 28 Pruebas unitarias (xUnit / Moq)
```

---

### 1. `SweetTrack-AuthAPI` (Identidad y Roles)
* **Puerto HTTP:** `5010`
* **Tecnologías:** ASP.NET Core Identity, JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`), Entity Framework Core.
* **Responsabilidades:**
  - Registro de usuarios con validación de complejidad de contraseña.
  - Asignación de roles (`Admin` para operadores de depósito y administradores, `Client` para kioscos).
  - Emisión de tokens JWT firmados con claims de rol, identificador único y correo electrónico.
  - Inicialización automática e idempotente del usuario administrador por defecto (`admin@sweettrack.com`) mediante `AuthDbSeeder`.
* **Endpoints Clave:**
  - `POST /api/auth/register` — Creación de cuenta kiosco/cliente.
  - `POST /api/auth/login` — Autenticación y obtención del token Bearer.

---

### 2. `SweetTrack-CatalogAPI` (Catálogo e Inventario)
* **Puerto HTTP:** `5020`
* **Tecnologías:** Entity Framework Core 10, `System.Threading.Channels`, `EPPlus` (procesamiento Excel).
* **Responsabilidades:**
  - Catálogo mayorista de golosinas con soporte para precios por bulto, unidades por bulto y código de barras.
  - Trazabilidad y auditoría de inventario tipo Kardex (`StockMovement`) para registrar entradas, salidas y ajustes manuales.
  - Control de concurrencia optimista (`RowVersion`) para evitar sobreescritura de stock entre operadores concurrentes.
  - Importación masiva de planillas Excel (`.xlsx`) mediante cola reactiva no bloqueante en memoria (`Channel<T>`) procesada por `BulkImportWorker` en segundo plano.
  - Precarga de 16 productos emblemáticos argentinos (Arcor, Mondelez, Felfort, Bagley) vía `CatalogDbSeeder`.
* **Endpoints Clave:**
  - `GET /api/catalog/products` — Consulta paginada con filtros por marca, categoría y búsqueda por texto.
  - `GET /api/catalog/products/{id}` — Detalle unitario con metadata de bultos.
  - `POST /api/catalog/products` — Alta de nuevo producto en catálogo (Admin).
  - `PUT /api/catalog/products/{id}` — Actualización con validación de concurrencia optimista (Admin).
  - `PATCH /api/catalog/products/{id}/stock` — Ajuste de existencias con registro en libro mayor (Admin).
  - `POST /api/catalog/products/bulk-upload` — Carga asíncrona de archivos `.xlsx` multipart/form-data.

---

### 3. `SweetTrack-OrderingAPI` (Pedidos, Logística y Tiempo Real)
* **Puerto HTTP:** `5030`
* **Tecnologías:** SignalR, `PdfSharpCore`, `MailKit`, `Quartz.NET`, Entity Framework Core 10.
* **Responsabilidades:**
  - **Máquina de Estados Finita (`OrderStateMachine`):** Orquesta el ciclo de vida de los pedidos a través de 7 estados canónicos:
    `PendienteConfirmacion (1)` ➔ `EnPreparacion (2)` ➔ `ListoParaDespacho (3)` ➔ `EnReparto (4)` ➔ `Entregado (5)`, con salidas terminales `Cancelado (6)` y `NoEntregado (7)`.
  - **Auditoría Inmutable:** Registro secuencial en `StatusHistory` con responsable, fecha UTC y notas operativas.
  - **Reglas Comerciales Mayoristas:** Control de umbral mínimo de compra ($50.000 ARS) y cálculo fiscal discriminado del 21% de IVA.
  - **Handshake Seguro WebSockets:** `OrdersHub` (`/hubs/orders`) utiliza un sistema de tickets efímeros de 30 segundos almacenados en `IMemoryCache` (`POST /api/orders/ws-ticket`), evitando exponer tokens JWT en los query parameters de las conexiones WebSockets.
  - **Remitos Comerciales en PDF:** `PdfWaybillGenerator` genera remitos vectoriales A4 listos para imprimir, con paginación dinámica multipágina (`y > 580`), disposición explícita de `XGraphics`, talón de recepción firmado y código de barras Code128.
  - **Procesos en Segundo Plano:** `DailyOrderConsolidationJob` (agrupamiento logístico por zonas) y `CleanupAbandonedCartsJob` (cancelación de órdenes pendientes vencidas) mediante Quartz.NET.
  - **Seeder Logístico:** Precarga de rutas de reparto zonales (CABA Norte, CABA Sur, CABA Oeste).
* **Endpoints Clave:**
  - `POST /api/orders` — Emisión de pedido mayorista con snapshot inmutable de precios y unidades.
  - `GET /api/orders/my-orders` — Listado de pedidos del kiosco autenticado con historial.
  - `GET /api/orders/{id}` — Detalle exhaustivo de orden y estados.
  - `PATCH /api/orders/{id}/status` — Transición de estado con validación estricta de roles.
  - `GET /api/dispatch/board` — Tablero Kanban para operarios de depósito.
  - `GET /api/dispatch/orders/{id}/remito-pdf` — Descarga binaria del remito oficial en PDF.
  - `POST /api/orders/ws-ticket` — Generación de ticket efímero de conexión para SignalR.

---

## 🛠️ Stack Tecnológico Global

* **Runtime:** [.NET 10 SDK](https://dotnet.microsoft.com/) (C# 13)
* **Base de Datos:** Microsoft SQL Server 2022 (Linux Container)
* **ORM:** Entity Framework Core 10.0 (Code-First con Fluent API)
* **Seguridad:** ASP.NET Core Identity & JWT Bearer
* **Comunicación Realtime:** ASP.NET Core SignalR
* **Motor PDF:** PdfSharpCore
* **Email:** MailKit
* **Tareas Programadas:** Quartz.NET
* **Procesamiento de Archivos:** EPPlus 7
* **Testing:** xUnit, Moq, Entity Framework Core InMemory

---

## 🚀 Puesta en Marcha en Desarrollo

### 1. Iniciar Microsoft SQL Server (Docker Compose)
Asegurarse de tener Docker Desktop iniciado y levantar la base de datos en la raíz del repositorio:

```bash
docker compose up -d
```
> La instancia quedará escuchando en `localhost:1433`.

---

### 2. Configurar Secretos de Usuario (.NET User Secrets)
Ejecutar los siguientes comandos desde la carpeta raíz del repositorio para aislar credenciales sin commitearlas:

```bash
# 1. Configuración de Auth API
dotnet user-secrets set "ConnectionStrings:AuthDatabase" "Server=localhost,1433;Database=SweetTrack_Auth;User Id=sa;Password=TuPasswordSeguro123!;TrustServerCertificate=True;" --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API
dotnet user-secrets set "DefaultAdmin:Email" "admin@sweettrack.com" --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API
dotnet user-secrets set "DefaultAdmin:Password" "Admin123*!" --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API

# 2. Configuración de Catalog API
dotnet user-secrets set "ConnectionStrings:CatalogDatabase" "Server=localhost,1433;Database=SweetTrack_Catalog;User Id=sa;Password=TuPasswordSeguro123!;TrustServerCertificate=True;" --project backend/SweetTrack-CatalogAPI/SweetTrack.Catalog.API

# 3. Configuración de Ordering API
dotnet user-secrets set "ConnectionStrings:OrderingDatabase" "Server=localhost,1433;Database=SweetTrack_Orders;User Id=sa;Password=TuPasswordSeguro123!;TrustServerCertificate=True;" --project backend/SweetTrack-OrderingAPI/SweetTrack.Ordering.API
dotnet user-secrets set "JwtOptions:Secret" "TuClaveSecretaDeFirmaJwt1234567890SuperLargaYRobusta!" --project backend/SweetTrack-OrderingAPI/SweetTrack.Ordering.API
```

---

### 3. Aplicar Migraciones de Base de Datos
Aplicar las migraciones Code-First sobre las tres bases de datos:

```bash
# Migración SweetTrack_Auth
dotnet ef database update --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.Data --startup-project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API

# Migración SweetTrack_Catalog
dotnet ef database update --project backend/SweetTrack-CatalogAPI/SweetTrack.Catalog.Data --startup-project backend/SweetTrack-CatalogAPI/SweetTrack.Catalog.API

# Migración SweetTrack_Orders
dotnet ef database update --project backend/SweetTrack-OrderingAPI/SweetTrack.Ordering.Data --startup-project backend/SweetTrack-OrderingAPI/SweetTrack.Ordering.API
```

---

### 4. Ejecución de los Microservicios

* **Opción A (Visual Studio / Rider - Recomendado):**
  Abrir el archivo de solución unificada `backend/SweetTrack.slnx`. Establecer los tres proyectos `.API` para inicio múltiple simultáneo y presionar **F5**.

* **Opción B (CLI / Terminales separadas):**
  ```bash
  # Terminal 1: Auth API (5010)
  dotnet run --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API

  # Terminal 2: Catalog API (5020)
  dotnet run --project backend/SweetTrack-CatalogAPI/SweetTrack.Catalog.API

  # Terminal 3: Ordering API (5030)
  dotnet run --project backend/SweetTrack-OrderingAPI/SweetTrack.Ordering.API
  ```

---

## 📖 Documentación Interactiva (Swagger UI)

Cada microservicio expone su documentación interactiva OpenAPI y permite autorizar peticiones mediante JWT Bearer:

* **Auth API:** [http://localhost:5010/swagger](http://localhost:5010/swagger)
* **Catalog API:** [http://localhost:5020/swagger](http://localhost:5020/swagger)
* **Ordering API:** [http://localhost:5030/swagger](http://localhost:5030/swagger)

---

## 🧪 Pruebas Automatizadas (xUnit)

La solución cuenta con **47 pruebas unitarias** pasando al 100%:

```bash
# Ejecutar todas las pruebas de la solución backend
dotnet test backend/SweetTrack.slnx
```

| Proyecto de Pruebas | Cobertura / Alcance | Cantidad |
| :--- | :--- | :---: |
| `SweetTrack.Auth.Tests` | Generación de tokens JWT, claims, validaciones de identidad | 6 |
| `SweetTrack.Catalog.Tests` | Concurrencia optimista, movimientos Kardex, canales de importación Excel | 13 |
| `SweetTrack.Ordering.Tests` | Máquina de estados finita, reglas de rol, umbral $50.000, remito PDF | 28 |
| **Total** | **Suite Backend Completa** | **47** |
