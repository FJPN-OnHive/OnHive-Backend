using EHive.Core.Library.Enums.Courses;

namespace EHive.Core.Library.Entities.Courses
{
    public class CourseStaff
    {
        public CourseRoles Role { get; set; } = CourseRoles.Teacher;

        public string UserId { get; set; } = string.Empty;

        public string Observation { get; set; } = string.Empty;
    }
}