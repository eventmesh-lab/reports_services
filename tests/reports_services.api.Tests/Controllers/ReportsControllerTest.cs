using Xunit;
using Moq;
using MediatR;
using Microsoft.AspNetCore.Mvc;
using reports_services.api.Controllers;
using reports_services.application.Queries.Queries;
using reports_services.application.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace reports_services.api.Tests.Controllers
{
    public class ReportsControllerTests
    {
        private readonly Mock<IMediator> _mediatorMock;
        private readonly ReportsController _controller;

        public ReportsControllerTests()
        {
            _mediatorMock = new Mock<IMediator>();
            _controller = new ReportsController(_mediatorMock.Object);
        }

        [Fact]
        public async Task GetReportePagosPorEvento_ShouldReturnBadRequest_WhenIdIsEmpty()
        {
            var result = await _controller.GetReportePagosPorEvento(Guid.Empty, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result);
            Assert.Equal("El ID del evento es requerido.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetReportePagosPorEvento_ShouldReturnOk_WhenRequestIsValid()
        {
            var eventId = Guid.NewGuid();

            var expectedResponse = new List<ReportePagosPorDiaDto>
            {
                new ReportePagosPorDiaDto()
            };

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetReportePagosPorEventoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedResponse);

            var result = await _controller.GetReportePagosPorEvento(eventId, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result);
            Assert.Same(expectedResponse, okResult.Value);

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetReportePagosPorEventoQuery>(q => q.IdEvento == eventId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetReportePagosPorEvento_ShouldReturn500_WhenExceptionOccurs()
        {
            var eventId = Guid.NewGuid();
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetReportePagosPorEventoQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Database error"));

            var result = await _controller.GetReportePagosPorEvento(eventId, CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result);
            Assert.Equal(500, objectResult.StatusCode);
        }

        [Fact]
        public async Task GetReporteEncuestaEvento_ShouldReturnBadRequest_WhenIdIsEmpty()
        {
            var result = await _controller.GetReporteEncuestaEvento(Guid.Empty, CancellationToken.None);

            var badRequestResult = Assert.IsType<BadRequestObjectResult>(result.Result);
            Assert.Equal("El ID del evento no es válido.", badRequestResult.Value);
        }

        [Fact]
        public async Task GetReporteEncuestaEvento_ShouldReturnOk_WhenDataExists()
        {
            var eventId = Guid.NewGuid();
            var expectedDto = new PromedioEventSurveyDto();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetPromedioEncuestaPorEventoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _controller.GetReporteEncuestaEvento(eventId, CancellationToken.None);

            var okResult = Assert.IsType<OkObjectResult>(result.Result);
            Assert.Same(expectedDto, okResult.Value);

            _mediatorMock.Verify(m => m.Send(
                It.Is<GetPromedioEncuestaPorEventoQuery>(q => q.EventoId == eventId),
                It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task GetReporteEncuestaEvento_ShouldReturnNotFound_WhenResultIsNull()
        {
            var eventId = Guid.NewGuid();

            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetPromedioEncuestaPorEventoQuery>(), It.IsAny<CancellationToken>()))
                .ReturnsAsync((PromedioEventSurveyDto)null);

            var result = await _controller.GetReporteEncuestaEvento(eventId, CancellationToken.None);

            var notFoundResult = Assert.IsType<NotFoundObjectResult>(result.Result);
            Assert.Contains(eventId.ToString(), notFoundResult.Value.ToString());
        }

        [Fact]
        public async Task GetReporteEncuestaEvento_ShouldReturn500_WhenExceptionOccurs()
        {
            var eventId = Guid.NewGuid();
            _mediatorMock
                .Setup(m => m.Send(It.IsAny<GetPromedioEncuestaPorEventoQuery>(), It.IsAny<CancellationToken>()))
                .ThrowsAsync(new Exception("Service failure"));

            var result = await _controller.GetReporteEncuestaEvento(eventId, CancellationToken.None);

            var objectResult = Assert.IsType<ObjectResult>(result.Result);
            Assert.Equal(500, objectResult.StatusCode);
        }
    }
}