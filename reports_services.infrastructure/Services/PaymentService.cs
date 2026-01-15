using reports_services.application.DTOs;
using reports_services.application.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace reports_services.infrastructure.Services
{
    public class PaymentService : IPaymentService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public PaymentService(HttpClient httpClient)
        {
            _httpClient = httpClient;
          
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<List<HistorialPagoExternoDto>> ObtenerHistorialPorEventoAsync(Guid idEvento, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"api/payments/historial/{idEvento}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
            
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return new List<HistorialPagoExternoDto>();
                }

               
                throw new Exception($"Error al consultar Pagos: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            var resultado = JsonSerializer.Deserialize<List<HistorialPagoExternoDto>>(content, _jsonOptions);

            return resultado ?? new List<HistorialPagoExternoDto>();
        }
    }
}
