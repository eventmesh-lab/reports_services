using MediatR;
using reports_services.application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reports_services.application.Queries.Queries
{
    public class GetPromedioEncuestaPorEventoQuery : IRequest<PromedioEventSurveyDto>
    {
        public Guid EventoId { get; set; }

        public GetPromedioEncuestaPorEventoQuery(Guid eventoId)
        {
            EventoId = eventoId;
        }
    }
}
