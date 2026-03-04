using OnHive.Core.Library.Enums.Courses;
using System.Text.Json.Serialization;

namespace OnHive.Core.Library.Contracts.Courses
{
    public class CourseStaffDto
    {
        [JsonPropertyName("role")]
        public CourseRoles Role { get; set; } = CourseRoles.Teacher;

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("observation")]
        public string Observation { get; set; } = string.Empty;
    }
}