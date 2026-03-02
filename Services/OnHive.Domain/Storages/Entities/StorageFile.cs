namespace EHive.Core.Library.Entities.Storages
{
    public class StorageFile : EntityBase
    {
        public string FileId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public string Type { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string FileUrl { get; set; } = string.Empty;

        public string TargetFolder { get; set; } = "files";

        public bool Public { get; set; } = true;

        public List<string> Categories { get; set; } = [];

        public List<string> Tags { get; set; } = [];
    }
}