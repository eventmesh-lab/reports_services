using MediatR;
using Microsoft.AspNetCore.Mvc;
using reports_services.application.DTOs;
using reports_services.application.Queries.Queries;

namespace reports_services.api.Controllers
{
    [ApiController]
    [Route("api/reports")]
    public class ReportsController : ControllerBase
    {
        private readonly IMediator _mediator;

        public ReportsController(IMediator mediator)
        {
            _mediator = mediator;
        }

        [HttpGet("pagosPorEvento/{idEvento}")]
        public async Task<IActionResult> GetReportePagosPorEvento(Guid idEvento, CancellationToken cancellationToken)
        {
            if (idEvento == Guid.Empty)
            {
                return BadRequest("El ID del evento es requerido.");
            }

            try
            {
                var query = new GetReportePagosPorEventoQuery(idEvento);
                var result = await _mediator.Send(query, cancellationToken);

                // Opcional: Si la lista está vacía podrías retornar NotFound o Ok([])
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error generando el reporte", details = ex.Message });
            }
        }

        [HttpGet("promedioPorEvento/{eventId}")]
        public async Task<ActionResult<PromedioEventSurveyDto>> GetReporteEncuestaEvento(Guid eventId, CancellationToken cancellationToken)
        {
            if (eventId == Guid.Empty)
            {
                return BadRequest("El ID del evento no es válido.");
            }

            try
            {
                var query = new GetPromedioEncuestaPorEventoQuery(eventId);
                var result = await _mediator.Send(query, cancellationToken);

                if (result == null)
                {
                    return NotFound($"No se encontraron estadísticas de encuestas para el evento {eventId} (o el evento no tiene encuestas asociadas).");
                }

                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { message = "Error interno generando el reporte de encuestas", details = ex.Message });
            }
        }
    }
}
