using reports_services.application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reports_services.application.Interfaces
{
    public interface ISurveyService
    {
        Task<PromedioEventSurveyDto?> ObtenerPromedioEncuestaPorEventoAsync(Guid eventId, CancellationToken cancellationToken);
    }
}
