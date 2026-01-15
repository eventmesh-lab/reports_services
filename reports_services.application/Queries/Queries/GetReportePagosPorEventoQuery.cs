using MediatR;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using reports_services.application.DTOs;

namespace reports_services.application.Queries.Queries
{
    public class GetReportePagosPorEventoQuery : IRequest<List<ReportePagosPorDiaDto>>
    {
        public Guid IdEvento { get; set; }

        public GetReportePagosPorEventoQuery(Guid idEvento)
        {
            IdEvento = idEvento;
        }
    }
}
