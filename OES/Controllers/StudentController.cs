using OES.Models;
using Questions.Models;
using Questions.Models.Exam;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Linq;
using System.Net.Mail;
using System.Net;
using System.Web;
using System.Web.Helpers;
using System.Web.Mvc;
using static System.Net.WebRequestMethods;
using System.Threading.Tasks;


namespace OES.Controllers
{
    public class StudentController : Controller
    {
        // GET: Student
        OnlineExamDbContext db = new OnlineExamDbContext();
        public ActionResult StudentDashboard(int studentId)
        {
            var studentInfo = db.Students.FirstOrDefault(e => e.StudentId == studentId);
            var stuBatch = db.AddStudentBatch.Where(e => e.StudentId == studentId).ToList();
            var batchIds = stuBatch.Select(e => e.BatchId).ToList();
            var batches = db.CreateBatch.Where(e => batchIds.Contains(e.BatchId)).ToList();
            var studentAndBatch = new StudentAndBatch
            {
                Student = studentInfo,
                Batches = batches
            };
            return View(studentAndBatch);
        }
        public ActionResult GoToBatch(int batchid,int studentid)
        {
            var examInfo = db.createExamInfo.Where(e=>e.BatchId == batchid).ToList();
            var ExamStatus = db.ExamStatus.Where(e=>e.StudentId == studentid).ToList();
            var student = db.Students.FirstOrDefault(e=>e.StudentId == studentid);
            ExamAndEStatus ex = new ExamAndEStatus()
            {
                CreateExamInfo = examInfo,
                ExamStatus = ExamStatus,
                Student = student,
            };
            return View(ex);
        }
        public ActionResult StartExam(int examid, int studentid)
        {
            var student = db.Students.FirstOrDefault(e => e.StudentId == studentid);
            var examInfo = db.createExamInfo.FirstOrDefault(e=>e.ExamId == examid);
            var questions = db.questions.Where(e => e.ExamId == examid).ToList();
            TempData["questions"] = questions;
            TempData["examInfo"] = examInfo;
            TempData["student"] = student;
            PassExamDetails passExamDetails = new PassExamDetails()
            {
                Student = student,
                CreateExamInfo = examInfo,
                Questions = questions
            };
            return View(passExamDetails);
        }
        [HttpPost]
        public ActionResult SubmitAnswers(FormCollection form)
        {

            var questions = TempData["questions"] as List<CreateQuestions>;
            var examInfo = TempData["examInfo"] as CreateExamInfo;
            var student = TempData["student"] as Student;

            int totalQuestions = questions.Count;
            List<int> allQuestions = Enumerable.Range(1, totalQuestions).ToList();

            List<QuestionAnswer> answers = new List<QuestionAnswer>();
            HashSet<int> answeredQuestions = new HashSet<int>();
             Task.Run(() =>
            {
                // Collect the answers from the form
                foreach (var key in form.AllKeys)
                {
                    if (key.Contains("answers"))
                    {
                        try
                        {
                            var questionNo = int.Parse(key.Split('[')[1].Split(']')[0]);
                            var selectedOption = form[key];

                            answers.Add(new QuestionAnswer
                            {
                                QuestionNo = questionNo,
                                SelectedOption = selectedOption
                            });
                            answeredQuestions.Add(questionNo);
                        }
                        catch (Exception)
                        {
                            continue;
                        }
                    }
                }

                // Add unanswered questions with "NA"
                var unansweredQuestions = allQuestions.Except(answeredQuestions).ToList();
                foreach (var questionNo in unansweredQuestions)
                {
                    answers.Add(new QuestionAnswer
                    {
                        QuestionNo = questionNo,
                        SelectedOption = "NA",
                    });
                }

                answers = answers.OrderBy(a => a.QuestionNo).ToList();

                // Count correct answers
                int countCoAns = 0;
                for (int i = 0; i < totalQuestions; i++)
                {
                    if (answers[i].SelectedOption == questions[i].CorrectOption)
                    {
                        countCoAns++;
                    }
                }

                // Calculate total marks
                int totalMarks = (int)Math.Round((examInfo.ExamMarks * countCoAns) / (double)totalQuestions);

                // Save exam status to database
                ExamStatus examStatus = new ExamStatus()
                {
                    BatchId = examInfo.BatchId,
                    ExamId = examInfo.ExamId,
                    StudentId = student.StudentId,
                    Marks = totalMarks
                };
                db.ExamStatus.Add(examStatus);
                db.SaveChanges();

                // Prepare email content with responsive design using enhanced styling
                string body = $@"
<html>
<head>
    <style>
        /* General styles */
        body {{
            font-family: Arial, sans-serif;
            line-height: 1.6;
            background-color: #f4f4f4;
            margin: 0;
            padding: 0;
        }}
        .container {{
            max-width: 600px;
            margin: 0 auto;
            background-color: #ffffff;
            border: 1px solid #ddd;
            border-radius: 8px;
            padding: 20px;
        }}
        .header {{
            text-align: center;
            padding: 20px;
            background-color: #007BFF;
            color: white;
            font-size: 24px;
            border-radius: 8px 8px 0 0;
        }}
        .content {{
            padding: 20px;
            color: #333;
        }}
        .question {{
            background-color: #f9f9f9;
            padding: 15px;
            border-radius: 5px;
            margin-bottom: 10px;
        }}
        .question h4 {{
            margin: 0;
            font-size: 18px;
        }}
        .answer, .correct-answer {{
            margin-top: 10px;
            font-size: 16px;
            padding: 5px;
            background-color: #f1f1f1;
            border-radius: 5px;
        }}
        .answer {{
            background-color: #d4edda; /* Light green */
        }}
        .correct-answer {{
            background-color: #f8d7da; /* Light red */
        }}
        .footer {{
            text-align: center;
            font-size: 12px;
            color: #888;
            margin-top: 20px;
        }}
        @media only screen and (max-width: 600px) {{
            .container {{
                padding: 15px;
            }}
            .header {{
                font-size: 20px;
            }}
        }}
    </style>
</head>
<body>
    <div class='container'>
        <div class='header'>
            Your Exam Results
        </div>
        <div class='content'>
            <p>Hello <strong>{student.StuName}</strong>,</p>
            <p>Your exam results are as follows:</p>
            <p><strong>Total Marks:</strong> {totalMarks} out of {examInfo.ExamMarks}</p>

            <h3>Results Breakdown:</h3>";

                for (int i = 0; i < questions.Count; i++)
                {
                    body += $@"
            <div class='question'>
                <h4>Q{i + 1}: {questions[i].Question}</h4>
                <div class='answer'>
                    <strong>Your Answer:</strong> {answers[i].SelectedOption}
                </div>
                <div class='correct-answer'>
                    <strong>Correct Answer:</strong> {questions[i].CorrectOption}
                </div>
            </div>
    ";
                }

                body += $@"
        </div>
        <div class='footer'>
            <p>Best regards,<br>Your Team Examino </p>
        </div>
    </div>
</body>
</html>
";

                // Send the email
                try
                {
                    MailMessage message = new MailMessage
                    {
                        From = new MailAddress("teamexamino@gmail.com"),
                        Subject = "Exam Result",
                        IsBodyHtml = true, 
                        Body = body
                    };

                    message.To.Add(student.StuEmail);

                    SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
                    {
                        Credentials = new NetworkCredential("teamexamino@gmail.com", "xina qygq hkpr xnqa"), 
                        EnableSsl = true
                    };

                    client.Send(message);
                }
                catch (Exception ex)
                {
                    // Log or handle exception
                    Console.WriteLine("Error sending exam result email: " + ex.Message);
                }
            });

            return RedirectToAction("StudentDashboard", new {studentid = student.StudentId});
        }
        [HttpPost]
        public JsonResult CheckStudentExamBlock(int studentId, int examId)
        {
            bool result = false;
            var checkStudent = db.blockExams.FirstOrDefault(e => e.StudentId == studentId && e.ExamId == examId);
            if (checkStudent == null)
            {
                BlockExam blockExam = new BlockExam();
                blockExam.StudentId = studentId;
                blockExam.ExamId = examId;
                db.blockExams.Add(blockExam);
                db.SaveChanges();
                result = true;
            }  
            return Json(new { success = result });
        }
    }
}