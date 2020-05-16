using RiskAssessment.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity.Core.Metadata.Edm;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Web;
using System.Web.Hosting;
using System.Web.Mvc;
using WebGrease.Css.Ast;

namespace RiskAssessment.Controllers
{
    public class HomeController : Controller
    {


        public ActionResult testflaticon()
        {
            return View();
        }
        public ActionResult Index()
        {
            return View();

        }

        public ActionResult Assessment()
        {
            var _ctx = new fall_reliefEntities();
            ViewBag.Assessment = _ctx.tbl_RiskAss_Assessment.Where(x => x.IsActive == true).Select(x => new { x.AssessmentTypeID, x.AssessmentType }).ToList();

            SessionModel _model = null;

            if (Session["SessionModel"] == null)
                _model = new SessionModel();
            else
                _model = (SessionModel)Session["SessionModel"];


            return View(_model);

        }


        public ActionResult Instruction(SessionModel model, int number)
        {
            if (model != null)
            {
                var _ctx = new fall_reliefEntities();
                var assessment = _ctx.tbl_RiskAss_Assessment.Where(x => x.AssessmentTypeID == number).FirstOrDefault();
                if (assessment != null)
                {
                    ViewBag.AssessmentType = assessment.AssessmentType;
                    ViewBag.AssessmentDescription = assessment.Description;

                }
            }
            Session["SessionModel"] = model;
            model.AssessmentTypeID = number;
            if (model == null)
                return RedirectToAction("Assessment");


            return View(model);
        }

        public ActionResult RiskAssessment(SessionModel model)
        {
            if (model != null)
            {
                Session["SessionModel"] = model;
            }

            if (model == null)
            {
                return RedirectToAction("Assessment");
            }

            var _ctx = new fall_reliefEntities();
            //To create new assessment
            tbl_RiskAss_Session newAssessment = new tbl_RiskAss_Session()
            {
                timeStamp = DateTime.UtcNow,
                AssessmentTypeID = model.AssessmentTypeID,
                sessionID = Guid.NewGuid()

            };
            _ctx.tbl_RiskAss_Session.Add(newAssessment);
            _ctx.SaveChanges();

            this.Session["SessionID"] = newAssessment.sessionID;

            return RedirectToAction("QuestionAssessment", new { @SessionID = Session["SessionID"] });
        }


        public ActionResult QuestionAssessment(Guid SessionID, int? secNO)
        {

            if (SessionID == null)
            {
                return RedirectToAction("Assessment");

            }

            var _ctx = new fall_reliefEntities();

            var asessment = _ctx.tbl_RiskAss_Session.Where(x => x.sessionID.Equals(SessionID)).FirstOrDefault();

            if (asessment == null)
            {
                return RedirectToAction("Assessment");
            }

            if (secNO.GetValueOrDefault() < 1)
                secNO = 1;

            var assSecQuestionID = _ctx.tbl_RiskAss_QuestionSection
                .Where(x => x.AssessmentTypeID == asessment.AssessmentTypeID && x.SectionNo == secNO).FirstOrDefault();

            if (assSecQuestionID.SectionNo > 0)
            {
                var _model = _ctx.tbl_RiskAss_QuestionSection.Where(x => x.SectionID == assSecQuestionID.SectionID)
                    .Select(x => new QuestionModel()
                    {
                        AssessmentTypeID = x.tbl_RiskAss_Assessment.AssessmentTypeID,
                        AssessmentType = x.tbl_RiskAss_Assessment.AssessmentType,
                        QuestionSection = x.SectionQuestion,
                        SectionID = x.SectionID,
                        SectionNo = secNO ?? default,
                        Qlist = x.tbl_RiskAss_Questions.Select(z => new QuestionSectionModel()
                        {
                            Question = z.Question,
                            QuestionID = z.QuestionID,
                            QuestionType = z.AnswerType,

                            Rlist = z.tbl_RiskAss_ResponseChoice.Select(y => new QRmodel()
                            {
                                ResponseID = y.ID,
                                Response = y.Response
                            }).ToList()
                        }).ToList()

                    }).FirstOrDefault();


                ////now if it is already answered ealier, set the choice of the user

                //first question number
                var firstqno = _ctx.tbl_RiskAss_AssQuestion.Where(x => x.SectionNumber == assSecQuestionID.SectionNo && x.AssessmentTypeID == asessment.AssessmentTypeID).OrderBy(x => x.QuestionID).Take(1).Select(x => x.QuestionID).First();
                //no of questions
                var noOfQuestions = _ctx.tbl_RiskAss_Questions.Where(x => x.SectionID == assSecQuestionID.SectionID).Count();


                for (int t = 0; t < noOfQuestions; t++) 
                {
                    var qno = t + firstqno;

                    var savedAnswers = _ctx.tbl_RiskAss_AssessmentResponse.Where(x => x.SectionID == assSecQuestionID.SectionID && x.AssessmentNo == asessment.AssessmentNo && x.AssQuestionID == qno)
                    .Select(x => new { x.responseID, x.Answer }).ToList();

                    foreach (var savedAnswer in savedAnswers)
                    {
                        _model.Qlist[t].Rlist.Where(x => x.ResponseID == savedAnswer.responseID).FirstOrDefault().Answer = savedAnswer.Answer;
                    }

                }

                _model.TotalNoOfSections = _ctx.tbl_RiskAss_QuestionSection.Where(x => x.AssessmentTypeID == asessment.AssessmentTypeID).Count();

                return View(_model);
            }

            else
            {

                return View("Error");
            }

        }
        [HttpPost]
        public ActionResult PostAnswer(QASectionModel repsonses)
        {
            var _ctx = new fall_reliefEntities();
            //assessment object
            var assessment = _ctx.tbl_RiskAss_Session.Where(x => x.sessionID.Equals(repsonses.SessionID)).FirstOrDefault();

            //assessment section
            var AssSection = _ctx.tbl_RiskAss_QuestionSection.Where(x => x.AssessmentTypeID == assessment.AssessmentTypeID && x.SectionID == repsonses.SectionID).FirstOrDefault();

            //first question number
            var firstqno = _ctx.tbl_RiskAss_AssQuestion.Where(x => x.SectionNumber == AssSection.SectionNo && x.AssessmentTypeID == assessment.AssessmentTypeID).OrderBy(x => x.QuestionID).Take(1).Select(x => x.QuestionNumber).First();
            //no of questions
            var noOfQuestions = _ctx.tbl_RiskAss_Questions.Where(x => x.SectionID == AssSection.SectionID).Count();

            //loop through each question within the section to populate the answer into SQL server
            for (int t = firstqno; t < noOfQuestions + firstqno; t++)
            {

                //question objective
                var questionInfo = _ctx.tbl_RiskAss_AssQuestion.Where(x => x.AssessmentTypeID == assessment.AssessmentTypeID && x.SectionNumber == AssSection.SectionNo && x.QuestionNumber == t).FirstOrDefault();
                //selected response
                var selectedReponse = repsonses.Alist.Where(x => x.QuestionID == questionInfo.QuestionID).FirstOrDefault();

                //existing response
                var existingRecord = _ctx.tbl_RiskAss_AssessmentResponse.Where(x => x.AssessmentNo == assessment.AssessmentNo && x.AssQuestionID == questionInfo.QuestionID).FirstOrDefault();

                //insert reponses into database
                if (existingRecord == null) //if this is new entry
                {

                    var ReponseResult = (
                     from a in _ctx.tbl_RiskAss_ResponseChoice
                     join b in selectedReponse.UserSelectedID on a.ID equals b
                     select new { a.ID }).AsEnumerable()
                     .Select(x => new tbl_RiskAss_AssessmentResponse()
                     {
                         id = Guid.NewGuid(),
                         AssessmentNo = assessment.AssessmentNo,
                         AssQuestionID = questionInfo.QuestionID,
                         SectionID = AssSection.SectionID,
                         responseID = x.ID,
                         Answer = "Checked"
                     }).ToList();

                    _ctx.tbl_RiskAss_AssessmentResponse.AddRange(ReponseResult);
                    _ctx.SaveChanges();
                }

                //update existing record
                else
                {
                    var dbRecord = _ctx.tbl_RiskAss_AssessmentResponse.Find(existingRecord.id);

                    var ReponseResult = (
                     from a in _ctx.tbl_RiskAss_ResponseChoice
                     join b in selectedReponse.UserSelectedID on a.ID equals b
                     select new { a.ID, a.RiskScore }).AsEnumerable()
                     .Select(x => new tbl_RiskAss_AssessmentResponse()
                     {
                         responseID = x.ID,
                         Answer = "Checked"
                     }).FirstOrDefault();

                    dbRecord.responseID = ReponseResult.responseID;
                    dbRecord.Answer = ReponseResult.Answer;

                    _ctx.SaveChanges();

                }
            };

            //get the next question depending on the direction
            var nextSectionNumber = 1;

            if (repsonses.Direction.Equals("forward", StringComparison.CurrentCultureIgnoreCase))
            {
                nextSectionNumber = _ctx.tbl_RiskAss_QuestionSection.Where(x => x.AssessmentTypeID == repsonses.AssessmentTypeID
                && x.SectionID > repsonses.SectionID).OrderBy(x => x.SectionNo).Take(1).Select(x => x.SectionNo ?? default).First();

            }
            else
            {
                if (repsonses.Direction.Equals("backwards", StringComparison.CurrentCultureIgnoreCase))
                {
                    nextSectionNumber = _ctx.tbl_RiskAss_QuestionSection.Where(x => x.AssessmentTypeID == repsonses.AssessmentTypeID
                    && x.SectionID < repsonses.SectionID).OrderByDescending(x => x.SectionNo).Take(1).Select(x => x.SectionNo ?? default).FirstOrDefault();
                }
            }


            if (!repsonses.Direction.Equals("nextPage", StringComparison.CurrentCultureIgnoreCase))
            {
                return RedirectToAction("QuestionAssessment", new
                {
                    @SessionID = Session["SessionID"],
                    @secNo = nextSectionNumber
                });
            }
            return RedirectToAction("AssessmentResult", new
            {
                @SessionID = Session["SessionID"]
            });


        }


        public ActionResult AssessmentResult(Guid SessionID)
        {

            var _ctx = new fall_reliefEntities();

            var assessment = _ctx.tbl_RiskAss_Session.Where(x => x.sessionID.Equals(SessionID)).FirstOrDefault();
            var RiskLevelResult = _ctx.vw_RiskAss_RiskLevel.Where(x => x.AssessmentNo.Equals(assessment.AssessmentNo)).FirstOrDefault();

            var _model =
                  _ctx.vw_RiskAss_RiskLevel.Where(x => x.AssessmentNo.Equals(assessment.AssessmentNo))
                  .Select(x => new RiskLevelModel()
                  {
                      AssessmentNo = x.AssessmentNo,
                      AssessmentType = x.AssessmentType,
                      RiskList = _ctx.vw_RiskAss_RiskStatement.Where(y => y.AssessmentNo == assessment.AssessmentNo).Select(z => new RiskStatementModel()
                      {
                          RiskID = z.RiskID,
                          RiskName = z.Risk,
                          RiskStatement = z.Risk_Statement,
                          ImgDir = z.RiskImg

                      }).ToList()

                  }).FirstOrDefault();

            ViewBag.AssessmentType = assessment.tbl_RiskAss_Assessment.AssessmentType;
            ViewBag.RiskScore = RiskLevelResult.RiskScore;
            ViewBag.RiskLevel = RiskLevelResult.RiskLevel;
            ViewBag.AssessmentNo = assessment.AssessmentNo;

            return View(_model);
        }



        public ActionResult ActionPlan(Guid SessionID)
        {

            var _ctx = new fall_reliefEntities();

            var assessment = _ctx.tbl_RiskAss_Session.Where(x => x.sessionID.Equals(SessionID)).FirstOrDefault();
            var assessmentType = assessment.tbl_RiskAss_Assessment.AssessmentTypeID;


            ViewBag.AssessmentType = assessment.tbl_RiskAss_Assessment.AssessmentType;
            ViewBag.AssessmentNo = assessment.AssessmentNo;
            ViewBag.AssessmentTypeID = assessmentType;


            //Health Action Plan
            if (assessmentType == 1)
            {

                var _model =
                      _ctx.tbl_RiskAss_Session.Where(x => x.AssessmentNo.Equals(assessment.AssessmentNo))
                      .Select(x => new ActionPlanModel()
                      {
                          AssessmentNo = x.AssessmentNo,
                          AssessmentTypeID = 1,
                          Dlist = _ctx.vw_ActionPlan_HealthRisks.Where(y => y.AssessmentNo == assessment.AssessmentNo).Select(z => new HealthPlanModel
                          {
                              Risk = z.Risk,
                              Nlist = _ctx.vw_ActionPlan_DietSolution.Where(s => s.risk == z.Risk && s.AssessmentNo == z.AssessmentNo).Select(t => new NutrientModel
                              {
                                  Nutrient = t.Nutrient,
                                  Description = t.SolutionDesc,
                                  Flist = _ctx.vw_ActionPlan_DietSolutionFood.Where(u => u.risk == t.risk && u.AssessmentNo == t.AssessmentNo && u.Nutrient == t.Nutrient).Select(h => new FoodModel
                                  {
                                      Description = h.FoodDescription,
                                      FoodGroupName = h.FoodGroupName,
                                      ImgDir = h.imgDir
                                  }).ToList()
                              }).ToList(),
                              Elist = _ctx.vw_QuestionID_Activity.Where(k => k.AssessmentNo == z.AssessmentNo && k.Risk == z.Risk).Select(m => new ExerciseModel
                              {
                                  Excerise = m.Exercise_name,
                                  ImgDir = m.Exercise_image_link,
                                  Slist = _ctx.vw_QuestionID_Activity_Steps.Where(n => n.actUID == m.actUID).Select(f => new ExceriseSteps
                                  {
                                      StepNo = f.sequenceID,
                                      Description = f.Exercise_description
                                  }).ToList()


                              }).ToList()

                          })
                          .ToList()
                      }).FirstOrDefault();

                return View(_model);
            }
            //Home Safe Action Plan
            else
            {


                var _model =
                      _ctx.vw_ActionPlan_HomeSafety.Where(x => x.AssessmentNo.Equals(assessment.AssessmentNo))
                      .Select(x => new ActionPlanModel()
                      {
                          AssessmentNo = x.AssessmentNo,
                          AssessmentTypeID = 2,
                          HSList = _ctx.vw_ActionPlan_HomeSafety.Where(y => y.AssessmentNo == assessment.AssessmentNo).Select(z => new HomeSafetyActionPlanModel
                          {
                              Risk = z.Risk,
                              ActionRequired = z.Action,
                              Location = z.Location_Room,
                              Who = z.Who,
                              Status = z.Status,
                              HSimg = z.imgdir
                          }).ToList()

                      }).FirstOrDefault();

                return View(_model);

            }
        }

    }
}