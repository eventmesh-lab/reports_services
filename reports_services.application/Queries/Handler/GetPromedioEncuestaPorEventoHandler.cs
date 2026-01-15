using MediatR;
using reports_services.application.DTOs;
using reports_services.application.Interfaces;
using reports_services.application.Queries.Queries;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reports_services.application.Queries.Handler
{
    public class GetPromedioEncuestaPorEventoHandler : IRequestHandler<GetPromedioEncuestaPorEventoQuery, PromedioEventSurveyDto>
    {
        private readonly ISurveyService _surveyService;

        public GetPromedioEncuestaPorEventoHandler(ISurveyService surveyService)
        {
            _surveyService = surveyService;
        }

        public async Task<PromedioEventSurveyDto> Handle(GetPromedioEncuestaPorEventoQuery request, CancellationToken cancellationToken)
        {
            var stats = await _surveyService.ObtenerPromedioEncuestaPorEventoAsync(request.EventoId, cancellationToken);

            return stats;
        }
    }
}
