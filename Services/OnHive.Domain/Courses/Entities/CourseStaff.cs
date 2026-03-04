using OnHive.Core.Library.Enums.Courses;

namespace OnHive.Core.Library.Entities.Courses
{
    public class CourseStaff
    {
        public CourseRoles Role { get; set; } = CourseRoles.Teacher;

        public string UserId { get; set; } = string.Empty;

        public string Observation { get; set; } = string.Empty;
    }
}