using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace OES.Models
{
    public class StudentAndBatch
    {
        public Student Student { get; set; }
        public List<CreateBatch> Batches { get; set; }

    }
}