# SweetTrack: Plataforma B2B

SweetTrack es un sistema B2B (Business to Business) diseñado para que kioscos y minimercados puedan ingresar, explorar catálogos de golosinas, verificar stock y realizar pedidos. Por otro lado, la plataforma permite a la administración gestionar el inventario de manera centralizada, supervisar a los clientes y procesar los pedidos entrantes.

---

## Arquitectura de Microservicios

El backend está construido con **.NET C#**, dividiendo las responsabilidades en microservicios independientes bajo el patrón *Database-per-service* y una arquitectura limpia en capas.

* **Auth & Identity API:** Gestiona el registro de usuarios y emite los tokens JWT para la seguridad de la plataforma.
* **Catalog & Inventory API:** Gestiona las marcas y el stock de productos implementando el Patrón Repositorio y Mapster.
* **Ordering API:** Recibe los carritos de compras y genera los pedidos, validando la identidad del usuario de forma segura.
* **Frontend SPA:** Interfaz de usuario construida con **Vue 3**, **Tailwind CSS** y **Pinia** para la gestión del estado global y el carrito de compras.

---

## Estándares de Calidad y Validación

* **Validación de Entradas (FluentValidation):** Implementado en los DTOs de entrada para garantizar integridad de datos antes de alcanzar la capa de controladores.
* **Testing Automatizado (xUnit & Moq):** Cobertura de pruebas unitarias sobre la lógica de negocio en la capa de servicios (`SweetTrack.Auth.Tests`).

---

## Estado del Desarrollo

* ✅ **Microservicio 1 (Auth API):** Implementado con arquitectura en cuatro capas (Core, Data, Services, API).
* ✅ **Base de Datos Auth:** Configurada en un contenedor Docker.
* 🚧 **Testing y Validación Auth:** Pruebas unitarias con xUnit/Moq y validaciones con FluentValidation.
* ⏳ **Microservicio 2 (Catalog API):** En etapa de planificación técnica.
* ⏳ **Microservicio 3 (Ordering API):** Pendiente.
* ⏳ **Frontend (Vue 3):** Pendiente.
* ⏳ **Despliegue unificado (Docker Compose):** Pendiente de integración global (`docker-compose.yml`).

---

## Guía de Ejecución Local

Para levantar el proyecto en tu entorno de desarrollo, asegúrate de cumplir con los siguientes requisitos:

1. **Stack principal:** ASP.NET Core Web API (.NET 10) y Microsoft SQL Server 2022.
2. **Seguridad y Credenciales:** La configuración local (cadenas de conexión y firma JWT) se maneja mediante la herramienta **User Secrets** de Visual Studio. El archivo `appsettings.json` solo contiene valores de plantilla por seguridad.
3. **Pruebas de API:** Los endpoints expuestos (`/api/auth/register` y `/api/auth/login`) están documentados y se pueden probar directamente usando **Swagger** o ejecutando las peticiones a través de los archivos `.http` nativos de Visual Studio.
