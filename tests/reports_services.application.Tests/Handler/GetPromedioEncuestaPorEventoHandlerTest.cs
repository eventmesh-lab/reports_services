using Xunit;
using Moq;
using reports_services.application.Queries.Handler;
using reports_services.application.Queries.Queries;
using reports_services.application.Interfaces;
using reports_services.application.DTOs;
using System;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;

namespace reports_services.application.Tests.Queries.Handlers
{
    public class GetPromedioEncuestaPorEventoHandlerTests
    {
        private readonly Mock<ISurveyService> _surveyServiceMock;
        private readonly GetPromedioEncuestaPorEventoHandler _handler;

        public GetPromedioEncuestaPorEventoHandlerTests()
        {
            _surveyServiceMock = new Mock<ISurveyService>();
            _handler = new GetPromedioEncuestaPorEventoHandler(_surveyServiceMock.Object);
        }

        [Fact]
        public async Task Handle_ShouldReturnDto_WhenServiceReturnsData()
        {
            var eventId = Guid.NewGuid();
            var query = new GetPromedioEncuestaPorEventoQuery(eventId);
            var expectedDto = new PromedioEventSurveyDto();

            _surveyServiceMock
                .Setup(s => s.ObtenerPromedioEncuestaPorEventoAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync(expectedDto);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.NotNull(result);
            Assert.Same(expectedDto, result);

            _surveyServiceMock.Verify(s => s.ObtenerPromedioEncuestaPorEventoAsync(eventId, It.IsAny<CancellationToken>()), Times.Once);
        }

        [Fact]
        public async Task Handle_ShouldReturnNull_WhenServiceReturnsNull()
        {
            var eventId = Guid.NewGuid();
            var query = new GetPromedioEncuestaPorEventoQuery(eventId);

            _surveyServiceMock
                .Setup(s => s.ObtenerPromedioEncuestaPorEventoAsync(eventId, It.IsAny<CancellationToken>()))
                .ReturnsAsync((PromedioEventSurveyDto)null);

            var result = await _handler.Handle(query, CancellationToken.None);

            Assert.Null(result);

            _surveyServiceMock.Verify(s => s.ObtenerPromedioEncuestaPorEventoAsync(eventId, It.IsAny<CancellationToken>()), Times.Once);
        }
    }
}