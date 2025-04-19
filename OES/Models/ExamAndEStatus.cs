using Questions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OES.Models
{
    public class ExamAndEStatus
    {
        public List<CreateExamInfo> CreateExamInfo { get; set; }
        public List<ExamStatus> ExamStatus { get; set; }
        public Student Student { get; set; }
    }
}