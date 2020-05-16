using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace RiskAssessment.Models
{
    public class ResponseModel
    {
        public int ResponseID { get; set; }
        public string IsChecked { get; set; }
    }


    public class QASectionModel
    {
        public int AssessmentTypeID { get; set; }
        public int SectionID { get; set; }
        public Guid SessionID { get; set; }
        public string Direction { get; set; }
        public List<AnswerModel> Alist { get; set; }
    }

    public class AnswerModel
    {
        public int QuestionID { get; set; }
        public List<ResponseModel> UserReponse { get; set; }
        public string Answer { get; set; }
        public string Direction { get; set; }

        public List<int> UserSelectedID
        {
            get
            {
                return UserReponse == null ? new List<int>() :
                    UserReponse.Where(x => x.IsChecked == "on" || "true".Equals(x.IsChecked, StringComparison.InvariantCultureIgnoreCase)).Select(x => x.ResponseID).ToList();
            }
        }
    }
}