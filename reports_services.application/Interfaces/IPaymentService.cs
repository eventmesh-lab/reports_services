using reports_services.application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reports_services.application.Interfaces
{
    public interface IPaymentService
    {
        Task<List<HistorialPagoExternoDto>> ObtenerHistorialPorEventoAsync(Guid idEvento, CancellationToken cancellationToken);
    }
}
