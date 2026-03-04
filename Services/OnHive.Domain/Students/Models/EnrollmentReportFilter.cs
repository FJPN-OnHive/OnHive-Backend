namespace OnHive.Students.Domain.Models
{
    public class EnrollmentReportFilter
    {
        public DateTime InitialDate { get; set; } = DateTime.MinValue;

        public DateTime FinalDate { get; set; } = DateTime.MaxValue;

        public List<string> Courses { get; set; } = [];
    }
}