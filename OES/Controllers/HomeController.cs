using System;
using System.Net.Mail;
using System.Net;
using System.Web.Mvc;
using OES.Models;
using Questions.Models;
using System.Linq;

namespace OES.Controllers
{
    public class HomeController : Controller
    {
        OnlineExamDbContext db = new OnlineExamDbContext();
        // GET: Home
        public ActionResult Index()
        {
            return View();
        }
        [HttpPost]
        public ActionResult Login(string Email, string Password, string Role)
        {
            if (Role == "Teacher")
            {
                var teacher = db.Teachers.FirstOrDefault(e => e.TeacherEmail == Email && e.TeacherPassword == Password);
                if (teacher != null)
                {
                    int teacherId = teacher.TeacherId;
                    return RedirectToAction("TeacherDashboard", "Teacher", new { teacherId = teacherId });
                }
                else
                {
                    TempData["Message"] = "Invalid Teacher Credentials.";
                }
            }
            else if (Role == "Student")
            {
                if (Role == "Student")
                {
                    var student = db.Students.FirstOrDefault(e => e.StuEmail == Email && e.StuPassword == Password);
                    if (student != null)
                    {
                        int studentId = student.StudentId;
                        return RedirectToAction("StudentDashboard", "Student", new { studentId = studentId });
                    }
                    else
                    {
                        TempData["Message"] = "Invalid Student Credentials.";
                    }
                }
            }
            else
            {
                // Handle admin login
            }
            return RedirectToAction("Index");
        }
             
        public ActionResult StudentSignup()
        {
            Student student = new Student()
            {
                StuName = (Session["student"] as Student).StuName,
                StuEmail = (Session["student"] as Student).StuEmail,
                StuUserName = (Session["student"] as Student).StuUserName,
                StuPassword = (Session["student"] as Student).StuPassword
            };            
            db.Students.Add(student);
            db.SaveChanges();
            TempData["Message"] = "Registration successful!";
            return View("Index");
        }       

        public ActionResult TeacherSignup()
        {
            Teacher teacher = new Teacher()
            {
                TeacherName = (Session["teacher"] as Teacher).TeacherName,
                TeacherEmail = (Session["teacher"] as Teacher).TeacherEmail,
                InstitudeName = (Session["teacher"] as Teacher).InstitudeName,
                TeacherPassword = (Session["teacher"] as Teacher).TeacherPassword
            };
            db.Teachers.Add(teacher);
            db.SaveChanges();
            TempData["Message"] = "Registration successful!";
            return View("index");
        }
           
        [HttpPost]
        public JsonResult CreateOtpStu(string StuName, string stuEmail, string StuUserName, string StuPass, string RegRole)
        {
            Student student = new Student();
            student.StuName = StuName;
            student.StuEmail = stuEmail;
            student.StuUserName = StuUserName;
            student.StuPassword = StuPass;
            Session["student"]= student;
            Session["regRole"] = RegRole;
            try
            {
                // Generate OTP
                Random random = new Random();
                string otp = random.Next(100000, 999999).ToString();

                // Store OTP in Session
                Session["GeneratedOtp"] = otp;
                Session["Email"] = stuEmail;

                // Send OTP Email
                //MailMessage message = new MailMessage
                //{
                //    From = new MailAddress("tusharsutar799@gmail.com"),
                //    Subject = "Your OTP Code",
                //    Body = $"Hello {FullName},\n\nYour OTP is: {otp}. Please use this code to verify your identity.\n\nThank you!",
                //};
                MailMessage message = new MailMessage
                {
                    From = new MailAddress("teamexamino@gmail.com"),
                    Subject = "Your OTP Code",
                    Body = $@"
                        <html>
                        <head>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                    line-height: 1.6;
                                    background-color: #f9f9f9;
                                    color: #333;
                                    padding: 20px;
                                }}
                                .container {{
                                    max-width: 600px;
                                    margin: 0 auto;
                                    background-color: #fff;
                                    border: 1px solid #ddd;
                                    border-radius: 8px;
                                    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                                    padding: 20px;
                                }}
                                .header {{
                                    text-align: center;
                                    padding: 10px 0;
                                    background-color: #4CAF50;
                                    color: white;
                                    font-size: 20px;
                                    border-radius: 8px 8px 0 0;
                                }}
                                .content {{
                                    text-align: center;
                                    margin: 20px 0;
                                }}
                                .otp {{
                                    display: inline-block;
                                    padding: 10px 20px;
                                    background-color: #4CAF50;
                                    color: white;
                                    font-size: 24px;
                                    font-weight: bold;
                                    border-radius: 5px;
                                    margin: 10px 0;
                                }}
                                .footer {{
                                    text-align: center;
                                    font-size: 12px;
                                    color: #999;
                                    margin-top: 20px;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    Your OTP Code
                                </div>
                                <div class='content'>
                                    <p>Hello <strong>{StuName}</strong>,</p>
                                    <p>Your OTP is:</p>
                                    <div class='otp'>{otp}</div>
                                    <p>Please use this code to verify your identity.</p>
                                    <p>Thank you!</p>
                                </div>
                                <div class='footer'>
                                    <p>Team Examino</p>
                                </div>
                            </div>
                        </body>
                        </html>",
                    IsBodyHtml = true // Enable HTML in the email body
                };

                message.To.Add(stuEmail);

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("teamexamino@gmail.com", "xina qygq hkpr xnqa"),
                    EnableSsl = true
                };
                client.Send(message);

                return Json(new { success = true, message = "OTP sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to send OTP: " + ex.Message });
            }
        }
        [HttpPost]
        public JsonResult CreateOtpTea(string TeaName, string TeaEmail,string TeaInstitute, string TeaPassword, string RegRole)
        {
            Teacher tea = new Teacher()
            {
                TeacherName = TeaName,
                TeacherEmail = TeaEmail,
                InstitudeName = TeaInstitute,
                TeacherPassword = TeaPassword                
            };
            Session["teacher"] = tea;
            Session["regRole"] = RegRole;
            try
            {
                // Generate OTP
                Random random = new Random();
                string otp = random.Next(100000, 999999).ToString();

                // Store OTP in Session
                Session["GeneratedOtp"] = otp;
                Session["Email"] = TeaEmail;

                // Send OTP Email
                //MailMessage message = new MailMessage
                //{
                //    From = new MailAddress("tusharsutar799@gmail.com"),
                //    Subject = "Your OTP Code",
                //    Body = $"Hello {FullName},\n\nYour OTP is: {otp}. Please use this code to verify your identity.\n\nThank you!",
                //};
                MailMessage message = new MailMessage
                {
                    From = new MailAddress("teamexamino@gmail.com"),
                    Subject = "Your OTP Code",
                    Body = $@"
                        <html>
                        <head>
                            <style>
                                body {{
                                    font-family: Arial, sans-serif;
                                    line-height: 1.6;
                                    background-color: #f9f9f9;
                                    color: #333;
                                    padding: 20px;
                                }}
                                .container {{
                                    max-width: 600px;
                                    margin: 0 auto;
                                    background-color: #fff;
                                    border: 1px solid #ddd;
                                    border-radius: 8px;
                                    box-shadow: 0 2px 4px rgba(0, 0, 0, 0.1);
                                    padding: 20px;
                                }}
                                .header {{
                                    text-align: center;
                                    padding: 10px 0;
                                    background-color: #4CAF50;
                                    color: white;
                                    font-size: 20px;
                                    border-radius: 8px 8px 0 0;
                                }}
                                .content {{
                                    text-align: center;
                                    margin: 20px 0;
                                }}
                                .otp {{
                                    display: inline-block;
                                    padding: 10px 20px;
                                    background-color: #4CAF50;
                                    color: white;
                                    font-size: 24px;
                                    font-weight: bold;
                                    border-radius: 5px;
                                    margin: 10px 0;
                                }}
                                .footer {{
                                    text-align: center;
                                    font-size: 12px;
                                    color: #999;
                                    margin-top: 20px;
                                }}
                            </style>
                        </head>
                        <body>
                            <div class='container'>
                                <div class='header'>
                                    Your OTP Code
                                </div>
                                <div class='content'>
                                    <p>Hello <strong>{TeaName}</strong>,</p>
                                    <p>Your OTP is:</p>
                                    <div class='otp'>{otp}</div>
                                    <p>Please use this code to verify your identity.</p>
                                    <p>Thank you!</p>
                                </div>
                                <div class='footer'>
                                    <p>Team Examino</p>
                                </div>
                            </div>
                        </body>
                        </html>",
                    IsBodyHtml = true // Enable HTML in the email body
                };

                message.To.Add(TeaEmail);

                SmtpClient client = new SmtpClient("smtp.gmail.com", 587)
                {
                    Credentials = new NetworkCredential("teamexamino@gmail.com", "xina qygq hkpr xnqa"),
                    EnableSsl = true
                };
                client.Send(message);

                return Json(new { success = true, message = "OTP sent successfully!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = "Failed to send OTP: " + ex.Message });
            }
        }

        [HttpPost]
        public ActionResult VerifyOtp(string[] Otp)
        {
            string enteredOtp = string.Join("", Otp);
            string storedOtp = Session["GeneratedOtp"] as string;

            if (!string.IsNullOrEmpty(storedOtp) && enteredOtp == storedOtp)
            {
                //TempData["Message"] = "OTP verified successfully!";
                if(Session["regRole"] as string == "studentt")
                {
                    return RedirectToAction("StudentSignup");
                }
                else if(Session["regRole"] as string == "teacherr")
                {
                    return RedirectToAction("TeacherSignup");
                }
                else
                {
                    return RedirectToAction("Index");
                }
                
            }
            else
            {
                TempData["Message"] = "Invalid OTP. Please try again.";
                return RedirectToAction("Index");
            }
        }


    }
}
