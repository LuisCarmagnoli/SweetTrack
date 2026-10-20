# SweetTrack: Backend y Microservicios

Este directorio contiene los servicios de backend para la plataforma SweetTrack. El sistema adopta una arquitectura de **microservicios independientes** con bases de datos desacopladas (*Database-per-service*) y Clean Architecture en cuatro capas.

---

## Servicios Disponibles

### 1. `SweetTrack-AuthAPI` (Autenticación e Identidad)
Microservicio responsable del registro de usuarios, control de roles (Kiosquero y Administrador) y emisión de JSON Web Tokens (JWT).

#### Estructura en 4 Capas (Clean Architecture):
* **`SweetTrack.Auth.Core`:** Contratos, DTOs de transferencia (`LoginRequestDTO`, `RegisterRequestDTO`, `AuthResponseDTO`) e interfaces de servicios (`IAuthService`, `IJwtTokenGenerator`). Capa desacoplada sin dependencias externas.
* **`SweetTrack.Auth.Data`:** Persistencia y Entity Framework Core (`ApplicationDbContext`), hereda de `IdentityDbContext<IdentityUser>` para la administración y almacenamiento seguro de credenciales.
* **`SweetTrack.Auth.Services`:** Reglas de negocio, validación de contraseñas con `UserManager`, asignación de roles y generación de tokens JWT (`HmacSha256`).
* **`SweetTrack.Auth.API`:** Controladores REST (`AuthController`), pipeline de autenticación Bearer, configuración de inyección de dependencias (`Program.cs`) y Swagger UI.

---

## Stack Tecnológico

* **Framework:** ASP.NET Core Web API (.NET 10)
* **Lenguaje:** C# 13
* **Base de Datos:** Microsoft SQL Server 2022 (ejecutado en contenedor Docker)
* **ORM:** Entity Framework Core 10 (Code-First)
* **Identidad y Seguridad:** ASP.NET Core Identity y JWT Bearer
* **Documentación:** Swagger UI (OpenAPI) con esquema de autorización Bearer

---

## Puesta en Marcha

### 1. Iniciar Microsoft SQL Server (Docker)
Para levantar la instancia oficial de SQL Server en un contenedor:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=TuPasswordFuerte123!" \
   -p 1433:1433 --name sweettrack-sql \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Configuración de Cadena de Conexión y Secretos
Las credenciales locales se administran mediante **User Secrets** en Visual Studio o directamente en el archivo `appsettings.Development.json`:

```json
{
  "ConnectionStrings": {
    "AuthDatabase": "Server=localhost,1433;Database=SweetTrack_Auth;User Id=sa;Password=TuPasswordFuerte123!;TrustServerCertificate=True;"
  },
  "JwtOptions": {
    "Secret": "TuClaveSecretaSuperSeguraParaFirmarTokensJWT12345!",
    "Issuer": "SweetTrack.AuthAPI",
    "Audience": "SweetTrack.Client"
  }
}
```

### 3. Aplicar Migraciones de Base de Datos
Desde la carpeta raíz del backend:

```bash
dotnet ef database update --project SweetTrack-AuthAPI/SweetTrack.Auth.Data --startup-project SweetTrack-AuthAPI/SweetTrack.Auth.API
```

### 4. Ejecutar la API

* **Con Visual Studio:** Abrir `SweetTrack.slnx`, seleccionar el perfil `http` o `https` y presionar **F5**.
* **Por terminal:**
  ```bash
  dotnet run --project SweetTrack-AuthAPI/SweetTrack.Auth.API
  ```

---

## Documentación de Endpoints (Swagger UI)

Al iniciar la API en entorno de desarrollo, el navegador abre automáticamente la interfaz interactiva en:
* **Swagger UI:** `http://localhost:5010/swagger` (o `https://localhost:7205/swagger`)

### Endpoints Expuestos:
* `POST /api/auth/register` — Recibe `email` y `password`, registra el perfil en Identity y asigna el rol correspondiente.
* `POST /api/auth/login` — Valida credenciales y retorna un token JWT válido por 7 días.

Nota: La interfaz cuenta con el botón **Authorize** para ingresar el token Bearer (`Bearer {token}`) y probar endpoints protegidos de forma interactiva.
