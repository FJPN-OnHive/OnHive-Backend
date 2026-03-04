using System.ComponentModel.DataAnnotations;

namespace OnHive.Students.Domain.Models
{
    public class StudentsApiSettings
    {
        public string? StudentsAdminPermission { get; set; } = "students_admin";

        public int StudentCodeSize { get; set; } = 16;
    }
}