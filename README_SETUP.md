# Hotel Management System - Guía Completa de Instalación

##  Requisitos Previos - TODO lo que necesitas instalar

### 1. **Node.js y npm** (Para Angular Frontend)
- **Descargar**: https://nodejs.org/ (Versión LTS recomendada: 18.x o superior)
- **Verificar instalación**:
  ```powershell
  node --version
  npm --version
  ```
- **Instalar Angular CLI globalmente** (después de instalar Node):
  ```powershell
  npm install -g @angular/cli
  ```

### 2. **.NET 9 SDK** (Para ASP.NET Core Backend)
- **Descargar**: https://dotnet.microsoft.com/download
- **Seleccionar**: .NET 9 SDK
- **Verificar instalación**:
  ```powershell
  dotnet --version
  ```

### 3. **SQL Server** (Base de Datos)
- **Opción - SQL Server Express** (Gratuito):
  - Descargar: https://www.microsoft.com/en-us/sql-server/sql-server-downloads
  - Seleccionar: SQL Server 2022 Express
  - **IMPORTANTE**: Durante la instalación, establece contraseña para `sa` (administrador)
  
- **Verificar instalación**:
  ```powershell
  sqlcmd -S localhost -U sa -P <tu_contraseña>
  1> SELECT @@version
  2> GO
  ```

### 4. **SQL Server Management Studio (SSMS)** - Herramienta visual para SQL Server
- **Descargar**: https://learn.microsoft.com/en-us/sql/ssms/download-sql-server-management-studio-ssms
- **Usar para**: Ver y administrar bases de datos gráficamente

### 5. **Python 3.10+** (Para ML Service)
- **Descargar**: https://www.python.org/downloads/
- **IMPORTANTE**: Durante instalación, marcar "Add Python to PATH"
- **Verificar instalación**:
  ```powershell
  python --version
  pip --version
  ```

### 6. **Git** (Control de versiones)
- **Descargar**: https://git-scm.com/download/win
- **Verificar instalación**:
  ```powershell
  git --version
  ```

### 7. **Visual Studio Code** (Editor recomendado)
- **Descargar**: https://code.visualstudio.com/
- **Extensiones recomendadas**:
  - C# (ms-dotnettools.csharp)
  - SQL Server (ms-mssql.mssql)
  - Python (ms-python.python)
  - Angular Language Service (Angular.ng-template)

---

##  Instalación del Proyecto

### Paso 1: Clonar el repositorio

```powershell
cd C:\Users\<tu_usuario>\
git clone https://github.com/hotelproyecto11-jpg/proyectohotel11.git
cd proyectohotel11
```

### Paso 2: Configurar SQL Server

#### Opción A - Connection String local (Trusted Connection)
Si usas Windows Authentication y SQL Server en localhost:

```
Server=localhost;Database=PricingMvpDb;Trusted_Connection=True;TrustServerCertificate=True;
```

#### Opción B - Connection String con usuario/contraseña
Si usas usuario `sa`:

```
Server=localhost;Database=PricingMvpDb;User Id=sa;Password=<tu_contraseña>;TrustServerCertificate=True;
```

**Ubicación del archivo**: `backend/PricingMvp.Api/appsettings.json`

```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=localhost;Database=PricingMvpDb;Trusted_Connection=True;TrustServerCertificate=True;"
  }
}
```

### Paso 3: Backend Setup (.NET)

```powershell
cd backend\PricingMvp.Api

# Restaurar dependencias
dotnet restore

# Aplicar migraciones (crea base de datos automáticamente)
dotnet ef database update --project ../PricingMvp.Infrastructure

# Ejecutar backend
dotnet run
```

**El backend estará disponible en**: `http://localhost:5081`

**Swagger (documentación de APIs)**: `http://localhost:5081/swagger`

### Paso 4: Frontend Setup (Angular)

```powershell
cd frontend

# Instalar dependencias
npm install

# Ejecutar Angular dev server
ng serve
```

**El frontend estará disponible en**: `http://localhost:4200`

### Paso 5: ML Service Setup (Python)

```powershell
cd ml_service

# Crear entorno virtual
python -m venv venv

# Activar entorno virtual
.\venv\Scripts\Activate.ps1

# Instalar dependencias
pip install -r requirements.txt

# Ejecutar servidor ML
uvicorn main:app --reload --port 8000
```

**El servicio ML estará disponible en**: `http://localhost:8000`

---

## Verificar que todo funciona

### Desde el navegador:

1. **Frontend**: Abre `http://localhost:4200`
   - Deberías ver la pantalla de login

2. **Backend Swagger**: Abre `http://localhost:5081/swagger`
   - Deberías ver documentación interactiva de APIs

3. **ML Service**: Abre `http://localhost:8000/docs`
   - Deberías ver documentación de FastAPI

### Desde PowerShell:

```powershell
# Verificar backend
Invoke-RestMethod -Uri "http://localhost:5081/api/hotels" -Method Get

# Verificar ML
Invoke-RestMethod -Uri "http://localhost:8000/docs" -Method Get
```

---

## Crear Usuario de Prueba

### Via Swagger (http://localhost:5081/swagger)

1. Busca `POST /api/auth/register`
2. Click en "Try it out"
3. Ingresa:
```json
{
  "email": "usuario@posadas.com",
  "password": "Password123!",
  "fullName": "Usuario Prueba",
  "hotelId": 1
}
```

### Credenciales por defecto (admin@pricingmvp.com):

**Email**: `admin@pricingmvp.com`
**Contraseña**: Busca en `DataSeeder.cs` (backend) Esta es la **Contraseña** Admin123!

O crea un admin nuevo via SQL:

```sql
USE PricingMvpDb;

INSERT INTO [dbo].[Users] 
(Email, PasswordHash, FullName, Role, IsActive, HotelId, CreatedAt)
VALUES 
('admin@pricingmvp.com', '$2a$11$...', 'Admin Principal', 0, 1, 1, GETDATE())
```

---

## Estructura de Carpetas

```
pricing-mvp/
├── backend/                          # ASP.NET Core 9 Backend
│   ├── PricingMvp.Api/              # API Controllers
│   │   ├── Controllers/
│   │   ├── Program.cs               # Configuración principal
│   │   └── appsettings.json         # Config (Connection String, JWT)
│   ├── PricingMvp.Application/      # DTOs, Interfaces, Business Logic
│   ├── PricingMvp.Domain/           # Entities, Enums
│   ├── PricingMvp.Infrastructure/   # Database, Migrations
│   └── PricingMvp.sln               # Solución .NET
│
├── frontend/                         # Angular 20.3 Frontend
│   ├── src/
│   │   ├── app/
│   │   │   ├── components/          # Componentes
│   │   │   ├── services/            # Servicios
│   │   │   └── app.routes.ts        # Rutas
│   │   └── main.ts                  # Entry point
│   ├── angular.json                 # Config Angular
│   └── package.json                 # Dependencias Node
│
├── ml_service/                      # Python FastAPI ML
│   ├── main.py                      # Servidor FastAPI
│   ├── requirements.txt              # Dependencias Python
│   └── venv/                        # Entorno virtual
│
└── docs/                            # Documentación
```

---

## 🔧 Troubleshooting

### Error: "Connection to localhost failed"
- **Solución**: Verifica que SQL Server está corriendo
- En Windows, abre Services (services.msc) y busca "SQL Server"

### Error: "Port 4200 already in use"
- **Solución**: Cambia el puerto
  ```powershell
  ng serve --port 4201
  ```

### Error: "dotnet: command not found"
- **Solución**: Reinstala .NET SDK y asegúrate de que está en PATH

### Error: "Python: module not found"
- **Solución**: Asegúrate de activar el entorno virtual
  ```powershell
  .\venv\Scripts\Activate.ps1
  pip install -r requirements.txt
  ```

### Error: "npm: command not found"
- **Solución**: Reinstala Node.js (incluye npm)

---

## Archivos Clave a Conocer

| Archivo | Función |
|---------|---------|
| `appsettings.json` | Connection String, JWT config |
| `Program.cs` | Configuración del servidor .NET |
| `package.json` | Dependencias de Angular |
| `requirements.txt` | Dependencias de Python |
| `launchSettings.json` | Puertos y perfiles de ejecución |

---




