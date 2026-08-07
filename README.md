# Cashito API — El motor financiero detrás de Cashito

API REST construida con ASP.NET Core y arquitectura DDD, lista para escalar con nuevos módulos de gestión financiera.

---

## Descripción del sistema

**Cashito API** es el backend de Cashito, una plataforma orientada a la gestión de finanzas personales. Expone servicios de autenticación, gestión de usuarios y control de acceso mediante JWT, sentando las bases para módulos financieros futuros.

En su estado actual, el proyecto cuenta con un módulo de **Identidad y Acceso (IAM)** completamente funcional.

**Funcionalidades implementadas:**

- Registro e inicio de sesión de usuarios con JWT
- Gestión de usuarios (consulta, creación, actualización, eliminación lógica)
- Asignación de roles (`Admin`, `User`) con control de acceso por permisos
- Cambio de contraseña con verificación de la contraseña actual
- Documentación interactiva de la API con Swagger

---

## Flujo del negocio

```
┌─────────────┐     POST /authentication/sign-up      ┌──────────────┐
│   Cliente   │ ────────────────────────────────────► │   Backend    │
│  (Frontend) │                                       │  (Registro)  │
└─────────────┘                                       └──────┬───────┘
       │                                                     │
       │                              Asigna rol "User" + hash BCrypt
       │                                                     ▼
       │                                              ┌──────────────┐
       │     POST /authentication/sign-in             │   MySQL DB   │
       └────────────────────────────────────────────► │  (Usuarios)  │
                                                      └──────┬───────┘
                                                             │
       ◄──────────────── JWT Bearer Token ──────────────────┘
       │
       │  Authorization: Bearer {token}
       ▼
┌─────────────┐     GET/PUT/DELETE /users/*           ┌──────────────┐
│   Cliente   │ ────────────────────────────────────► │  Endpoints   │
│ (Protegido) │                                       │  protegidos  │
└─────────────┘                                       └──────────────┘
       │
       │  Rol Admin → actualizar / eliminar usuarios
       │  Usuario autenticado → consultar perfil, cambiar contraseña
       ▼
┌─────────────┐
│  Swagger UI │  ← Documentación y pruebas de la API
└─────────────┘
```

---

## Tecnologías usadas

- **.NET 9** — Runtime y framework base
- **ASP.NET Core Web API** — Exposición de endpoints REST
- **Entity Framework Core 9** — ORM y acceso a datos
- **MySQL** — Base de datos relacional (`MySql.EntityFrameworkCore`)
- **JWT Bearer** — Autenticación basada en tokens (`Microsoft.AspNetCore.Authentication.JwtBearer`)
- **BCrypt.Net-Next** — Hash seguro de contraseñas
- **Swashbuckle (Swagger)** — Documentación OpenAPI interactiva
- **Arquitectura DDD** — Bounded contexts, agregados, CQRS (Commands / Queries)

---

## Arquitectura

Organizado por **bounded contexts** con capas DDD (`CashitoBackend/`):

```
CashitoBackend/
├── Program.cs                          # Bootstrap, DI, pipeline HTTP
├── appsettings.json                    # Configuración (BD, JWT, logging)
├── IAM/                                # Bounded Context: Identidad y Acceso
│   ├── Domain/
│   │   ├── Model/
│   │   │   ├── Aggregates/             # User (agregado raíz)
│   │   │   ├── Entities/               # Role
│   │   │   ├── Commands/               # SignIn, SignUp, CreateUser, etc.
│   │   │   ├── Queries/                # GetUserById, GetAllUsers, GetAllRoles
│   │   │   └── ValueObjects/           # Roles (enum)
│   │   ├── Repositories/               # IUserRepository, IRoleRepository
│   │   └── Services/                   # IUserCommandService, IUserQueryService
│   ├── Application/
│   │   └── Internal/
│   │       ├── CommandServices/        # Lógica de escritura (CQRS)
│   │       ├── QueryServices/          # Lógica de lectura (CQRS)
│   │       ├── EventHandlers/          # SeedRolesHostedService
│   │       └── OutboundServices/       # ITokenService, IHashingService
│   ├── Infrastructure/
│   │   ├── Persistence/EFC/            # Repositorios EF Core
│   │   ├── Tokens/JWT/                 # Generación y validación de tokens
│   │   ├── Hashing/BCrypt/             # Servicio de hash de contraseñas
│   │   ├── Authorization/              # PassthroughAuthenticationHandler
│   │   └── Pipeline/Middleware/        # RequestAuthorizationMiddleware
│   └── Interfaces/
│       ├── REST/                       # Controllers, Resources, Transform
│       └── ACL/                        # Fachada anti-corrupción (IamContextFacade)
└── Shared/                             # Contexto compartido
    ├── Domain/                         # Excepciones, eventos, value objects base
    └── Infrastructure/                 # AppDbContext, UnitOfWork, middleware global
```

---

## Cómo ejecutar en local

### Requisitos previos

- [.NET 9 SDK](https://dotnet.microsoft.com/download)
- [MySQL 8+](https://dev.mysql.com/downloads/) en ejecución

1. **Clonar el repositorio:**

   ```bash
   git clone https://github.com/tuusuario/cashito-backend.git
   cd cashito-backend
   ```

2. **Crear la base de datos** en MySQL:

   ```sql
   CREATE DATABASE cashito CHARACTER SET utf8mb4 COLLATE utf8mb4_unicode_ci;
   ```

3. **Configurar la conexión** en `CashitoBackend/appsettings.json`:

   ```json
   {
     "TokenSettings": {
       "Secret": "TU_CLAVE_SECRETA_MINIMO_32_CARACTERES"
     },
     "ConnectionStrings": {
       "DefaultConnection": "server=localhost;port=3306;database=cashito;user=TU_USUARIO;password=TU_CONTRASEÑA"
     }
   }
   ```

   > La base de datos se crea automáticamente al iniciar la aplicación (`EnsureCreated`).

4. **Restaurar dependencias y ejecutar:**

   ```bash
   cd CashitoBackend
   dotnet restore
   dotnet run
   ```

5. **Puertos disponibles:**

   | Perfil  | URL                              |
   |---------|----------------------------------|
   | HTTP    | `http://localhost:5210`          |
   | HTTPS   | `https://localhost:7115`         |
   | Swagger | `http://localhost:5210/swagger`  |

---

## Endpoints principales

### Autenticación (`/api/v1/authentication`)

```
POST   /api/v1/authentication/sign-in     # Iniciar sesión (público)
POST   /api/v1/authentication/sign-up     # Registrar usuario (público)
```

### Usuarios (`/api/v1/users`)

```
GET    /api/v1/users                      # Listar todos los usuarios (autenticado)
GET    /api/v1/users/{id}                 # Obtener usuario por ID (autenticado)
POST   /api/v1/users                      # Crear usuario (público)
PUT    /api/v1/users/{id}                 # Actualizar usuario (rol Admin)
DELETE /api/v1/users/{id}                 # Eliminar usuario — desactivación lógica (rol Admin)
POST   /api/v1/users/{id}/change-password # Cambiar contraseña (autenticado)
```

### Roles (`/api/v1/roles`)

```
GET    /api/v1/roles                      # Listar roles del sistema (autenticado)
```

> Todos los endpoints protegidos requieren el header: `Authorization: Bearer {token}`

---

## Reglas de negocio importantes

- **Username único:** No se permite registrar dos usuarios con el mismo nombre de usuario.
- **Credenciales en sign-in:** Si el usuario no existe o la contraseña no coincide con el hash BCrypt almacenado, se rechaza el acceso con un mensaje genérico (`Invalid username or password`).
- **Rol por defecto en registro:** Al registrarse vía `sign-up`, el usuario recibe automáticamente el rol `User`.
- **Roles del sistema:** Solo existen dos roles válidos: `Admin` y `User`. Cualquier otro nombre de rol es rechazado.
- **Email válido:** El campo email se valida con expresión regular; un formato inválido lanza `Invalid e-mail format`.
- **Eliminación lógica:** Al eliminar un usuario, no se borra de la base de datos; se marca como inactivo (`Active = false`).
- **Cambio de contraseña:** Requiere la contraseña actual correcta antes de aplicar el nuevo hash.
- **Autorización por token:** Las rutas protegidas exigen un JWT válido en el header `Authorization`. Rutas anónimas (`sign-in`, `sign-up`, `POST /users`) están exentas.
- **Política AdminOnly:** Operaciones sensibles (actualizar/eliminar usuarios) requieren el rol `Admin`.
- **Auditoría:** Las entidades heredan de `AuditableAggregateRoot`, registrando fechas de creación y actualización automáticamente.

---

## Capturas de pantalla


### Swagger — Documentación API

![Swagger UI con endpoints de Cashito](docs/swagger.png)

### Swagger — Autenticación JWT

![Configuración Bearer token en Swagger](docs/swagger-auth.png)

---

## Proyecto relacionado

- [Cashito Frontend](https://github.com/tuusuario/cashito-frontend) — Interfaz Angular que consume esta API.

---

## Autor

**Victor Andres Cruz Ibarra** | **andrestheb@gmail.com** | **+51 960 938 630**

*Cashito API — Backend de gestión financiera personal con arquitectura DDD, autenticación JWT y documentación Swagger.*
