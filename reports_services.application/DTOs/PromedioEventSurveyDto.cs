using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace reports_services.application.DTOs
{
    public class PromedioEventSurveyDto
    {
        public Guid EventoId { get; set; }
        public Guid SurveyId { get; set; }
        public string SurveyTitle { get; set; }
        public List<PromedioQuestionDto> QuestionsStats { get; set; } = new List<PromedioQuestionDto>();
    }
}
