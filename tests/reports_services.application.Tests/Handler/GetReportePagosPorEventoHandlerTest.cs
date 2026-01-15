using Xunit;
using Moq;
using reports_services.application.Queries.Handler;
using reports_services.application.Queries.Queries;
using reports_services.application.Interfaces;
using reports_services.application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace reports_services.application.Tests.Handler
{
    public class GetReportePagosPorEventoHandlerTests
    {
        private readonly Mock<IPaymentService> _paymentServiceMock;
        private readonly GetReportePagosPorEventoHandler _handler;

        public GetReportePagosPorEventoHandlerTests()
        {
            _paymentServiceMock = new Mock<IPaymentService>();
            _handler = new GetReportePagosPorEventoHandler(_paymentServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenServiceReturnsNull()
        {
            var query = new GetReportePagosPorEventoQuery(Guid.NewGuid());

            _paymentServiceMock
                .Setup(s => s.ObtenerHistorialPorEventoAsync(query.IdEvento, It.IsAny<CancellationToken>()))
                .ReturnsAsync((List<HistorialPagoExternoDto>)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ShouldReturnEmptyList_WhenServiceReturnsEmpty()
        {
            var query = new GetReportePagosPorEventoQuery(Guid.NewGuid());

            _paymentServiceMock
                .Setup(s => s.ObtenerHistorialPorEventoAsync(query.IdEvento, It.IsAny<CancellationToken>()))
                .ReturnsAsync(new List<HistorialPagoExternoDto>());

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Empty(result);
        }

        [Fact]
        public async Task Handle_ShouldReturnGroupedAndSummedReport_WhenPaymentsExist()
        {
            var eventId = Guid.NewGuid();
            var query = new GetReportePagosPorEventoQuery(eventId);
            var date1 = new DateTime(2025, 1, 1, 10, 0, 0);
            var date2 = new DateTime(2025, 1, 2, 12, 0, 0);

            var payments = new List<HistorialPagoExternoDto>
            {
                new HistorialPagoExternoDto
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = date1,
                    MontoPago = new MontoExternoDto { montoPago = 100 },
                    IdUsuario = Guid.NewGuid(),
                    TipoMedioDePago = "CreditCard",
                    UltimosDigitosTarjeta = "1234"
                },
                new HistorialPagoExternoDto
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = date1.AddHours(1),
                    MontoPago = new MontoExternoDto { montoPago = 50 },
                    IdUsuario = Guid.NewGuid(),
                    TipoMedioDePago = "DebitCard",
                    UltimosDigitosTarjeta = "5678"
                },
                new HistorialPagoExternoDto
                {
                    Id = Guid.NewGuid(),
                    CreatedAt = date2,
                    MontoPago = new MontoExternoDto { montoPago = 200 },
                    IdUsuario = Guid.NewGuid(),
                    TipoMedioDePago = "Paypal",
                    UltimosDigitosTarjeta = "9999"
                }
            };

            _paymentServiceMock
                .Setup(s => s.ObtenerHistorialPorEventoAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(payments);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Equal(2, result.Count);

            var reportDate2 = result.First();
            Assert.Equal(date2.Date, reportDate2.Fecha);
            Assert.Equal(200, reportDate2.TotalDelDia);
            Assert.Equal(1, reportDate2.CantidadTransacciones);

            var reportDate1 = result.Last();
            Assert.Equal(date1.Date, reportDate1.Fecha);
            Assert.Equal(150, reportDate1.TotalDelDia);
            Assert.Equal(2, reportDate1.CantidadTransacciones);

            var detail = reportDate1.Pagos.First();
            Assert.Equal("CreditCard (**1234)", detail.MetodoPago);
        }
    }
}