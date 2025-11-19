# Sistema de Gestión de Precios Dinámicos para Hoteles

Sistema integral de gestión de habitaciones y precios para cadenas hoteleras. Implementa algoritmos de pricing dinámico basados en ocupación, temporada y predicciones de ML.

## Características Principales

### Pricing Dinámico Inteligente
- Cálculo automático de precios basado en:
  - **Ocupación**: +35% (>85%), +25% (>75%), -20% (<50%)
  - **Temporada**: +40% Dic-Ene, +20% Jul-Ago
  - **Características**: +15% vista al mar, +10% balcón, +12% capacidad
  - **ML Prediction**: Demanda predicha 0-100

### Autenticación Multi-rol
- **Admins**: Gestión completa (configurable por hotel)
- **Staff**: Consulta de habitaciones y precios
- **Multi-tenant**: Cada usuario ve solo datos de su hotel

### Gestión de Hoteles
- Filtrado por hotel (admins regulares solo ven su hotel)
- Gestión de habitaciones por tipo
- Historial de precios para auditoría

### Inteligencia Artificial
- Modelo GradientBoosting entrenado
- Predicción de demanda en tiempo real
- Mejora continua con datos históricos

## Stack Tecnológico

| Componente | Tecnología | Versión |
|-----------|-----------|---------|
| Frontend | Angular | 20.3 |
| Backend | ASP.NET Core | 9.0 |
| Base Datos | SQL Server | 2019+ |
| ML Service | Python/FastAPI | 3.10+ |
| Autenticación | JWT | RFC 7519 |

## Inicio Rápido

### Requisitos Mínimos
- Windows 10+
- 4GB RAM
- 2GB espacio disponible

### Instalación Completa
Consulta [README_SETUP.md](./README_SETUP.md) para instrucciones **detalladas** de instalación incluyendo:
- Instalación de software necesario
- Configuración de base de datos
- Setup de cada servicio
- Troubleshooting

### Inicio Rápido (Desarrollo)

```powershell
# Terminal 1: Backend
cd backend\PricingMvp.Api
dotnet run

# Terminal 2: Frontend
cd frontend
npm install
ng serve

# Terminal 3: ML Service
cd ml_service
python -m venv venv
.\venv\Scripts\Activate.ps1
pip install -r requirements.txt
uvicorn main:app --reload --port 8000
```

**Acceso:**
- Frontend: http://localhost:4200
- Backend: http://localhost:5081
- ML: http://localhost:8000
- Swagger: http://localhost:5081/swagger

## Estructura del Proyecto

```
pricing-mvp/
├── backend/                    # .NET 9 Backend
│   ├── PricingMvp.Api/        # Controllers y endpoints
│   ├── PricingMvp.Application/ # DTOs y lógica de negocio
│   ├── PricingMvp.Domain/      # Entities y enums
│   └── PricingMvp.Infrastructure/ # EF Core y migraciones
│
├── frontend/                   # Angular 20.3
│   ├── src/app/
│   │   ├── components/        # Componentes de UI
│   │   ├── services/          # Servicios HTTP y lógica
│   │   └── models/            # Interfaces TypeScript
│   └── public/                # Archivos estáticos
│
├── ml_service/                # FastAPI Python
│   ├── main.py               # Servidor y modelos
│   ├── requirements.txt       # Dependencias
│   └── venv/                 # Entorno virtual
│
└── docs/                      # Documentación
```

## Conceptos Clave

### Multi-Tenant Architecture
- Cada usuario pertenece a un hotel
- Admins regulares solo ven datos de su hotel
- Solo `admin@pricingmvp.com` ve todo

### Roles de Sistema
```
Admin@pricingmvp.com → Ve/edita TODO (usuarios, hoteles, rooms)
Admin Regular        → Ve/edita solo su hotel y sus usuarios
Staff/Usuarios       → Consulta solo sus rooms
```

### Algoritmo de Pricing
```
PrecioSugerido = BasePrice × OccupancyMultiplier × SeasonMultiplier + Characteristics
```

## API Endpoints

### Autenticación
```http
POST /api/auth/login           # Login usuario
POST /api/auth/register        # Registro nuevo usuario
```

### Usuarios (Admin)
```http
GET /api/users                 # Listar usuarios (filtrado por hotel)
PATCH /api/users/{id}/hotel    # Cambiar hotel de usuario
```

### Habitaciones
```http
GET /api/rooms                 # Listar rooms (filtrado por hotel)
GET /api/rooms/{id}            # Obtener room específica
POST /api/rooms                # Crear room (Admin)
PUT /api/rooms/{id}            # Editar room (Admin)
DELETE /api/rooms/{id}         # Eliminar room (Solo admin@pricingmvp.com)
```

### Pricing
```http
GET /api/pricing/suggest/{id}  # Obtener precio sugerido
POST /api/pricing/apply        # Aplicar precio sugerido
```

### Hotels
```http
GET /api/hotels                # Listar hoteles (público)
POST /api/hotels               # Crear hotel (Admin)
DELETE /api/hotels/{id}        # Eliminar hotel (Solo admin@pricingmvp.com)
```

## Modelo de Datos

### Users
```
Id | Email | PasswordHash | FullName | Role | HotelId | IsActive | CreatedAt
```

### Hotels
```
Id | Name | City | State | Address | Stars | Description | CreatedAt
```

### Rooms
```
Id | HotelId | RoomNumber | Type | BasePrice | Capacity | HasBalcony | HasSeaView | IsDeleted
```

### PriceHistory
```
Id | RoomId | SuggestedPrice | Reason | CreatedAt
```

## Seguridad

- **JWT Tokens**: 24 horas expiración (configurable en appsettings.json)
- **Contraseñas**: BCrypt hashing
- **CORS**: Configurado para desarrollo
- **Dominios**: Solo @posadas.com permitidos en registro
- **Roles**: Validación en cada endpoint

## Testing

### Manual (Swagger)
1. Ir a http://localhost:5081/swagger
2. Click en "Authorize"
3. Login primero
4. Usar endpoints

### Automatizado
```powershell
# Backend tests
cd backend
dotnet test

# Frontend tests  
cd frontend
npm test
```



