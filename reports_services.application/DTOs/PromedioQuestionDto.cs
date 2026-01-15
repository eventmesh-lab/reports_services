using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reports_services.application.DTOs
{
    public class PromedioQuestionDto
    {
        public Guid QuestionId { get; set; }
        public string QuestionText { get; set; }
        public double PromedioCalculado { get; set; }
        public int CantidadRespuestas { get; set; }
    }
}
