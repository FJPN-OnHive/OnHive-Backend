namespace OnHive.Core.Library.Entities.Teachers
{
    public class Teacher : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string Code { get; set; } = string.Empty;

        public string Title { get; set; } = string.Empty;

        public List<TeacherDiscipline> Disciplines { get; set; } = new();

        public List<string> MetaData { get; set; } = new();
    }
}