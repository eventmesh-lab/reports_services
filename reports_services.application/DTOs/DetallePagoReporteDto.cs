using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reports_services.application.DTOs
{
    public class DetallePagoReporteDto
    {
        public Guid IdPago { get; set; }
        public Guid IdUsuario { get; set; }

        public decimal MontoPago { get; set; }

        public string MetodoPago { get; set; }

        public DateTime Hora { get; set; }
    }
}
