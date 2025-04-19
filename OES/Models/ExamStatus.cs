using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OES.Models
{
    public class ExamStatus
    {
        [Key]
        public int ExamStatusId { get; set; }
        public int BatchId {  get; set; }
        public int ExamId { get; set; }
        public int StudentId { get; set; }
        public int Marks { get; set; }

    }
}