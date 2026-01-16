# Arquitectura del Sistema

## Visión General

Reports Services está diseñado siguiendo los principios de **Clean Architecture**, asegurando la separación de responsabilidades y manteniendo el código desacoplado y testeable.

## Diagrama de Capas

```
┌─────────────────────────────────────────────────────────┐
│                    Presentation Layer                    │
│                  (reports_services.api)                  │
│                                                           │
│  ┌──────────────┐  ┌────────────┐  ┌─────────────────┐ │
│  │ Controllers  │  │  Program   │  │  Middleware     │ │
│  └──────────────┘  └────────────┘  └─────────────────┘ │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                   Application Layer                      │
│              (reports_services.application)              │
│                                                           │
│  ┌──────────┐  ┌──────────┐  ┌──────────┐  ┌────────┐ │
│  │ Queries  │  │ Handlers │  │   DTOs   │  │ Interf.│ │
│  └──────────┘  └──────────┘  └──────────┘  └────────┘ │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                     Domain Layer                         │
│                (reports_services.domain)                 │
│                                                           │
│            ┌──────────────────────────────┐             │
│            │   Business Logic & Entities  │             │
│            └──────────────────────────────┘             │
└───────────────────────────┬─────────────────────────────┘
                            │
┌───────────────────────────▼─────────────────────────────┐
│                 Infrastructure Layer                     │
│            (reports_services.infrastructure)             │
│                                                           │
│  ┌─────────────────┐       ┌──────────────────┐        │
│  │ Payment Service │       │  Survey Service  │        │
│  │  (HTTP Client)  │       │   (HTTP Client)  │        │
│  └─────────────────┘       └──────────────────┘        │
└─────────────────────────────────────────────────────────┘
```

## Descripción de Capas

### 1. Presentation Layer (API)

**Responsabilidad**: Exponer endpoints HTTP y manejar las peticiones de los clientes.

**Componentes**:
- `ReportsController`: Controlador REST que expone los endpoints de reportes
- `Program.cs`: Configuración de la aplicación, DI, middleware
- Configuración de Swagger para documentación de API
- Configuración de CORS

**Dependencias**:
- Application Layer (Queries, Handlers, DTOs)
- ASP.NET Core
- MediatR

### 2. Application Layer

**Responsabilidad**: Coordinar la lógica de aplicación y orquestar el flujo de datos.

**Componentes**:

#### Queries (CQRS)
- `GetReportePagosPorEventoQuery`: Query para obtener reportes de pagos
- `GetPromedioEncuestaPorEventoQuery`: Query para obtener estadísticas de encuestas

#### Handlers
- `GetReportePagosPorEventoHandler`: Procesa la query de pagos
- `GetPromedioEncuestaPorEventoHandler`: Procesa la query de encuestas

#### DTOs (Data Transfer Objects)
- `DetallePagoReporteDto`: Información detallada de pagos
- `HistorialPagoExternoDto`: Historial de pagos externos
- `PromedioEventSurveyDto`: Promedios de encuestas por evento
- `PromedioQuestionDto`: Promedios por pregunta
- `ReportePagosPorDiaDto`: Reportes agrupados por día

#### Interfaces
- `IPaymentService`: Contrato para el servicio de pagos
- `ISurveyService`: Contrato para el servicio de encuestas

**Dependencias**:
- Domain Layer
- MediatR para implementación de CQRS

### 3. Domain Layer

**Responsabilidad**: Contener las entidades del dominio y la lógica de negocio pura.

**Características**:
- Sin dependencias externas
- Entidades del dominio
- Lógica de negocio core
- Reglas de validación del dominio

**Principio**: Esta capa no debe depender de ninguna otra capa.

### 4. Infrastructure Layer

**Responsabilidad**: Implementar los detalles técnicos de comunicación con servicios externos.

**Componentes**:

#### PaymentService
```csharp
public class PaymentService : IPaymentService
{
    // Implementa comunicación HTTP con el servicio de pagos
    // Endpoint: http://localhost:7183
}
```

#### SurveyService
```csharp
public class SurveyService : ISurveyService
{
    // Implementa comunicación HTTP con el servicio de encuestas
    // Endpoint: http://localhost:7186
}
```

**Dependencias**:
- HttpClient para comunicación REST
- System.Text.Json para serialización

## Patrones de Diseño

### 1. CQRS (Command Query Responsibility Segregation)

Separamos las operaciones de lectura (Queries) de las operaciones de escritura (Commands). En este microservicio, solo tenemos Queries ya que es un servicio de reportes de solo lectura.

**Implementación**:
```csharp
// Query
public record GetReportePagosPorEventoQuery(Guid IdEvento) : IRequest<List<DetallePagoReporteDto>>;

// Handler
public class GetReportePagosPorEventoHandler : IRequestHandler<GetReportePagosPorEventoQuery, List<DetallePagoReporteDto>>
{
    // Lógica de procesamiento
}
```

**Beneficios**:
- Separación clara de responsabilidades
- Fácil de testear
- Escalable independientemente

### 2. Mediator Pattern

Usamos MediatR para desacoplar los Controllers de los Handlers.

**Flujo**:
```
Controller → MediatR → Handler → Service → External API
```

**Ventajas**:
- Reduce el acoplamiento
- Facilita la adición de pipelines (logging, validación, etc.)
- Mejora la testeabilidad

### 3. Dependency Injection

Todas las dependencias se inyectan a través del contenedor de DI de ASP.NET Core.

**Configuración en Program.cs**:
```csharp
builder.Services.AddHttpClient<IPaymentService, PaymentService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:7183/");
});

builder.Services.AddMediatR(cfg => 
    cfg.RegisterServicesFromAssembly(typeof(GetReportePagosPorEventoHandler).Assembly));
```

### 4. Repository Pattern

Aunque no usamos una base de datos directa, los servicios (`PaymentService`, `SurveyService`) actúan como repositorios que abstraen el acceso a datos externos.

## Flujo de Datos

### Ejemplo: Obtener Reporte de Pagos

```
1. Cliente HTTP
   └─> GET /api/reports/pagosPorEvento/{idEvento}

2. ReportsController
   └─> Valida el GUID
   └─> Crea GetReportePagosPorEventoQuery
   └─> Envía query a MediatR

3. MediatR
   └─> Encuentra GetReportePagosPorEventoHandler
   └─> Ejecuta el handler

4. GetReportePagosPorEventoHandler
   └─> Inyecta IPaymentService
   └─> Llama ObtenerHistorialPorEventoAsync()

5. PaymentService
   └─> HTTP GET a http://localhost:7183/api/payments/historial/{idEvento}
   └─> Deserializa respuesta JSON
   └─> Retorna List<HistorialPagoExternoDto>

6. Handler procesa los datos
   └─> Puede aplicar transformaciones
   └─> Retorna List<DetallePagoReporteDto>

7. Controller
   └─> Retorna Ok(result) con status 200
```

## Comunicación entre Microservicios

### Arquitectura de Microservicios

```
┌─────────────────┐       HTTP       ┌─────────────────┐
│                 │◄─────────────────┤                 │
│ Payment Service │                  │ Reports Service │
│   (Port 7183)   │──────────────────►│   (Port 7187)   │
└─────────────────┘       JSON       └─────────────────┘
                                             │
                                             │ HTTP
                                             │ JSON
                                             ▼
                                      ┌─────────────────┐
                                      │                 │
                                      │ Survey Service  │
                                      │   (Port 7186)   │
                                      └─────────────────┘
```

### Protocolo de Comunicación

- **Protocolo**: HTTP/HTTPS
- **Formato**: JSON
- **Cliente**: HttpClient con configuración de retry y timeout
- **Serialización**: System.Text.Json con `PropertyNameCaseInsensitive = true`

### Manejo de Errores

```csharp
if (!response.IsSuccessStatusCode)
{
    if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
    {
        return new List<HistorialPagoExternoDto>(); // Lista vacía
    }
    
    throw new Exception($"Error al consultar Pagos: {response.StatusCode}");
}
```

## Principios SOLID Aplicados

### Single Responsibility Principle (SRP)
Cada clase tiene una única responsabilidad:
- Controllers: manejar HTTP requests
- Handlers: lógica de aplicación
- Services: comunicación externa

### Open/Closed Principle (OCP)
El sistema está abierto para extensión pero cerrado para modificación:
- Nuevos handlers pueden agregarse sin modificar código existente
- Nuevos servicios pueden implementar las interfaces existentes

### Liskov Substitution Principle (LSP)
Las implementaciones pueden sustituirse por sus interfaces:
- `IPaymentService` puede tener múltiples implementaciones
- Útil para testing con mocks

### Interface Segregation Principle (ISP)
Interfaces pequeñas y específicas:
- `IPaymentService`: solo métodos de pagos
- `ISurveyService`: solo métodos de encuestas

### Dependency Inversion Principle (DIP)
Dependemos de abstracciones, no de implementaciones concretas:
- Handlers dependen de `IPaymentService`, no de `PaymentService`
- Facilita el testing y el cambio de implementaciones

## Estrategia de Testing

### Unit Tests

Cada capa tiene sus propios tests:

```
tests/
├── reports_services.api.Tests
│   └── Controllers/
│       └── ReportsControllerTest.cs
├── reports_services.application.Tests
│   ├── Queries/
│   └── Handler/
└── reports_services.infrastructure.Tests
    └── Services/
```

### Mocking

Usamos **Moq** para crear mocks de las dependencias:

```csharp
var mockPaymentService = new Mock<IPaymentService>();
mockPaymentService
    .Setup(x => x.ObtenerHistorialPorEventoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
    .ReturnsAsync(expectedData);
```

### Cobertura

Se busca mantener una cobertura de tests > 80% en:
- Handlers de aplicación
- Controllers
- Servicios de infraestructura

## Consideraciones de Seguridad

### CORS
Configurado para permitir solo orígenes específicos:
```csharp
policy.WithOrigins("http://localhost:3000")
```

### Validación de Entrada
- Validación de GUIDs en endpoints
- Manejo de excepciones centralizado
- Retorno de mensajes de error apropiados sin exponer detalles internos

### Comunicación entre Servicios
- TODO: Implementar autenticación entre microservicios
- TODO: Agregar circuit breaker pattern para resiliencia
- TODO: Implementar rate limiting

## Escalabilidad

### Consideraciones
- El servicio es **stateless**, facilitando escalado horizontal
- Puede ejecutarse en múltiples instancias detrás de un load balancer
- La comunicación HTTP con otros servicios puede beneficiarse de:
  - Connection pooling (ya implementado con HttpClient)
  - Caching de respuestas
  - Circuit breakers para resiliencia

### Recomendaciones Futuras
- Implementar Redis para caching de reportes
- Agregar message queues para reportes asíncronos pesados
- Considerar GraphQL para queries más flexibles

## Monitoreo y Observabilidad

### Logging
- Configurado con `ILogger` de ASP.NET Core
- Niveles: Information, Warning, Error
- TODO: Integrar con sistemas como ELK o Application Insights

### Health Checks
- TODO: Implementar endpoints de health check
- TODO: Verificar conectividad con servicios dependientes

### Métricas
- TODO: Agregar métricas de negocio (reportes generados, tiempo de respuesta)
- TODO: Integrar con Prometheus/Grafana

## Referencias

- [Clean Architecture - Robert C. Martin](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [MediatR Pattern](https://github.com/jbogard/MediatR)
- [CQRS Pattern](https://martinfowler.com/bliki/CQRS.html)
- [Microservices Patterns](https://microservices.io/patterns/index.html)
