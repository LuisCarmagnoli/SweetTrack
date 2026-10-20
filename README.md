# SweetTrack: Plataforma B2B

SweetTrack es un sistema B2B (Business to Business) diseñado para que kioscos y minimercados puedan consultar catálogos de productos, verificar disponibilidad de stock en tiempo real y emitir pedidos de reabastecimiento. La plataforma centraliza la administración de inventario, la supervisión de cuentas de clientes y el procesamiento de órdenes comerciales.

---

## Estructura del Repositorio (Monorepo)

El proyecto está organizado en un monorepo que separa los servicios de backend y el cliente web:

```text
SweetTrack/
├── backend/                  # Ecosistema de microservicios (.NET 10 y SQL Server)
│   └── SweetTrack-AuthAPI/   # Microservicio de autenticación, identidad y JWT
│   └── README.md             # Guía técnica de configuración, Docker y base de datos
├── frontend/                 # Aplicación cliente SPA (Vue 3, TypeScript y Vite)
│   └── README.md             # Guía técnica de desarrollo, testing con Vitest y UI
├── docs/                     # Especificaciones funcionales y de arquitectura
└── README.md                 # Documentación principal del repositorio
```

* [Guía técnica de Backend](backend/README.md)
* [Guía técnica de Frontend](frontend/README.md)

---

## Arquitectura del Sistema

### Backend (Microservicios en .NET 10)
Diseñado bajo el patrón *Database-per-service* y Clean Architecture en cuatro capas (`Core`, `Data`, `Services`, `API`):
* **Auth & Identity API:** Gestión de usuarios (kiosqueros y administradores), validación de credenciales con ASP.NET Core Identity y emisión de tokens JWT.
* **Catalog & Inventory API:** *(En planificación)* Gestión de marcas, productos y control de stock implementando el patrón repositorio.
* **Ordering API:** *(Planificado)* Recepción de carritos de compras y procesamiento de órdenes de pedido.

### Frontend (SPA en Vue 3)
* **Framework:** Vue 3 con Composition API (`<script setup lang="ts">`).
* **Lenguaje:** TypeScript con tipado estricto.
* **Estado Global:** Pinia (`userStore` para sesión y autenticación).
* **Estilos:** Tailwind CSS con soporte para modo oscuro.
* **Validación:** Zod alineado a los contratos y DTOs del backend.
* **Testing:** Suite de pruebas unitarias con Vitest y `@vue/test-utils`.

---

## Estado del Desarrollo

* **[Completado] Microservicio 1 (Auth API):** Implementado con Clean Architecture, JWT y documentación Swagger UI.
* **[Completado] Base de Datos Auth:** SQL Server 2022 configurado sobre contenedor Docker.
* **[Completado] Frontend SPA (Vue 3):** Implementado (Vistas de Login y Registro, Dashboard, Store de sesión y suite de pruebas unitarias en Vitest).
* **[En planificación] Microservicio 2 (Catalog API):** Diseño técnico y definición de contratos de datos.
* **[Planificado] Microservicio 3 (Ordering API):** Gestión de compras y checkout.
* **[Planificado] Despliegue unificado (Docker Compose):** Integración de servicios en `docker-compose.yml`.

---

## Guía de Ejecución Local

### Prerrequisitos
* [.NET 10 SDK](https://dotnet.microsoft.com/)
* [Node.js](https://nodejs.org/) (v20 o superior)
* [Docker Desktop](https://www.docker.com/) (para Microsoft SQL Server)

### 1. Iniciar la Base de Datos (Docker)
```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=TuPasswordFuerte123!" \
   -p 1433:1433 --name sweettrack-sql \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Iniciar el Backend (.NET 10)
```bash
# Aplicar migraciones de base de datos
dotnet ef database update --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.Data --startup-project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API

# Iniciar la API
dotnet run --project backend/SweetTrack-AuthAPI/SweetTrack.Auth.API
```
> La documentación interactiva de Swagger UI estará disponible en: `http://localhost:5010/swagger`

### 3. Iniciar el Frontend (Vue 3)
```bash
cd frontend
npm install
npm run dev
```
> La aplicación web estará disponible en: `http://localhost:5173`
