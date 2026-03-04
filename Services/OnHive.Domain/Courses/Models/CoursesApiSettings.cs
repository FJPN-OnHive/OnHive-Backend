using System.ComponentModel.DataAnnotations;

namespace OnHive.Courses.Domain.Models
{
    public class CoursesApiSettings
    {
        public string? CoursesAdminPermission { get; set; } = "courses_admin";
    }
}