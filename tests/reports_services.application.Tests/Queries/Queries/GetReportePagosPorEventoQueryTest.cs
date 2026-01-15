using Xunit;
using reports_services.application.Queries.Queries;
using reports_services.application.DTOs;
using MediatR;
using System;
using System.Collections.Generic;

namespace reports_services.application.Tests.Queries
{
    public class GetReportePagosPorEventoQueryTests
    {
        [Fact]
        public void Constructor_ShouldInitializeIdEventoCorrectly()
        {
            var expectedId = Guid.NewGuid();

            var query = new GetReportePagosPorEventoQuery(expectedId);

            Assert.Equal(expectedId, query.IdEvento);
        }

        [Fact]
        public void IdEvento_ShouldBeSettable()
        {
            var query = new GetReportePagosPorEventoQuery(Guid.NewGuid());
            var newId = Guid.NewGuid();

            query.IdEvento = newId;

            Assert.Equal(newId, query.IdEvento);
        }

        [Fact]
        public void ShouldImplementIRequestInterface()
        {
            var query = new GetReportePagosPorEventoQuery(Guid.NewGuid());

            Assert.IsAssignableFrom<IRequest<List<ReportePagosPorDiaDto>>>(query);
        }
    }
}