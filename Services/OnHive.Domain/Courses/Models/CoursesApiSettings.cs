using System.ComponentModel.DataAnnotations;

namespace EHive.Courses.Domain.Models
{
    public class CoursesApiSettings
    {
        public string? CoursesAdminPermission { get; set; } = "courses_admin";
    }
}