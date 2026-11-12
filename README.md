# SweetTrack: Plataforma B2B

SweetTrack es un sistema B2B (*Business to Business*) diseñado para que kioscos y minimercados puedan consultar catálogos mayoristas de golosinas y productos de consumo masivo, verificar disponibilidad de stock en tiempo real y emitir pedidos de reabastecimiento. La plataforma centraliza la administración de inventario, la supervisión de cuentas de clientes, el procesamiento de órdenes comerciales, el despacho en depósito y la generación documental de remitos.

---

## Estructura del Repositorio (Monorepo)

El proyecto está organizado en un monorepo modular que desacopla los microservicios de backend, la infraestructura de contenedores y la aplicación cliente web:

```text
SweetTrack/
├── backend/                        # Ecosistema de microservicios (.NET 10 y SQL Server)
│   ├── SweetTrack.slnx             # Solución maestra unificada para desarrollo simultáneo (F5)
│   ├── SweetTrack-AuthAPI/         # Microservicio 1: Autenticación, Identity, JWT y Roles
│   │   └── SweetTrack.Auth.slnx    # Solución individual del microservicio de autenticación
│   ├── SweetTrack-CatalogAPI/      # Microservicio 2: Catálogo mayorista, inventario y carga Excel
│   │   └── SweetTrack.Catalog.slnx # Solución individual del microservicio de catálogo
│   ├── SweetTrack-OrderingAPI/     # Microservicio 3: Pedidos, logística, SignalR y remitos PDF
│   │   └── SweetTrack.Ordering.slnx# Solución individual del microservicio de pedidos
│   └── README.md                   # Guía técnica de arquitectura backend y contratos
├── frontend/                       # Aplicación cliente SPA (Vue 3, TypeScript, Pinia y Vite)
│   ├── src/                        # Vistas de Auth, Dashboard, Catálogo, Pedidos, Despacho y UI
│   ├── tests/                      # Suite de pruebas unitarias y de integración (Vitest)
│   └── README.md                   # Guía técnica de desarrollo frontend y testing
├── docs/                           # Especificaciones funcionales, contratos y hojas de ruta
│   ├── modulo-1-auth-api.md        # Documentación técnica del Módulo 1 (Auth)
│   ├── modulo-2-catalog-api.md     # Documentación técnica del Módulo 2 (Catalog)
│   ├── modulo-3-ordering-api.md    # Documentación técnica del Módulo 3 (Ordering)
│   ├── azure-cloud-deployment-plan.md # Especificación y plan de despliegue en Microsoft Azure
│   └── ROADMAP.md                  # Hoja de ruta y reactivación rápida del proyecto
├── docker-compose.yml              # Orquestación de contenedores (SQL Server 2022)
├── .env.example                    # Plantilla de variables de entorno para Docker
└── README.md                       # Documentación principal del repositorio
```

* 🔗 [Guía técnica de Backend](backend/README.md)
* 🔗 [Guía técnica de Frontend](frontend/README.md)
* 🔗 [Hoja de Ruta del Proyecto](docs/ROADMAP.md)
* 🔗 [Plan de Migración y Despliegue en Microsoft Azure](docs/azure-cloud-deployment-plan.md)

---

## Arquitectura del Sistema

### Backend (Microservicios en .NET 10)
Diseñado bajo el patrón *Database-per-service* y **Clean Architecture** en cuatro capas desacopladas (`Domain`, `Data`, `Application`, `API`):

1. **Auth & Identity API (`Puerto 5010`):**
   * Gestión de cuentas de usuario (kiosqueros y administradores) con ASP.NET Core Identity.
   * Emisión y validación de JSON Web Tokens (JWT) firmados con claims de rol y correo.
   * Sembrado automático e idempotente de roles (`Admin`, `Client`) y credenciales iniciales (`AuthDbSeeder`).

2. **Catalog & Inventory API (`Puerto 5020`):**
   * Gestión de marcas, categorías y productos con concurrencia optimista (`byte[] RowVersion`).
   * Trazabilidad completa de inventario mediante libro mayor inmutable (`StockMovements`).
   * Canal reactivo asíncrono no bloqueante (`System.Threading.Channels`, 50 slots) y worker transaccional con EPPlus (`BulkImportWorker`) para importación masiva de planillas Excel (`.xlsx`).
   * Sembrado inicial idempotente con 16 golosinas emblemáticas del mercado argentino (Arcor, Mondelez, Felfort, Bagley, etc.).

3. **Ordering & Logistics API (`Puerto 5030`):**
   * Máquina de estados determinista (`OrderStateMachine`) con 7 estados de entrega canónicos y auditoría secuencial (`StatusHistory`).
   * Validación estricta del umbral mayorista ($50.000 ARS) y cálculo fiscal automático (21% IVA).
   * Canal en tiempo real con SignalR (`/hubs/orders`) utilizando tickets efímeros de un solo uso (30s) en `IMemoryCache` para evitar la exposición de tokens JWT en las URLs de conexión.
   * Generación de remitos comerciales A4 vectoriales con simulación de código de barras Code128 (`PdfSharpCore`) y servicio de notificación transaccional (`MailKit`).
   * Trabajos periódicos en segundo plano con Quartz.NET (`DailyOrderConsolidationJob` y `CleanupAbandonedCartsJob`).

### Frontend (SPA en Vue 3)
* **Framework:** Vue 3 con Composition API (`<script setup lang="ts">`).
* **Lenguaje:** TypeScript con modo estricto y sin tipos `any`.
* **Estado Global:** Pinia (`userStore` para sesión, `catalogStore` para productos/stock, `cartStore` para compras mayoristas y `orderStore` para seguimiento y despacho).
* **Estilos:** Tailwind CSS con diseño adaptativo y soporte completo para modo oscuro.
* **Componentes y Vistas:**
  * Autenticación (`LoginView`, `RegisterView`) y Panel Principal (`DashboardView`).
  * Catálogo Mayorista interactivo (`CatalogView` en `/catalog`) con buscador reactivo, selector de categorías/marcas y botón "+1 Bulto al Carrito".
  * Carrito de Compras (`CartDrawer`) con cálculo dinámico de bultos/unidades, barra de progreso al mínimo mayorista y modal de checkout (`CheckoutModal`).
  * Mis Pedidos (`MyOrdersView` en `/my-orders`) con stepper visual en vivo y cancelación de órdenes pendientes.
  * Tablero Kanban de Despacho (`DispatchBoardView` en `/dispatch`) para operarios de depósito con descarga de remitos en PDF.
* **Validación:** Esquemas Zod alineados a los contratos DTO del backend.
* **Testing:** Suite completa de 55 pruebas unitarias con Vitest y `@vue/test-utils`.

---

## ☁️ Estrategia de Despliegue en la Nube (Microsoft Azure)

El proyecto cuenta con un plan de modernización de infraestructura para alojar la solución integral en la nube de **Microsoft Azure**, reemplazando los contenedores de desarrollo local por servicios PaaS y Serverless administrados de alta resiliencia:

* **Frontend Web:** Alojado en **Azure Static Web Apps (SWA)** con distribución global por red CDN, compresión HTTP/3 y emisión automática de certificados SSL.
* **Microservicios Backend:** Desplegados sobre **Azure Container Apps (ACA)** con escalabilidad horizontal basada en demanda (KEDA), escalado a cero en horas no operativas e ingress seguro.
* **Bases de Datos Relacionales:** Aprovisionadas en **Azure SQL Database (Elastic Pool)** preservando el patrón *Database-per-service* (`SweetTrack_Auth`, `SweetTrack_Catalog`, `SweetTrack_Orders`) con backups automatizados, redundancia zonal y cifrado transparente (TDE).
* **WebSockets y Tiempo Real:** Desacoplados mediante **Azure SignalR Service (Serverless)**, absorbiendo miles de conexiones concurrentes de kioscos sin saturar los microservicios backend.
* **Aislamiento Total de Secretos:** Implementación de **Zero Secrets** mediante **Azure Key Vault** y **Azure Managed Identities** (Entra ID), eliminando contraseñas en texto plano de las cadenas de conexión.
* **Almacenamiento y Documentación:** **Azure Blob Storage** para la persistencia de remitos comerciales en PDF, imágenes de marcas y archivos Excel.
* **Observabilidad Distribuida:** Monitoreo y rastreo de trazas de punta a punta con **Azure Monitor** y **Application Insights** (OpenTelemetry).

> 📘 Consulta la especificación técnica completa y fases de migración en:  
> [**Plan de Migración y Despliegue Cloud en Microsoft Azure**](docs/azure-cloud-deployment-plan.md).

---

## Estado del Desarrollo

| Módulo / Componente | Alcance Técnico | Estado |
| :--- | :--- | :---: |
| **Módulo 1: Auth & Identity** | Clean Architecture, ASP.NET Core Identity, JWT, roles y seeder de admin | ✅ **Completado** |
| **Módulo 2: Catalog & Inventory** | EF Core 10, concurrencia optimista, canal masivo Excel, worker en segundo plano y UI | ✅ **Completado** |
| **Módulo 3: Ordering & Logistics** | Máquina de estados, SignalR en vivo, remitos PDF, Quartz.NET, carrito y Kanban | ✅ **Completado** |
| **Infraestructura & Secretos** | Contenedor SQL Server 2022 en Docker, volúmenes persistentes y User Secrets | ✅ **Completado** |
| **Solución Maestra** | Solución unificada `backend/SweetTrack.slnx` con perfiles de ejecución F5 | ✅ **Completado** |
| **Despliegue Cloud en Azure** | Arquitectura cloud en Azure Container Apps, Azure SQL y Static Web Apps | 📋 *En Planificación* |

---

## Guía de Ejecución Local

### Prerrequisitos
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Node.js](https://nodejs.org/) (v20 o superior) y `npm`
* [Docker Desktop](https://www.docker.com/) (para Microsoft SQL Server 2022)

---

### 1. Iniciar la Base de Datos (Docker Compose)

El entorno utiliza `docker-compose.yml` para levantar la instancia oficial de SQL Server 2022 con persistencia en el volumen `mssql_data`:

1. Crear el archivo `.env` en la raíz copiando la plantilla:
   ```bash
   cp .env.example .env
   ```
2. Iniciar el contenedor en segundo plano:
   ```bash
   docker compose up -d
   ```
   > El servidor quedará escuchando en `localhost:1433`.

---

### 2. Configurar Secretos de Usuario (.NET User Secrets)

Para garantizar la seguridad y evitar la exposición de credenciales en el repositorio Git, configurar las cadenas de conexión en el almacén de secretos local:

```bash
# 1. Secretos para Auth API
dotnet user-secrets set "ConnectionStrings:AuthDatabase" "Server=localhost,1433;Database=SweetTrack_Auth;User Id=sa;Password=TuPasswordSeguro123!;TrustServerCertificate=True;" --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API
dotnet user-secrets set "DefaultAdmin:Email" "admin@sweettrack.com" --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API
dotnet user-secrets set "DefaultAdmin:Password" "Admin123*!" --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API

# 2. Secretos para Catalog API
dotnet user-secrets set "ConnectionStrings:CatalogDatabase" "Server=localhost,1433;Database=SweetTrack_Catalog;User Id=sa;Password=TuPasswordSeguro123!;TrustServerCertificate=True;" --project backend/SweetTrack-CatalogAPI/SweetTrack.Catalog.API

# 3. Secretos para Ordering API
dotnet user-secrets set "ConnectionStrings:OrderingDatabase" "Server=localhost,1433;Database=SweetTrack_Orders;User Id=sa;Password=TuPasswordSeguro123!;TrustServerCertificate=True;" --project backend/SweetTrack-OrderingAPI/SweetTrack.Ordering.API
dotnet user-secrets set "JwtOptions:Secret" "TuClaveSecretaDeFirmaJwt1234567890SuperLargaYRobusta!" --project backend/SweetTrack-OrderingAPI/SweetTrack.Ordering.API
```

---

### 3. Aplicar Migraciones de Base de Datos

Ejecutar las migraciones de Entity Framework Core para inicializar las bases de datos `SweetTrack_Auth`, `SweetTrack_Catalog` y `SweetTrack_Orders`:

```bash
# Base de datos de Autenticación
dotnet ef database update --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.Data --startup-project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API

# Base de datos de Catálogo e Inventario
dotnet ef database update --project backend/SweetTrack-CatalogAPI/SweetTrack.Catalog.Data --startup-project backend/SweetTrack-CatalogAPI/SweetTrack.Catalog.API

# Base de datos de Pedidos y Logística
dotnet ef database update --project backend/SweetTrack-OrderingAPI/SweetTrack.Ordering.Data --startup-project backend/SweetTrack-OrderingAPI/SweetTrack.Ordering.API
```

---

### 4. Iniciar el Backend (.NET 10)

* **Opción A (Visual Studio / IDE):**
  Abrir la solución maestra [`backend/SweetTrack.slnx`](backend/SweetTrack.slnx). Establecer los tres proyectos de API (`SweetTrack.Auth.API`, `SweetTrack.Catalog.API` y `SweetTrack.Ordering.API`) como proyectos de inicio múltiple y presionar **F5**.

* **Opción B (Terminal / CLI):**
  ```bash
  # Terminal 1 - Auth API (Puerto 5010)
  dotnet run --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API

  # Terminal 2 - Catalog API (Puerto 5020)
  dotnet run --project backend/SweetTrack-CatalogAPI/SweetTrack.Catalog.API

  # Terminal 3 - Ordering API (Puerto 5030)
  dotnet run --project backend/SweetTrack-OrderingAPI/SweetTrack.Ordering.API
  ```

#### Documentación Interactiva (Swagger UI):
* **Auth API:** [http://localhost:5010/swagger](http://localhost:5010/swagger)
* **Catalog API:** [http://localhost:5020/swagger](http://localhost:5020/swagger)
* **Ordering API:** [http://localhost:5030/swagger](http://localhost:5030/swagger)

---

### 5. Iniciar el Frontend (Vue 3)

```bash
cd frontend
npm install
npm run dev
```

La aplicación web estará disponible en: **[http://localhost:5173](http://localhost:5173)**.

> **Acceso Inicial de Prueba:**
> * **Administrador:** `admin@sweettrack.com` / `Admin123*!` (permite ajustar stock, cargar planillas Excel y gestionar el tablero Kanban de despacho).
> * **Kiosco / Cliente:** Puede registrarse libremente desde la pantalla de Registro para armar pedidos y seguir entregas en vivo.

---

### 6. Ejecución de Pruebas Automatizadas

```bash
# Pruebas de Backend (xUnit / Moq - 46 tests pasando al 100%)
dotnet test backend/SweetTrack.slnx

# Pruebas de Frontend (Vitest - 55 tests pasando al 100%)
cd frontend
npm test

# Verificación estricta de tipos TypeScript
npm run type-check
```
