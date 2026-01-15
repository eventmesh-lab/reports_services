using Xunit;
using reports_services.application.Queries.Queries;
using System;

namespace reports_services.application.Tests.Queries
{
    public class GetPromedioEncuestaPorEventoQueryTests
    {
        [Fact]
        public void Constructor_ShouldInitializeEventoIdCorrectly()
        {
            var expectedId = Guid.NewGuid();

            var query = new GetPromedioEncuestaPorEventoQuery(expectedId);

            Assert.Equal(expectedId, query.EventoId);
        }

        [Fact]
        public void EventoId_ShouldBeSettable()
        {
            var initialId = Guid.NewGuid();
            var query = new GetPromedioEncuestaPorEventoQuery(initialId);
            var newId = Guid.NewGuid();

            query.EventoId = newId;

            Assert.Equal(newId, query.EventoId);
        }
    }
}