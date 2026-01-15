using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reports_services.application.DTOs
{
    public class ReportePagosPorDiaDto
    {
        public DateTime Fecha { get; set; }
        public decimal TotalDelDia { get; set; }
        public int CantidadTransacciones { get; set; }
        public List<DetallePagoReporteDto> Pagos { get; set; } = new List<DetallePagoReporteDto>();
    }
}
