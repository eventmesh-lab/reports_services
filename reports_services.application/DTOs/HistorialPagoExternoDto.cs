using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reports_services.application.DTOs
{
    public class HistorialPagoExternoDto
    {
        public Guid IdEvento { get; set; }
        public Guid Id { get; set; }
        public MontoExternoDto MontoPago { get; set; }
        public DateTime CreatedAt { get; set; }
        public string UltimosDigitosTarjeta { get; set; }
        public Guid IdUsuario { get; set; }
        public string TipoMedioDePago { get; set; }
    }
}
