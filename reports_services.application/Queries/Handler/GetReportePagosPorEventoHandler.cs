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
    public class
        GetReportePagosPorEventoHandler : IRequestHandler<GetReportePagosPorEventoQuery, List<ReportePagosPorDiaDto>>
    {
        private readonly IPaymentService _paymentService;

        public GetReportePagosPorEventoHandler(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        public async Task<List<ReportePagosPorDiaDto>> Handle(GetReportePagosPorEventoQuery request,
            CancellationToken cancellationToken)
        {
            var historialPagosRaw =
                await _paymentService.ObtenerHistorialPorEventoAsync(request.IdEvento, cancellationToken);

            if (historialPagosRaw == null || !historialPagosRaw.Any())
            {
                return new List<ReportePagosPorDiaDto>();
            }

            var reporteAgrupado = historialPagosRaw
                .GroupBy(p => p.CreatedAt.Date)
                .Select(grupo => new ReportePagosPorDiaDto
                {
                    Fecha = grupo.Key,

                    TotalDelDia = grupo.Sum(p => p.MontoPago.montoPago),

                    CantidadTransacciones = grupo.Count(),
                    Pagos = grupo.Select(p => new DetallePagoReporteDto()
                        {
                            IdPago = p.Id,
                            IdUsuario = p.IdUsuario,

                            MontoPago = p.MontoPago.montoPago ,

                            MetodoPago = $"{p.TipoMedioDePago} (**{p.UltimosDigitosTarjeta})",
                            Hora = p.CreatedAt
                        })
                        .OrderBy(p => p.Hora)
                        .ToList()
                })
                .OrderByDescending(r => r.Fecha)
                .ToList();

            return reporteAgrupado;
        }
    }
}