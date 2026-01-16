# Guía de Desarrollo

Esta guía proporciona información para desarrolladores que quieren contribuir o trabajar en el proyecto Reports Services.

## Tabla de Contenidos

- [Configuración del Entorno](#configuración-del-entorno)
- [Estructura del Proyecto](#estructura-del-proyecto)
- [Flujo de Trabajo de Desarrollo](#flujo-de-trabajo-de-desarrollo)
- [Estándares de Código](#estándares-de-código)
- [Testing](#testing)
- [Debugging](#debugging)
- [Buenas Prácticas](#buenas-prácticas)

## Configuración del Entorno

### Requisitos

1. **.NET 8.0 SDK o superior**
   ```bash
   dotnet --version
   # Debería mostrar 8.0.x o superior
   ```

2. **IDE recomendado** (uno de los siguientes):
   - Visual Studio 2022 (Community, Professional o Enterprise)
   - Visual Studio Code con extensión C#
   - JetBrains Rider

3. **Git**
   ```bash
   git --version
   ```

### Configuración Inicial

1. **Clonar el repositorio**:
   ```bash
   git clone https://github.com/eventmesh-lab/reports_services.git
   cd reports_services
   ```

2. **Restaurar paquetes NuGet**:
   ```bash
   dotnet restore
   ```

3. **Compilar el proyecto**:
   ```bash
   dotnet build
   ```

4. **Verificar que los tests pasen**:
   ```bash
   dotnet test
   ```

### Configuración de Servicios Dependientes

Este microservicio requiere dos servicios externos:

#### Payment Service (Port 7183)
```bash
# Clonar y ejecutar el servicio de pagos
git clone https://github.com/eventmesh-lab/payment_service.git
cd payment_service
dotnet run
```

#### Survey Service (Port 7186)
```bash
# Clonar y ejecutar el servicio de encuestas
git clone https://github.com/eventmesh-lab/survey_service.git
cd survey_service
dotnet run
```

### Variables de Entorno para Desarrollo

Crear un archivo `appsettings.Development.json` si necesitas configuraciones locales:

```json
{
  "Logging": {
    "LogLevel": {
      "Default": "Debug",
      "Microsoft.AspNetCore": "Warning"
    }
  },
  "ServiceUrls": {
    "PaymentService": "http://localhost:7183",
    "SurveyService": "http://localhost:7186"
  }
}
```

## Estructura del Proyecto

```
reports_services/
├── reports_services.api/              # API Layer
│   ├── Controllers/
│   │   └── ReportsController.cs       # Endpoints REST
│   ├── Program.cs                     # Configuración y startup
│   ├── appsettings.json               # Configuración
│   └── appsettings.Development.json   # Configuración de desarrollo
│
├── reports_services.application/      # Application Layer
│   ├── DTOs/                          # Data Transfer Objects
│   │   ├── DetallePagoReporteDto.cs
│   │   ├── HistorialPagoExternoDto.cs
│   │   ├── PromedioEventSurveyDto.cs
│   │   └── ...
│   ├── Interfaces/                    # Contratos de servicios
│   │   ├── IPaymentService.cs
│   │   └── ISurveyService.cs
│   └── Queries/                       # CQRS Queries
│       ├── Queries/                   # Query definitions
│       └── Handler/                   # Query handlers
│
├── reports_services.domain/           # Domain Layer
│   └── (Entidades de dominio)
│
├── reports_services.infrastructure/   # Infrastructure Layer
│   └── Services/
│       ├── PaymentService.cs          # Cliente HTTP para pagos
│       └── SurveyService.cs           # Cliente HTTP para encuestas
│
├── tests/                             # Tests
│   ├── reports_services.api.Tests/
│   ├── reports_services.application.Tests/
│   └── reports_services.infrastructure.Tests/
│
├── docs/                              # Documentación
├── Dockerfile                         # Configuración de Docker
└── reports_services.sln               # Solución de Visual Studio
```

## Flujo de Trabajo de Desarrollo

### 1. Crear una Nueva Feature

```bash
# Crear y cambiar a una nueva rama
git checkout -b feature/nombre-de-tu-feature

# Hacer cambios...

# Compilar y verificar
dotnet build

# Ejecutar tests
dotnet test

# Commit
git add .
git commit -m "feat: descripción de tu feature"

# Push
git push origin feature/nombre-de-tu-feature
```

### 2. Agregar un Nuevo Endpoint

#### Paso 1: Crear el DTO (Application Layer)

```csharp
// reports_services.application/DTOs/NuevoReporteDto.cs
namespace reports_services.application.DTOs
{
    public class NuevoReporteDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; }
        // ... otros campos
    }
}
```

#### Paso 2: Crear la Query (Application Layer)

```csharp
// reports_services.application/Queries/Queries/GetNuevoReporteQuery.cs
using MediatR;
using reports_services.application.DTOs;

namespace reports_services.application.Queries.Queries
{
    public record GetNuevoReporteQuery(Guid Id) : IRequest<NuevoReporteDto>;
}
```

#### Paso 3: Crear el Handler (Application Layer)

```csharp
// reports_services.application/Queries/Handler/GetNuevoReporteHandler.cs
using MediatR;
using reports_services.application.DTOs;
using reports_services.application.Interfaces;
using reports_services.application.Queries.Queries;

namespace reports_services.application.Queries.Handler
{
    public class GetNuevoReporteHandler : IRequestHandler<GetNuevoReporteQuery, NuevoReporteDto>
    {
        private readonly IServiceName _service;

        public GetNuevoReporteHandler(IServiceName service)
        {
            _service = service;
        }

        public async Task<NuevoReporteDto> Handle(GetNuevoReporteQuery request, CancellationToken cancellationToken)
        {
            // Implementar lógica aquí
            var data = await _service.ObtenerDatosAsync(request.Id, cancellationToken);
            
            // Transformar y retornar
            return new NuevoReporteDto
            {
                Id = data.Id,
                Nombre = data.Nombre
            };
        }
    }
}
```

#### Paso 4: Agregar el Endpoint (API Layer)

```csharp
// reports_services.api/Controllers/ReportsController.cs
[HttpGet("nuevoReporte/{id}")]
public async Task<ActionResult<NuevoReporteDto>> GetNuevoReporte(Guid id, CancellationToken cancellationToken)
{
    if (id == Guid.Empty)
    {
        return BadRequest("El ID es requerido.");
    }

    try
    {
        var query = new GetNuevoReporteQuery(id);
        var result = await _mediator.Send(query, cancellationToken);

        if (result == null)
        {
            return NotFound($"No se encontró el reporte con ID {id}");
        }

        return Ok(result);
    }
    catch (Exception ex)
    {
        return StatusCode(500, new { message = "Error generando el reporte", details = ex.Message });
    }
}
```

#### Paso 5: Crear Tests

```csharp
// tests/reports_services.api.Tests/Controllers/ReportsControllerTest.cs
[Fact]
public async Task GetNuevoReporte_DeberiaRetornarOk_CuandoExistenDatos()
{
    // Arrange
    var expectedDto = new NuevoReporteDto { Id = Guid.NewGuid(), Nombre = "Test" };
    var mockMediator = new Mock<IMediator>();
    mockMediator
        .Setup(m => m.Send(It.IsAny<GetNuevoReporteQuery>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(expectedDto);
    
    var controller = new ReportsController(mockMediator.Object);

    // Act
    var result = await controller.GetNuevoReporte(expectedDto.Id, CancellationToken.None);

    // Assert
    var okResult = Assert.IsType<OkObjectResult>(result.Result);
    var returnedDto = Assert.IsType<NuevoReporteDto>(okResult.Value);
    Assert.Equal(expectedDto.Id, returnedDto.Id);
}
```

### 3. Agregar un Nuevo Servicio Externo

#### Paso 1: Crear la Interface (Application Layer)

```csharp
// reports_services.application/Interfaces/INewService.cs
namespace reports_services.application.Interfaces
{
    public interface INewService
    {
        Task<List<DataDto>> ObtenerDatosAsync(Guid id, CancellationToken cancellationToken);
    }
}
```

#### Paso 2: Implementar el Servicio (Infrastructure Layer)

```csharp
// reports_services.infrastructure/Services/NewService.cs
using reports_services.application.DTOs;
using reports_services.application.Interfaces;
using System.Text.Json;

namespace reports_services.infrastructure.Services
{
    public class NewService : INewService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public NewService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<DataDto>> ObtenerDatosAsync(Guid id, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"api/endpoint/{id}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new List<DataDto>();
                }

                throw new Exception($"Error al consultar servicio: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);
            var resultado = JsonSerializer.Deserialize<List<DataDto>>(content, _jsonOptions);

            return resultado ?? new List<DataDto>();
        }
    }
}
```

#### Paso 3: Registrar en DI Container (API Layer)

```csharp
// reports_services.api/Program.cs
builder.Services.AddHttpClient<INewService, NewService>(client =>
{
    client.BaseAddress = new Uri("http://localhost:PORT/");
});
```

## Estándares de Código

### Convenciones de Nombres

- **Clases**: PascalCase - `ReportsController`, `PaymentService`
- **Métodos**: PascalCase - `GetReportePagosPorEvento`
- **Parámetros**: camelCase - `idEvento`, `cancellationToken`
- **Variables privadas**: _camelCase - `_httpClient`, `_mediator`
- **Constantes**: PascalCase - `BaseUrl`

### Organización de Usings

```csharp
// 1. System namespaces
using System;
using System.Collections.Generic;

// 2. Microsoft namespaces
using Microsoft.AspNetCore.Mvc;

// 3. Third-party namespaces
using MediatR;

// 4. Project namespaces
using reports_services.application.DTOs;
using reports_services.application.Interfaces;
```

### Async/Await

- Todos los métodos asíncronos deben terminar con `Async`
- Usar `CancellationToken` en métodos async de larga duración
- Evitar `async void`, usar `async Task` en su lugar

```csharp
// ✅ Correcto
public async Task<List<DataDto>> ObtenerDatosAsync(Guid id, CancellationToken cancellationToken)
{
    return await _service.GetDataAsync(id, cancellationToken);
}

// ❌ Incorrecto
public async void ObtenerDatos(Guid id)
{
    var data = await _service.GetData(id);
}
```

### Manejo de Errores

```csharp
// Siempre capturar excepciones específicas cuando sea posible
try
{
    var result = await _service.GetDataAsync(id, cancellationToken);
    return Ok(result);
}
catch (HttpRequestException ex)
{
    // Log the error
    return StatusCode(503, new { message = "Servicio no disponible" });
}
catch (Exception ex)
{
    // Log the error
    return StatusCode(500, new { message = "Error interno", details = ex.Message });
}
```

## Testing

### Ejecutar Tests

```bash
# Todos los tests
dotnet test

# Tests de un proyecto específico
dotnet test tests/reports_services.api.Tests

# Con detalles
dotnet test --logger "console;verbosity=detailed"

# Con cobertura
dotnet test /p:CollectCoverage=true
```

### Estructura de Tests

Seguir el patrón **AAA (Arrange, Act, Assert)**:

```csharp
[Fact]
public async Task MetodoTest_Condicion_ResultadoEsperado()
{
    // Arrange - Configurar datos y mocks
    var mockService = new Mock<IPaymentService>();
    mockService
        .Setup(x => x.ObtenerHistorialPorEventoAsync(It.IsAny<Guid>(), It.IsAny<CancellationToken>()))
        .ReturnsAsync(new List<HistorialPagoExternoDto>());

    // Act - Ejecutar el método bajo test
    var result = await handler.Handle(query, CancellationToken.None);

    // Assert - Verificar el resultado
    Assert.NotNull(result);
    Assert.Empty(result);
}
```

### Naming de Tests

```
[MethodName]_[Scenario]_[ExpectedResult]
```

Ejemplos:
- `GetReportePagosPorEvento_ConIdValido_RetornaListaDePagos`
- `GetReportePagosPorEvento_ConIdVacio_RetornaBadRequest`
- `ObtenerHistorialPorEventoAsync_CuandoServicioNoDisponible_LanzaExcepcion`

## Debugging

### Visual Studio Code

1. **Configurar launch.json**:

```json
{
    "version": "0.2.0",
    "configurations": [
        {
            "name": ".NET Core Launch (web)",
            "type": "coreclr",
            "request": "launch",
            "preLaunchTask": "build",
            "program": "${workspaceFolder}/reports_services.api/bin/Debug/net8.0/reports_services.api.dll",
            "args": [],
            "cwd": "${workspaceFolder}/reports_services.api",
            "env": {
                "ASPNETCORE_ENVIRONMENT": "Development"
            },
            "sourceFileMap": {
                "/Views": "${workspaceFolder}/Views"
            }
        }
    ]
}
```

2. **Agregar breakpoints** haciendo clic en el margen izquierdo del editor
3. **Presionar F5** para iniciar el debugging

### Visual Studio

1. Abrir `reports_services.sln`
2. Establecer `reports_services.api` como proyecto de inicio
3. Agregar breakpoints haciendo clic en el margen izquierdo
4. Presionar F5 o hacer clic en "Start Debugging"

### Logs

```csharp
// Inyectar ILogger
private readonly ILogger<ReportsController> _logger;

public ReportsController(IMediator mediator, ILogger<ReportsController> logger)
{
    _mediator = mediator;
    _logger = logger;
}

// Usar en el código
_logger.LogInformation("Obteniendo reporte para evento {EventId}", idEvento);
_logger.LogWarning("No se encontraron datos para evento {EventId}", idEvento);
_logger.LogError(ex, "Error al procesar reporte para evento {EventId}", idEvento);
```

## Buenas Prácticas

### 1. SOLID Principles

- **Single Responsibility**: Cada clase debe tener una sola razón para cambiar
- **Open/Closed**: Abierto para extensión, cerrado para modificación
- **Liskov Substitution**: Los tipos derivados deben ser sustituibles por sus tipos base
- **Interface Segregation**: Interfaces específicas mejor que interfaces generales
- **Dependency Inversion**: Depender de abstracciones, no de implementaciones concretas

### 2. Clean Code

```csharp
// ✅ Nombres descriptivos
public async Task<PromedioEventSurveyDto> GetPromedioEncuestaPorEventoAsync(Guid eventId)

// ❌ Nombres genéricos
public async Task<PromedioEventSurveyDto> GetData(Guid id)
```

```csharp
// ✅ Métodos pequeños y enfocados
public async Task<List<DataDto>> GetPayments(Guid eventId, CancellationToken ct)
{
    var payments = await _service.GetPaymentsAsync(eventId, ct);
    return MapToDto(payments);
}

// ❌ Métodos muy largos
public async Task<List<DataDto>> GetPayments(Guid eventId, CancellationToken ct)
{
    // 100+ líneas de código...
}
```

### 3. Dependency Injection

- Preferir constructor injection sobre property injection
- Registrar dependencias con el tiempo de vida apropiado:
  - `AddScoped`: Por request HTTP
  - `AddSingleton`: Una instancia para toda la aplicación
  - `AddTransient`: Nueva instancia cada vez

### 4. Error Handling

- No tragar excepciones sin log
- Retornar errores descriptivos a los clientes
- No exponer detalles internos en producción

### 5. Performance

- Usar `async/await` para operaciones I/O
- Evitar bloquear threads con `.Result` o `.Wait()`
- Usar `CancellationToken` para operaciones cancelables
- Considerar caching para datos frecuentemente accedidos

## Comandos Útiles

```bash
# Limpiar solución
dotnet clean

# Restaurar paquetes
dotnet restore

# Compilar
dotnet build

# Compilar en Release
dotnet build -c Release

# Ejecutar
dotnet run --project reports_services.api

# Tests con watch mode
dotnet watch test

# Formatear código
dotnet format

# Listar paquetes NuGet
dotnet list package

# Actualizar paquete
dotnet add package PackageName

# Crear migración (si usas EF Core)
dotnet ef migrations add MigrationName
```

## Recursos Adicionales

- [Documentación de .NET](https://docs.microsoft.com/dotnet/)
- [ASP.NET Core Documentation](https://docs.microsoft.com/aspnet/core/)
- [MediatR Documentation](https://github.com/jbogard/MediatR/wiki)
- [Clean Architecture](https://blog.cleancoder.com/uncle-bob/2012/08/13/the-clean-architecture.html)
- [C# Coding Conventions](https://docs.microsoft.com/dotnet/csharp/fundamentals/coding-style/coding-conventions)

## Obtener Ayuda

Si tienes preguntas o problemas:

1. Revisa esta documentación
2. Consulta la documentación de API en `/docs/API.md`
3. Revisa la arquitectura en `/docs/ARCHITECTURE.md`
4. Contacta al equipo de desarrollo

---

¡Feliz codificación! 🚀
