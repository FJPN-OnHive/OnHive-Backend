using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnHive.Core.Library.Entities.Students
{
    public class StudentReport : EntityBase
    {
        public string ReportName { get; set; } = string.Empty;

        public DateTime ReportDate { get; set; }

        public string FileUrl { get; set; } = string.Empty;
    }
}