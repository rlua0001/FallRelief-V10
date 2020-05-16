using System;
using System.Collections.Generic;
using System.Dynamic;
using System.Linq;
using System.Web;

namespace RiskAssessment.Models
{

    public class ActionPlanModel
    {
        public int AssessmentNo { get; set; }
        public int AssessmentTypeID { get; set; }
        public string AssessmentType { get; set; }
        public List<HealthPlanModel> Dlist {get; set;}
        public List<HomeSafetyActionPlanModel> HSList { get; set; }

    }


    public class HomeSafetyActionPlanModel
    {
        public string Risk { get; set; }
        public string ActionRequired { get; set; }
        public string Location { get; set; }
        public string Who { get; set; }
        public string Status { get; set; }
        public string HSimg { get; set; }

    }

    public class HealthPlanModel
    {
        public string Risk { get; set; }
        public List<NutrientModel> Nlist { get; set; }
        public List<ExerciseModel> Elist { get; set; }
    
    }

    public class NutrientModel 
    {
        public string Nutrient { get; set; }
        public string Description { get; set; }
        public List<FoodModel> Flist { get; set; }
    }

    public class FoodModel
    {
        public string FoodGroupName { get; set; }
        public string Description { get; set; }
        public string ImgDir { get; set; }

    }


    public class ExerciseModel
    {
        public string Excerise { get; set; }
        public List<ExceriseSteps> Slist { get; set; }
        public string ImgDir { get; set; }
    }

public class ExceriseSteps
    { 
        public int? StepNo { get; set; }
        public string Description { get; set; }

    }

}