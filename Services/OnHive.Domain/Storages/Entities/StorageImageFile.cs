namespace OnHive.Core.Library.Entities.Storages
{
    public class StorageImageFile : EntityBase
    {
        public string ImageId { get; set; } = string.Empty;

        public string Name { get; set; } = string.Empty;

        public string AltText { get; set; } = string.Empty;

        public string Subtitle { get; set; } = string.Empty;

        public string OriginalFileName { get; set; } = string.Empty;

        public string HiResImageUrl { get; set; } = string.Empty;

        public string MidResImageUrl { get; set; } = string.Empty;

        public string LowResImageUrl { get; set; } = string.Empty;

        public bool Public { get; set; } = true;

        public List<string> Categories { get; set; } = [];

        public List<string> Tags { get; set; } = [];
    }
}