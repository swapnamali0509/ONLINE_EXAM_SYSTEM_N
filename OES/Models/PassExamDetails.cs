using Questions.Models;
using Questions.Models.Exam;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OES.Models
{
    public class PassExamDetails
    {
        public Student Student { get; set; }
        public CreateExamInfo CreateExamInfo { get; set; }
        public List<CreateQuestions> Questions { get; set; }
    }
}