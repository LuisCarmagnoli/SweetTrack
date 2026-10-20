# SweetTrack: Backend & Microservicios

Este directorio contiene el ecosistema de servicios backend para la plataforma SweetTrack. Está diseñado siguiendo el patrón de **microservicios independientes** con bases de datos desacopladas (*Database-per-service*) y Clean Architecture.

---

## 🏗️ Microservicios Disponibles

### 1. `SweetTrack-AuthAPI` (Autenticación e Identidad)
Microservicio encargado del registro de usuarios, control de roles (Kiosquero vs Administrador) y emisión de JSON Web Tokens (JWT).

#### Estructura en 4 Capas (Clean Architecture):
* **`SweetTrack.Auth.Core`:** Contratos, DTOs de transferencia (`LoginRequestDTO`, `RegisterRequestDTO`, `AuthResponseDTO`) e interfaces (`IAuthService`, `IJwtTokenGenerator`). Cero dependencias externas.
* **`SweetTrack.Auth.Data`:** Persistencia y Entity Framework Core (`ApplicationDbContext`), hereda de `IdentityDbContext<IdentityUser>` para el almacenamiento seguro de usuarios y credenciales.
* **`SweetTrack.Auth.Services`:** Lógica de negocio, validación de contraseñas con `UserManager`, asignación de roles y generación de tokens JWT (`HmacSha256`).
* **`SweetTrack.Auth.API`:** Controladores REST (`AuthController`), middlewares de autenticación Bearer, configuración de arranque (`Program.cs`) y Swagger UI.

---

## 🧰 Stack Tecnológico

* **Framework:** ASP.NET Core Web API (.NET 10)
* **Lenguaje:** C# 13
* **Base de Datos:** Microsoft SQL Server 2022 (en contenedor Docker)
* **ORM:** Entity Framework Core 10 (Code-First)
* **Identidad y Seguridad:** ASP.NET Core Identity & JWT Bearer
* **Documentación:** Swagger UI (OpenAPI) con soporte de autorización Bearer

---

## 🚀 Guía de Puesta en Marcha

### 1. Levantar Microsoft SQL Server (Docker)
Si no tienes una instancia local de SQL Server, levanta la imagen oficial en Docker:

```bash
docker run -e "ACCEPT_EULA=Y" -e "MSSQL_SA_PASSWORD=TuPasswordFuerte123!" \
   -p 1433:1433 --name sweettrack-sql \
   -d mcr.microsoft.com/mssql/server:2022-latest
```

### 2. Configurar Cadena de Conexión y Secretos
Por seguridad, las credenciales no están hardcodeadas en `appsettings.json`. Puedes configurarlas mediante **User Secrets** en Visual Studio o directamente en `appsettings.Development.json`:

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

* **Con Visual Studio:** Abre `SweetTrack.slnx`, selecciona el perfil `http` o `https` y presiona **F5** o Play.
* **Por terminal:**
  ```bash
  dotnet run --project SweetTrack-AuthAPI/SweetTrack.Auth.API
  ```

---

## 📖 Documentación de Endpoints (Swagger UI)

Una vez iniciada la API en modo desarrollo, Visual Studio abrirá automáticamente tu navegador en:
* **Swagger UI:** `http://localhost:5010/swagger` (o `https://localhost:7205/swagger`)

### Endpoints Expuestos:
* `POST /api/auth/register` — Recibe `email` y `password`, crea el usuario en Identity y asigna el rol predeterminado.
* `POST /api/auth/login` — Valida credenciales y emite un token JWT con validez por 7 días.

> 💡 **Autenticación en Swagger:** La interfaz cuenta con el botón **Authorize** en la esquina superior para ingresar el token Bearer (`Bearer {token}`) y probar endpoints protegidos de forma interactiva.
