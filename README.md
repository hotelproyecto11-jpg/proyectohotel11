# PricingMVP

Proyecto MVP para un sistema de pricing de habitaciones.

Resumen rápido
- Backend API (ASP.NET Core): funcional y arrancable.
- Base de datos: EF Core con migraciones; las migraciones se aplican al iniciar y hay un seeder automático.
- Autenticación: login con email/contraseña y generación de JWT (BCrypt para verificar contraseña).
- Control de acceso por roles: protecciones y endpoints solo para `Admin` o `RevenueManager` donde corresponde.
- Funcionalidades implementadas en backend:
  - Login (generación de token JWT).
  - Endpoints de habitaciones: listar, detalle, crear, editar, soft-delete.
  - Endpoints de pricing: obtener precio sugerido y aplicar precio (con roles).
  - Swagger habilitado en Development.

- Frontend (Angular): proyecto configurado y listo para ejecutar.
  - Dependencias relevantes instalables (`@auth0/angular-jwt`, `bootstrap`, `chart.js`).
  - Estructura preparada para añadir `models`, `services`, `guards` e `interceptors`.

Ejecutar localmente (mínimos)

- Backend (desde `backend/PricingMvp.Api`):
  1. Configurar `ConnectionStrings:DefaultConnection` y `Jwt:Key` (appsettings o variables de entorno).
  2. `dotnet run` — la aplicación aplicará migraciones y ejecutará el seeder en Development.

- Frontend (desde `frontend`):
  1. `npm install`
  2. `npm start` (o `ng serve`) — dev server en `http://localhost:4200`.

Notas importantes
- Asegúrate de no commitear secretos (usar user-secrets o variables de entorno para `Jwt:Key` y la cadena de conexión).
- CORS ya permite `http://localhost:4200` para facilitar desarrollo conjunto frontend/backend.


