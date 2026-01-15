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
    public class SurveyService : ISurveyService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions;

        public SurveyService(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };
        }

        public async Task<PromedioEventSurveyDto?> ObtenerPromedioEncuestaPorEventoAsync(Guid eventId, CancellationToken cancellationToken)
        {
            var response = await _httpClient.GetAsync($"api/surveys/promedioRespuestasEvento/{eventId}", cancellationToken);

            if (!response.IsSuccessStatusCode)
            {
                if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    return null; 
                }

                throw new Exception($"Error al consultar Encuestas: {response.StatusCode}");
            }

            var content = await response.Content.ReadAsStringAsync(cancellationToken);

            if (string.IsNullOrWhiteSpace(content))
                return null;

            try
            {
                var result = JsonSerializer.Deserialize<PromedioEventSurveyDto>(content, _jsonOptions);
                return result;
            }
            catch (JsonException ex)
            {
                throw new Exception($"Error deserializando respuesta de encuestas: {ex.Message}");
            }
        }
    }
}
