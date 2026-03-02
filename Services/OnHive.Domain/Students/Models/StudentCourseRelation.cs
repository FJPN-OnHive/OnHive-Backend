namespace EHive.Students.Domain.Models
{
    public class StudentCourseRelation
    {
        public string UserId { get; set; } = string.Empty;

        public string CourseId { get; set; } = string.Empty;

        public string ProductId { get; set; } = string.Empty;

        public int? Progress { get; set; } = 0;
    }
}