using Questions.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OES.Models
{
    public class ExamResultToView
    {
        public CreateBatch BatchDetails { get; set; }
        public List<Student> BatchStudents { get; set; }
        public List<CreateExamInfo> ExamDetails { get; set; }
        public List<ExamStatus> ExamResults { get; set; }
    }
}