using Xunit;
using Moq;
using Moq.Protected;
using System.Net;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Text.Json;
using reports_services.infrastructure.Services;
using reports_services.application.DTOs;
using System;

namespace reports_services.infrastructure.Tests.Services
{
    public class SurveyServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly SurveyService _service;

        public SurveyServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            _service = new SurveyService(_httpClient);
        }

        [Fact]
        public async Task ObtenerPromedioEncuestaPorEventoAsync_ShouldReturnDto_WhenResponseIsSuccess()
        {
            var eventId = Guid.NewGuid();
            var expectedDto = new PromedioEventSurveyDto
            {
                EventoId = eventId,
                SurveyId = Guid.NewGuid(),
                SurveyTitle = "Satisfacción del Evento"
            };

            var jsonResponse = JsonSerializer.Serialize(expectedDto);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains($"api/surveys/promedioRespuestasEvento/{eventId}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var result = await _service.ObtenerPromedioEncuestaPorEventoAsync(eventId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(expectedDto.EventoId, result.EventoId);
            Assert.Equal(expectedDto.SurveyTitle, result.SurveyTitle);
        }

        [Fact]
        public async Task ObtenerPromedioEncuestaPorEventoAsync_ShouldReturnNull_WhenResponseIsNotFound()
        {
            var eventId = Guid.NewGuid();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.NotFound 
                });

            var result = await _service.ObtenerPromedioEncuestaPorEventoAsync(eventId, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task ObtenerPromedioEncuestaPorEventoAsync_ShouldThrowException_WhenResponseIsServerError()
        {
            var eventId = Guid.NewGuid();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.InternalServerError 
                });

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _service.ObtenerPromedioEncuestaPorEventoAsync(eventId, CancellationToken.None));

            Assert.Contains("Error al consultar Encuestas: InternalServerError", exception.Message);
        }

        [Fact]
        public async Task ObtenerPromedioEncuestaPorEventoAsync_ShouldReturnNull_WhenContentIsEmpty()
        {
            var eventId = Guid.NewGuid();

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent("")
                });

            var result = await _service.ObtenerPromedioEncuestaPorEventoAsync(eventId, CancellationToken.None);

            Assert.Null(result);
        }

        [Fact]
        public async Task ObtenerPromedioEncuestaPorEventoAsync_ShouldThrowException_WhenJsonIsInvalid()
        {
            var eventId = Guid.NewGuid();
            var invalidJson = "{ esto no es json valido }";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(invalidJson)
                });

            var exception = await Assert.ThrowsAsync<Exception>(() =>
                _service.ObtenerPromedioEncuestaPorEventoAsync(eventId, CancellationToken.None));

            Assert.Contains("Error deserializando respuesta de encuestas", exception.Message);
        }

        [Fact]
        public async Task ObtenerPromedioEncuestaPorEventoAsync_ShouldHandleCaseInsensitiveProperties()
        {
            var eventId = Guid.NewGuid();
            var jsonResponse = $"{{\"surveyTitle\": \"Test Title\", \"eventoId\": \"{eventId}\"}}";

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.IsAny<HttpRequestMessage>(),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var result = await _service.ObtenerPromedioEncuestaPorEventoAsync(eventId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal("Test Title", result.SurveyTitle);
            Assert.Equal(eventId, result.EventoId);
        }
    }
}