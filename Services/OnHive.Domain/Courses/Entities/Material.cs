using OnHive.Core.Library.Enums.Courses;

namespace OnHive.Core.Library.Entities.Courses
{
    public class Material
    {
        public MaterialTypes Type { get; set; } = MaterialTypes.Other;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public List<string> MetaData { get; set; } = new();
    }
}