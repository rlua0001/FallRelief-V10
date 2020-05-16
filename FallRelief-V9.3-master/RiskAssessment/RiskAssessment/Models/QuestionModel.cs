using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiskAssessment.Models
{
    public class QuestionModel
    {
        public int TotalNoOfSections { get; set; }
        public int AssessmentTypeID { get; set; }
        public string AssessmentType { get; set; }
        public int SectionNo { get; set; }
        public int SectionID { get; set; }
        public string QuestionSection { get; set; }
        public List<QuestionSectionModel> Qlist { get; set; }

    }

    public class QuestionSectionModel
    {
        public int QuestionID { get; set; }
        public string QuestionType { get; set; }
        public string Question { get; set; }
        public List<QRmodel> Rlist { get; set; }

    }

    public class QRmodel // question response modelte
    {
        public int ResponseID { get; set; }
        public string Response { get; set; }
        public string Answer { get; set; }
    }
}