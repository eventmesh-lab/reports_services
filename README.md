# Reports Services

Microservicio de generación de reportes para el sistema EventMesh. Este servicio se encarga de consolidar información de pagos y encuestas para generar reportes analíticos sobre eventos.

## 📋 Tabla de Contenidos

- [Descripción](#descripción)
- [Arquitectura](#arquitectura)
- [Requisitos Previos](#requisitos-previos)
- [Instalación](#instalación)
- [Configuración](#configuración)
- [Ejecución](#ejecución)
- [API Endpoints](#api-endpoints)
- [Testing](#testing)
- [Docker](#docker)
- [Tecnologías](#tecnologías)

## 🎯 Descripción

Reports Services es un microservicio diseñado para generar reportes analíticos consolidando información de otros microservicios del ecosistema EventMesh. Proporciona dos tipos principales de reportes:

1. **Reportes de Pagos por Evento**: Consolida y presenta el historial de pagos asociados a un evento específico
2. **Reportes de Encuestas por Evento**: Calcula promedios y estadísticas de las encuestas realizadas para un evento

## 🏗️ Arquitectura

El proyecto sigue los principios de **Clean Architecture** dividido en las siguientes capas:

```
reports_services/
├── reports_services.api/           # Capa de presentación (Controllers, API)
├── reports_services.application/   # Lógica de aplicación (Queries, Handlers, DTOs)
├── reports_services.domain/        # Entidades del dominio y lógica de negocio
├── reports_services.infrastructure/# Servicios externos e infraestructura
└── tests/                          # Pruebas unitarias para todas las capas
```

### Patrones Implementados

- **CQRS** (Command Query Responsibility Segregation) con MediatR
- **Clean Architecture** para separación de responsabilidades
- **Dependency Injection** para inversión de control
- **Repository Pattern** a través de servicios HTTP

## 🔧 Requisitos Previos

- [.NET 8.0 SDK](https://dotnet.microsoft.com/download/dotnet/8.0) o superior
- [Docker](https://www.docker.com/) (opcional, para containerización)
- Visual Studio 2022, Visual Studio Code o Rider (recomendado para desarrollo)

### Servicios Dependientes

Este microservicio requiere los siguientes servicios externos en ejecución:

- **Payment Service**: `http://localhost:7183` - Servicio de gestión de pagos
- **Survey Service**: `http://localhost:7186` - Servicio de gestión de encuestas

## 📦 Instalación

1. **Clonar el repositorio**:
```bash
git clone https://github.com/eventmesh-lab/reports_services.git
cd reports_services
```

2. **Restaurar dependencias**:
```bash
dotnet restore
```

3. **Compilar la solución**:
```bash
dotnet build
```

## ⚙️ Configuración

### Variables de Entorno

El servicio puede configurarse mediante las siguientes variables de entorno:

- `ASPNETCORE_URLS`: URL donde el servicio escuchará (por defecto: `http://*:7187`)
- `APP_PORT`: Puerto de la aplicación (por defecto: `7187`)

### appsettings.json

Puedes modificar la configuración en `reports_services.api/appsettings.json`:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Information",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "AllowedHosts": "*"
}
```

Para modificar las URLs de los servicios dependientes, edita `Program.cs`:

```csharp
builder.Services.AddHttpClient<IPaymentService, PaymentService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7183/");
});

builder.Services.AddHttpClient<ISurveyService, SurveyService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7186/");
});
```

## 🚀 Ejecución

### Modo Desarrollo

```bash
cd reports_services.api
dotnet run
```

El servicio estará disponible en:
- HTTP: `http://localhost:7187`
- Swagger UI: `http://localhost:7187/swagger`

### Modo Producción

```bash
dotnet run --configuration Release
```

## 📚 API Endpoints

### Swagger Documentation

La documentación interactiva de la API está disponible en: `http://localhost:7187/swagger`

### Endpoints Principales

#### 1. Obtener Reporte de Pagos por Evento

```http
GET /api/reports/pagosPorEvento/{idEvento}
```

**Descripción**: Obtiene el historial consolidado de pagos asociados a un evento específico.

**Parámetros**:
- `idEvento` (GUID): Identificador único del evento

**Respuesta exitosa (200)**:
```json
[
  {
    "idPago": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
    "monto": 100.00,
    "fechaPago": "2024-01-15T10:30:00Z",
    "metodoPago": "Tarjeta de Crédito",
    "estado": "Completado"
  }
]
```

**Errores**:
- `400 Bad Request`: ID de evento inválido o vacío
- `500 Internal Server Error`: Error al consultar el servicio de pagos

#### 2. Obtener Reporte de Encuestas por Evento

```http
GET /api/reports/promedioPorEvento/{eventId}
```

**Descripción**: Calcula el promedio de las respuestas de encuestas asociadas a un evento.

**Parámetros**:
- `eventId` (GUID): Identificador único del evento

**Respuesta exitosa (200)**:
```json
{
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nombreEvento": "Conferencia Tech 2024",
  "promedioGeneral": 4.5,
  "totalEncuestas": 150,
  "preguntasPromedio": [
    {
      "preguntaId": "uuid",
      "textoPregunta": "¿Cómo califica el evento?",
      "promedio": 4.8
    }
  ]
}
```

**Errores**:
- `400 Bad Request`: ID de evento inválido
- `404 Not Found`: No se encontraron encuestas para el evento
- `500 Internal Server Error`: Error al consultar el servicio de encuestas

### CORS

El servicio está configurado para aceptar peticiones desde:
- `http://localhost:3000` (frontend de desarrollo)

## 🧪 Testing

El proyecto incluye pruebas unitarias completas para todas las capas.

### Ejecutar todos los tests

```bash
dotnet test
```

### Ejecutar tests con cobertura

```bash
dotnet test /p:CollectCoverage=true /p:CoverageReportsFormat=lcov
```

### Ejecutar tests de una capa específica

```bash
# Tests de API
dotnet test tests/reports_services.api.Tests

# Tests de Application
dotnet test tests/reports_services.application.Tests

# Tests de Infrastructure
dotnet test tests/reports_services.infrastructure.Tests
```

### Estructura de Tests

```
tests/
├── reports_services.api.Tests/
│   └── Controllers/
│       └── ReportsControllerTest.cs
├── reports_services.application.Tests/
│   ├── Queries/
│   └── Handler/
└── reports_services.infrastructure.Tests/
    └── Services/
```

## 🐳 Docker

### Construir la imagen

```bash
docker build -t reports-services:latest .
```

### Ejecutar el contenedor

```bash
docker run -d -p 7187:7187 --name reports-services reports-services:latest
```

### Ejecutar con variables de entorno personalizadas

```bash
docker run -d \
  -p 8080:8080 \
  -e APP_PORT=8080 \
  --name reports-services \
  reports-services:latest
```

### Docker Compose (ejemplo)

```yaml
version: '3.8'
services:
  reports-services:
    build: .
    ports:
      - "7187:7187"
    environment:
      - ASPNETCORE_ENVIRONMENT=Production
      - APP_PORT=7187
    depends_on:
      - payment-service
      - survey-service
```

## 🛠️ Tecnologías

- **.NET 8.0**: Framework principal
- **ASP.NET Core**: Web API framework
- **MediatR**: Implementación de CQRS y mediator pattern
- **Swashbuckle (Swagger)**: Documentación de API
- **xUnit**: Framework de testing
- **Moq**: Library para mocking en tests
- **System.Text.Json**: Serialización/deserialización JSON
- **HttpClient**: Cliente HTTP para comunicación con microservicios

## 📝 Estructura del Proyecto

### reports_services.api
Capa de presentación que contiene:
- Controllers (ReportsController)
- Configuración de Swagger
- Configuración de CORS
- Punto de entrada de la aplicación (Program.cs)

### reports_services.application
Lógica de aplicación que incluye:
- **DTOs**: Data Transfer Objects
- **Queries**: Consultas CQRS
- **Handlers**: Manejadores de queries con MediatR
- **Interfaces**: Contratos para servicios

### reports_services.domain
Entidades del dominio y lógica de negocio central.

### reports_services.infrastructure
Implementaciones de servicios externos:
- **PaymentService**: Cliente HTTP para el servicio de pagos
- **SurveyService**: Cliente HTTP para el servicio de encuestas

## 🤝 Contribución

1. Fork el proyecto
2. Crea una rama para tu feature (`git checkout -b feature/AmazingFeature`)
3. Commit tus cambios (`git commit -m 'Add some AmazingFeature'`)
4. Push a la rama (`git push origin feature/AmazingFeature`)
5. Abre un Pull Request

## 📄 Licencia

Este proyecto es parte del ecosistema EventMesh Lab.

## 📧 Contacto

Para preguntas o soporte, por favor contacta al equipo de EventMesh Lab.

---

**Nota**: Asegúrate de que los servicios dependientes (Payment Service y Survey Service) estén ejecutándose antes de iniciar este microservicio.
