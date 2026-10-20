# SweetTrack: Plataforma B2B

SweetTrack es un sistema B2B (Business to Business) diseñado para que kioscos y minimercados puedan explorar catálogos de golosinas, verificar disponibilidad de stock en tiempo real y realizar pedidos de reabastecimiento. La plataforma permite a los distribuidores y administradores gestionar el inventario de manera centralizada, supervisar clientes y procesar pedidos entrantes.

---

## 🏛️ Estructura del Repositorio (Monorepo)

El proyecto está organizado en un monorepo dividido limpiamente entre servicios de backend y cliente web:

```text
SweetTrack/
├── backend/                  # Ecosistema de Microservicios (.NET 10 & SQL Server)
│   └── SweetTrack-AuthAPI/   # Microservicio 1: Autenticación, Identidad y JWT
│   └── README.md             # Guía de configuración, Docker y base de datos
├── frontend/                 # Aplicación Cliente SPA (Vue 3, TypeScript & Vite)
│   └── README.md             # Guía de desarrollo, testing con Vitest y UI
├── docs/                     # Documentación de arquitectura y especificaciones
└── README.md                 # Portada principal del proyecto
```

* 🔗 **[Guía técnica de Backend](backend/README.md)**
* 🔗 **[Guía técnica de Frontend](frontend/README.md)**

---

## 🚀 Arquitectura del Sistema

### Backend (Microservicios en .NET 10)
Diseñado bajo el patrón *Database-per-service* y Clean Architecture en cuatro capas (`Core`, `Data`, `Services`, `API`):
* **Auth & Identity API:** Gestión de usuarios (kiosqueros y administradores), validación con ASP.NET Core Identity y emisión de tokens seguros JWT.
* **Catalog & Inventory API:** *(En planificación)* Gestión de marcas, productos y control de stock implementando el patrón repositorio.
* **Ordering API:** *(Pendiente)* Recepción de carritos de compras y procesamiento de órdenes de pedido.

### Frontend (SPA Moderna en Vue 3)
* **Framework:** Vue 3 con Composition API (`<script setup lang="ts">`).
* **Lenguaje:** TypeScript con tipado estricto.
* **Estado Global:** Pinia (`userStore` para sesión y autenticación).
* **Estilado:** Tailwind CSS con soporte para modo oscuro.
* **Validación:** Zod alineado a los contratos y DTOs del backend.
* **Testing:** Suite de pruebas unitarias con Vitest y `@vue/test-utils`.

---

## 📊 Estado del Desarrollo

* ✅ **Microservicio 1 (Auth API):** Implementado con Clean Architecture, JWT y Swagger UI.
* ✅ **Base de Datos Auth:** SQL Server 2022 configurado sobre contenedor Docker.
* ✅ **Frontend SPA (Vue 3):** Implementado (Vistas de Login y Registro, Dashboard, Store de sesión y pruebas unitarias con Vitest).
* ⏳ **Microservicio 2 (Catalog API):** En etapa de planificación técnica.
* ⏳ **Microservicio 3 (Ordering API):** Pendiente.
* ⏳ **Despliegue unificado (Docker Compose):** Pendiente de integración global (`docker-compose.yml`).

---

## 🛠️ Guía Rápida de Ejecución Local

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
