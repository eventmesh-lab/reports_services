# Documentación de API

## Información General

- **Versión de API**: 1.0
- **Base URL**: `http://localhost:7187`
- **Protocolo**: HTTP/HTTPS
- **Formato de Datos**: JSON
- **Autenticación**: Sin autenticación (TODO: implementar)

## Documentación Interactiva

Cuando el servicio está en modo desarrollo, puedes acceder a la documentación interactiva de Swagger:

- **Swagger UI**: `http://localhost:7187/swagger`
- **OpenAPI JSON**: `http://localhost:7187/swagger/v1/swagger.json`

## Endpoints

### 1. Obtener Reporte de Pagos por Evento

Obtiene el historial consolidado de todos los pagos asociados a un evento específico.

#### Request

```http
GET /api/reports/pagosPorEvento/{idEvento}
```

#### Parámetros de Ruta

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| idEvento  | GUID | Sí        | Identificador único del evento |

#### Ejemplo de Request

```bash
curl -X GET "http://localhost:7187/api/reports/pagosPorEvento/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
     -H "accept: application/json"
```

#### Respuestas

##### 200 OK - Éxito

```json
[
  {
    "idPago": "a1b2c3d4-e5f6-7890-abcd-ef1234567890",
    "monto": 150.00,
    "moneda": "USD",
    "fechaPago": "2024-01-15T14:30:00Z",
    "metodoPago": "Tarjeta de Crédito",
    "estado": "Completado",
    "usuarioId": "user-123",
    "nombreUsuario": "Juan Pérez",
    "emailUsuario": "juan.perez@example.com"
  },
  {
    "idPago": "b2c3d4e5-f6g7-8901-bcde-fg2345678901",
    "monto": 200.00,
    "moneda": "USD",
    "fechaPago": "2024-01-16T10:15:00Z",
    "metodoPago": "PayPal",
    "estado": "Completado",
    "usuarioId": "user-456",
    "nombreUsuario": "María García",
    "emailUsuario": "maria.garcia@example.com"
  }
]
```

**Descripción de campos de respuesta**:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| idPago | GUID | Identificador único del pago |
| monto | decimal | Cantidad pagada |
| moneda | string | Código de moneda (USD, EUR, etc.) |
| fechaPago | DateTime | Fecha y hora del pago en formato ISO 8601 |
| metodoPago | string | Método de pago utilizado |
| estado | string | Estado del pago (Completado, Pendiente, Cancelado) |
| usuarioId | string | Identificador del usuario que realizó el pago |
| nombreUsuario | string | Nombre completo del usuario |
| emailUsuario | string | Email del usuario |

##### 400 Bad Request - ID Inválido

```json
{
  "message": "El ID del evento es requerido."
}
```

**Causas**:
- El GUID proporcionado es vacío (`00000000-0000-0000-0000-000000000000`)
- El formato del GUID es inválido

##### 500 Internal Server Error - Error del Servidor

```json
{
  "message": "Error generando el reporte",
  "details": "Error al consultar Pagos: ServiceUnavailable"
}
```

**Causas posibles**:
- El servicio de pagos no está disponible
- Error de red al comunicarse con el servicio de pagos
- Timeout en la comunicación
- Error al deserializar la respuesta

#### Notas Importantes

- Si no hay pagos para el evento, se retorna un array vacío `[]`
- Los pagos se retornan ordenados por fecha (más reciente primero)
- La comunicación con el servicio de pagos es síncrona

---

### 2. Obtener Promedio de Encuestas por Evento

Calcula y retorna el promedio de las respuestas de todas las encuestas asociadas a un evento específico.

#### Request

```http
GET /api/reports/promedioPorEvento/{eventId}
```

#### Parámetros de Ruta

| Parámetro | Tipo | Requerido | Descripción |
|-----------|------|-----------|-------------|
| eventId   | GUID | Sí        | Identificador único del evento |

#### Ejemplo de Request

```bash
curl -X GET "http://localhost:7187/api/reports/promedioPorEvento/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
     -H "accept: application/json"
```

#### Respuestas

##### 200 OK - Éxito

```json
{
  "eventId": "3fa85f64-5717-4562-b3fc-2c963f66afa6",
  "nombreEvento": "Conferencia de Tecnología 2024",
  "promedioGeneral": 4.5,
  "totalEncuestas": 150,
  "totalRespuestas": 750,
  "fechaInicio": "2024-01-10T09:00:00Z",
  "fechaFin": "2024-01-12T18:00:00Z",
  "preguntasPromedio": [
    {
      "preguntaId": "q1-uuid",
      "textoPregunta": "¿Cómo calificarías la organización del evento?",
      "tipoRespuesta": "Escala",
      "promedio": 4.8,
      "totalRespuestas": 148,
      "respuestaMinima": 3,
      "respuestaMaxima": 5
    },
    {
      "preguntaId": "q2-uuid",
      "textoPregunta": "¿El contenido cumplió tus expectativas?",
      "tipoRespuesta": "Escala",
      "promedio": 4.3,
      "totalRespuestas": 145,
      "respuestaMinima": 2,
      "respuestaMaxima": 5
    },
    {
      "preguntaId": "q3-uuid",
      "textoPregunta": "¿Recomendarías este evento?",
      "tipoRespuesta": "Escala",
      "promedio": 4.6,
      "totalRespuestas": 150,
      "respuestaMinima": 3,
      "respuestaMaxima": 5
    }
  ]
}
```

**Descripción de campos de respuesta**:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| eventId | GUID | Identificador del evento |
| nombreEvento | string | Nombre del evento |
| promedioGeneral | decimal | Promedio general de todas las encuestas (escala 1-5) |
| totalEncuestas | integer | Número total de encuestas completadas |
| totalRespuestas | integer | Número total de respuestas individuales |
| fechaInicio | DateTime | Fecha de inicio del evento |
| fechaFin | DateTime | Fecha de finalización del evento |
| preguntasPromedio | array | Array de estadísticas por pregunta |

**Campos de preguntasPromedio**:

| Campo | Tipo | Descripción |
|-------|------|-------------|
| preguntaId | GUID | Identificador de la pregunta |
| textoPregunta | string | Texto completo de la pregunta |
| tipoRespuesta | string | Tipo de respuesta (Escala, Abierta, Múltiple) |
| promedio | decimal | Promedio de respuestas para esta pregunta |
| totalRespuestas | integer | Número de respuestas para esta pregunta |
| respuestaMinima | integer | Valor mínimo recibido |
| respuestaMaxima | integer | Valor máximo recibido |

##### 400 Bad Request - ID Inválido

```json
{
  "message": "El ID del evento no es válido."
}
```

**Causas**:
- El GUID proporcionado es vacío
- El formato del GUID es inválido

##### 404 Not Found - No se encontraron encuestas

```json
{
  "message": "No se encontraron estadísticas de encuestas para el evento 3fa85f64-5717-4562-b3fc-2c963f66afa6 (o el evento no tiene encuestas asociadas)."
}
```

**Causas**:
- El evento no existe
- El evento no tiene encuestas asociadas
- No hay respuestas a las encuestas del evento

##### 500 Internal Server Error - Error del Servidor

```json
{
  "message": "Error interno generando el reporte de encuestas",
  "details": "Error al consultar el servicio de encuestas: Connection refused"
}
```

**Causas posibles**:
- El servicio de encuestas no está disponible
- Error de red al comunicarse con el servicio
- Timeout en la comunicación
- Error al procesar los datos de encuestas

#### Notas Importantes

- Los promedios se calculan en escala de 1 a 5
- Solo se incluyen encuestas completadas
- Las preguntas abiertas no se incluyen en el cálculo de promedios
- El `promedioGeneral` es el promedio de todos los promedios de preguntas

---

## Códigos de Estado HTTP

| Código | Descripción | Uso |
|--------|-------------|-----|
| 200    | OK          | Solicitud exitosa, datos retornados |
| 400    | Bad Request | Parámetros inválidos o faltantes |
| 404    | Not Found   | Recurso no encontrado |
| 500    | Internal Server Error | Error del servidor o servicios dependientes |

## Manejo de Errores

Todos los errores siguen el formato:

```json
{
  "message": "Descripción del error para el usuario",
  "details": "Información técnica adicional (solo en desarrollo)"
}
```

### Errores Comunes y Soluciones

#### 1. Service Unavailable (500)

**Problema**: No se puede conectar con servicios dependientes.

**Solución**:
- Verificar que Payment Service esté corriendo en `http://localhost:7183`
- Verificar que Survey Service esté corriendo en `http://localhost:7186`
- Revisar logs del servicio para más detalles

#### 2. Invalid GUID Format (400)

**Problema**: El formato del GUID no es válido.

**Solución**:
- Asegurar que el GUID tenga el formato correcto: `xxxxxxxx-xxxx-xxxx-xxxx-xxxxxxxxxxxx`
- Ejemplo válido: `3fa85f64-5717-4562-b3fc-2c963f66afa6`

#### 3. Empty Response Array

**Problema**: Se recibe un array vacío `[]` cuando se esperaban datos.

**Solución**:
- Verificar que el evento exista en los servicios correspondientes
- Verificar que haya datos asociados al evento (pagos o encuestas)
- Confirmar que el ID del evento sea correcto

## Configuración de CORS

El servicio permite peticiones desde los siguientes orígenes:

- `http://localhost:3000` (Frontend de desarrollo)

Para agregar más orígenes, modificar `Program.cs`:

```csharp
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowLocalhost3000", policy =>
    {
        policy.WithOrigins("http://localhost:3000", "https://tu-dominio.com")
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});
```

## Rate Limiting

**Estado actual**: No implementado

**TODO**: Considerar implementar rate limiting para:
- Limitar número de requests por minuto por IP
- Proteger contra ataques DDoS
- Prevenir abuso de la API

## Versionamiento

**Versión actual**: v1 (implícita)

**Estrategia futura**:
- Usar versionamiento en URL: `/api/v2/reports/...`
- O versionamiento por headers: `Accept: application/vnd.reports.v2+json`

## Ejemplos de Integración

### JavaScript (Fetch API)

```javascript
// Obtener reporte de pagos
async function getPaymentReport(eventId) {
  try {
    const response = await fetch(
      `http://localhost:7187/api/reports/pagosPorEvento/${eventId}`
    );
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Error fetching payment report:', error);
    throw error;
  }
}

// Obtener reporte de encuestas
async function getSurveyReport(eventId) {
  try {
    const response = await fetch(
      `http://localhost:7187/api/reports/promedioPorEvento/${eventId}`
    );
    
    if (!response.ok) {
      throw new Error(`HTTP error! status: ${response.status}`);
    }
    
    const data = await response.json();
    return data;
  } catch (error) {
    console.error('Error fetching survey report:', error);
    throw error;
  }
}
```

### Python (requests)

```python
import requests

def get_payment_report(event_id):
    """Obtener reporte de pagos por evento"""
    url = f"http://localhost:7187/api/reports/pagosPorEvento/{event_id}"
    
    try:
        response = requests.get(url)
        response.raise_for_status()
        return response.json()
    except requests.exceptions.RequestException as e:
        print(f"Error: {e}")
        return None

def get_survey_report(event_id):
    """Obtener reporte de encuestas por evento"""
    url = f"http://localhost:7187/api/reports/promedioPorEvento/{event_id}"
    
    try:
        response = requests.get(url)
        response.raise_for_status()
        return response.json()
    except requests.exceptions.RequestException as e:
        print(f"Error: {e}")
        return None

# Uso
event_id = "3fa85f64-5717-4562-b3fc-2c963f66afa6"
payments = get_payment_report(event_id)
surveys = get_survey_report(event_id)
```

### C# (HttpClient)

```csharp
using System.Net.Http;
using System.Text.Json;

public class ReportsClient
{
    private readonly HttpClient _httpClient;
    private const string BaseUrl = "http://localhost:7187";

    public ReportsClient(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.BaseAddress = new Uri(BaseUrl);
    }

    public async Task<List<DetallePagoReporteDto>> GetPaymentReportAsync(Guid eventId)
    {
        var response = await _httpClient.GetAsync($"/api/reports/pagosPorEvento/{eventId}");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<List<DetallePagoReporteDto>>(content);
    }

    public async Task<PromedioEventSurveyDto> GetSurveyReportAsync(Guid eventId)
    {
        var response = await _httpClient.GetAsync($"/api/reports/promedioPorEvento/{eventId}");
        response.EnsureSuccessStatusCode();
        
        var content = await response.Content.ReadAsStringAsync();
        return JsonSerializer.Deserialize<PromedioEventSurveyDto>(content);
    }
}
```

## Testing de la API

### Con cURL

```bash
# Test reporte de pagos
curl -X GET "http://localhost:7187/api/reports/pagosPorEvento/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
     -H "accept: application/json" \
     -v

# Test reporte de encuestas
curl -X GET "http://localhost:7187/api/reports/promedioPorEvento/3fa85f64-5717-4562-b3fc-2c963f66afa6" \
     -H "accept: application/json" \
     -v
```

### Con Postman

1. Importar la colección desde Swagger: `http://localhost:7187/swagger/v1/swagger.json`
2. O crear requests manualmente:
   - Method: GET
   - URL: `http://localhost:7187/api/reports/pagosPorEvento/{eventId}`
   - Headers: `Accept: application/json`

### Con archivo .http (VS Code REST Client)

El proyecto incluye un archivo `reports_services.api.http` para testing:

```http
### Obtener reporte de pagos
GET http://localhost:7187/api/reports/pagosPorEvento/3fa85f64-5717-4562-b3fc-2c963f66afa6
Accept: application/json

### Obtener reporte de encuestas
GET http://localhost:7187/api/reports/promedioPorEvento/3fa85f64-5717-4562-b3fc-2c963f66afa6
Accept: application/json
```

## Mejoras Futuras

- [ ] Implementar autenticación y autorización
- [ ] Agregar paginación para reportes grandes
- [ ] Implementar filtros adicionales (fecha, estado, etc.)
- [ ] Agregar endpoints para exportar reportes (PDF, Excel)
- [ ] Implementar caching con Redis
- [ ] Agregar webhooks para notificaciones de reportes
- [ ] Implementar GraphQL como alternativa a REST
- [ ] Agregar compresión de respuestas (gzip)
- [ ] Implementar rate limiting
- [ ] Agregar soporte para queries asíncronas con callbacks

## Soporte

Para reportar problemas con la API o sugerir mejoras, por favor:
1. Revisa la documentación de Swagger
2. Verifica que los servicios dependientes estén funcionando
3. Revisa los logs del servicio
4. Contacta al equipo de desarrollo con detalles específicos del error
