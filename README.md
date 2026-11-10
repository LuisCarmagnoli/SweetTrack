# SweetTrack: Plataforma B2B

SweetTrack es un sistema B2B (*Business to Business*) diseñado para que kioscos y minimercados puedan consultar catálogos mayoristas de golosinas y productos de consumo masivo, verificar disponibilidad de stock en tiempo real y emitir pedidos de reabastecimiento. La plataforma centraliza la administración de inventario, la supervisión de cuentas de clientes y el procesamiento de órdenes comerciales.

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
│   └── README.md                   # Guía técnica de arquitectura backend y contratos
├── frontend/                       # Aplicación cliente SPA (Vue 3, TypeScript, Pinia y Vite)
│   ├── src/                        # Vistas de Auth, Dashboard, Catálogo y componentes UI
│   ├── tests/                      # Suite de pruebas unitarias y de integración (Vitest)
│   └── README.md                   # Guía técnica de desarrollo frontend y testing
├── docs/                           # Especificaciones funcionales, contratos y hojas de ruta
├── docker-compose.yml              # Orquestación de contenedores (SQL Server 2022)
├── .env.example                    # Plantilla de variables de entorno para Docker
└── README.md                       # Documentación principal del repositorio
```

* 🔗 [Guía técnica de Backend](backend/README.md)
* 🔗 [Guía técnica de Frontend](frontend/README.md)
* 🔗 [Hoja de Ruta del Proyecto](docs/ROADMAP.md)

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

3. **Ordering & Logistics API (`Planificado - Módulo 3`):**
   * Recepción de carritos de compras, cálculo de listas de precios y emisión de órdenes de reabastecimiento.

### Frontend (SPA en Vue 3)
* **Framework:** Vue 3 con Composition API (`<script setup lang="ts">`).
* **Lenguaje:** TypeScript con modo estricto y sin tipos `any`.
* **Estado Global:** Pinia (`userStore` para sesión/tokens y `catalogStore` para productos, filtros y stock).
* **Estilos:** Tailwind CSS con diseño adaptativo y soporte completo para modo oscuro.
* **Componentes y Vistas:**
  * Autenticación (`LoginView`, `RegisterView`) y Panel Principal (`DashboardView`).
  * Catálogo Mayorista interactivo (`CatalogView` en `/catalog`) con buscador reactivo (debounce 350ms), selector de categorías/marcas y cálculo de precios por bulto/unidad.
  * Modales administrativos para ajuste manual de inventario (`StockAdjustmentModal`) y carga masiva de Excel con barra de progreso (`BulkImportModal`).
* **Validación:** Esquemas Zod alineados estrictamente a los contratos DTO del backend.
* **Testing:** Suite completa de pruebas con Vitest y `@vue/test-utils` (33 pruebas automatizadas).

---

## Estado del Desarrollo

| Módulo / Componente | Alcance Técnico | Estado |
| :--- | :--- | :---: |
| **Módulo 1: Auth & Identity** | Clean Architecture, ASP.NET Core Identity, JWT, roles y seeder de admin | ✅ **Completado** |
| **Módulo 2: Catalog Backend** | EF Core 10, concurrencia optimista, canal masivo Excel, worker en segundo plano | ✅ **Completado** |
| **Módulo 2: Catalog UI** | Vista `/catalog`, modales de ajuste y carga Excel, Pinia store y diseño responsive | ✅ **Completado** |
| **Infraestructura & Secretos** | Contenedor SQL Server 2022 en Docker, volúmenes persistentes y User Secrets | ✅ **Completado** |
| **Solución Maestra** | Solución unificada `backend/SweetTrack.slnx` con perfiles de ejecución F5 | ✅ **Completado** |
| **Módulo 3: Ordering & Logistics** | Carrito de compras, procesamiento de pedidos, estados de entrega y checkout | 📋 *Próximo Hito* |

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
```

---

### 3. Aplicar Migraciones de Base de Datos

Ejecutar las migraciones de Entity Framework Core para inicializar las bases de datos `SweetTrack_Auth` y `SweetTrack_Catalog`:

```bash
# Base de datos de Autenticación
dotnet ef database update --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.Data --startup-project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API

# Base de datos de Catálogo e Inventario
dotnet ef database update --project backend/SweetTrack-CatalogAPI/SweetTrack.Catalog.Data --startup-project backend/SweetTrack-CatalogAPI/SweetTrack.Catalog.API
```

---

### 4. Iniciar el Backend (.NET 10)

* **Opción A (Visual Studio / IDE):**
  Abrir la solución maestra [`backend/SweetTrack.slnx`](backend/SweetTrack.slnx). Establecer ambos proyectos de API (`SweetTrack.Auth.API` y `SweetTrack.Catalog.API`) como proyectos de inicio múltiple y presionar **F5**.

* **Opción B (Terminal / CLI):**
  ```bash
  # Terminal 1 - Auth API (Puerto 5010)
  dotnet run --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API

  # Terminal 2 - Catalog API (Puerto 5020)
  dotnet run --project backend/SweetTrack-CatalogAPI/SweetTrack.Catalog.API
  ```

#### Documentación Interactiva (Swagger UI):
* **Auth API:** [http://localhost:5010/swagger](http://localhost:5010/swagger)
* **Catalog API:** [http://localhost:5020/swagger](http://localhost:5020/swagger)

---

### 5. Iniciar el Frontend (Vue 3)

```bash
cd frontend
npm install
npm run dev
```

La aplicación web estará disponible en: **[http://localhost:5173](http://localhost:5173)**.

> **Acceso Inicial de Prueba:**
> * **Administrador:** `admin@sweettrack.com` / `Admin123*!` (permite ajustar stock manual y cargar planillas Excel).
> * **Kiosco / Cliente:** Puede registrarse libremente desde la pantalla de Registro en la aplicación web.

---

### 6. Ejecución de Pruebas Automatizadas

```bash
# Pruebas de Backend (xUnit / Moq - 19 tests)
dotnet test backend/SweetTrack.slnx

# Pruebas de Frontend (Vitest - 33 tests)
cd frontend
npm test

# Verificación estricta de tipos TypeScript
npm run typecheck
```
