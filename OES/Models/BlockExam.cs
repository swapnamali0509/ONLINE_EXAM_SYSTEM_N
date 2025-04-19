using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Web;

namespace OES.Models
{
    public class BlockExam
    {
        [Key]
        public int Id { get; set; }
        public int StudentId { get; set; }
        public int ExamId { get; set; }
    }
}