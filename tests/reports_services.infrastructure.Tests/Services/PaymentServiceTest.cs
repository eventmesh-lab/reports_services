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
using System.Collections.Generic;
using System;
using System.Linq;

namespace reports_services.infrastructure.Tests.Services
{
    public class PaymentServiceTests
    {
        private readonly Mock<HttpMessageHandler> _httpMessageHandlerMock;
        private readonly HttpClient _httpClient;
        private readonly PaymentService _service;

        public PaymentServiceTests()
        {
            _httpMessageHandlerMock = new Mock<HttpMessageHandler>();

            _httpClient = new HttpClient(_httpMessageHandlerMock.Object)
            {
                BaseAddress = new Uri("http://localhost/")
            };

            _service = new PaymentService(_httpClient);
        }

        [Fact]
        public async Task ObtenerHistorialPorEventoAsync_ShouldReturnList_WhenResponseIsSuccess()
        {
            var eventId = Guid.NewGuid();
            var expectedData = new List<HistorialPagoExternoDto>
            {
                new HistorialPagoExternoDto { Id = Guid.NewGuid(), TipoMedioDePago = "CreditCard" },
                new HistorialPagoExternoDto { Id = Guid.NewGuid(), TipoMedioDePago = "PayPal" }
            };

            var jsonResponse = JsonSerializer.Serialize(expectedData);

            _httpMessageHandlerMock
                .Protected()
                .Setup<Task<HttpResponseMessage>>(
                    "SendAsync",
                    ItExpr.Is<HttpRequestMessage>(req =>
                        req.Method == HttpMethod.Get &&
                        req.RequestUri.ToString().Contains($"api/payments/historial/{eventId}")),
                    ItExpr.IsAny<CancellationToken>()
                )
                .ReturnsAsync(new HttpResponseMessage
                {
                    StatusCode = HttpStatusCode.OK,
                    Content = new StringContent(jsonResponse)
                });

            var result = await _service.ObtenerHistorialPorEventoAsync(eventId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);
            Assert.Equal("CreditCard", result[0].TipoMedioDePago);
        }

        [Fact]
        public async Task ObtenerHistorialPorEventoAsync_ShouldReturnEmptyList_WhenResponseIsNotFound()
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

            var result = await _service.ObtenerHistorialPorEventoAsync(eventId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task ObtenerHistorialPorEventoAsync_ShouldThrowException_WhenResponseIsServerError()
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
                _service.ObtenerHistorialPorEventoAsync(eventId, CancellationToken.None));

            Assert.Contains("Error al consultar Pagos: InternalServerError", exception.Message);
        }

        [Fact]
        public async Task ObtenerHistorialPorEventoAsync_ShouldHandleCaseInsensitiveJson()
        {
            var eventId = Guid.NewGuid();
            var jsonResponse = "[{\"id\": \"e4d3f2a1-1111-2222-3333-444455556666\", \"tipoMedioDePago\": \"Debit\"}]";

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

            var result = await _service.ObtenerHistorialPorEventoAsync(eventId, CancellationToken.None);

            Assert.Single(result);
            Assert.Equal("Debit", result[0].TipoMedioDePago);
            Assert.Equal(Guid.Parse("e4d3f2a1-1111-2222-3333-444455556666"), result[0].Id);
        }

        [Fact]
        public async Task ObtenerHistorialPorEventoAsync_ShouldReturnEmptyList_WhenContentIsNull()
        {
            var eventId = Guid.NewGuid();
            var jsonResponse = "null";

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

            var result = await _service.ObtenerHistorialPorEventoAsync(eventId, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }
    }
}