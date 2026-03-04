namespace OnHive.Core.Library.Entities.Teachers
{
    public class TeacherDiscipline
    {
        public string DisciplineId { get; set; } = string.Empty;

        public string Role { get; set; } = string.Empty;

        public List<string> MetaData { get; set; } = new();
    }
}