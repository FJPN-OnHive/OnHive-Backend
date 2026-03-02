using System.ComponentModel.DataAnnotations;

namespace EHive.Storages.Domain.Models
{
    public class StoragesApiSettings
    {
        public string? StoragesAdminPermission { get; set; } = "storages_admin";

        public int LowResWidth { get; set; } = 100;

        public int MidResWidth { get; set; } = 500;

        public string? DataFolder { get; set; } = "data/";

        public string? BucketRegion { get; set; }

        public string? BucketName { get; set; }

        public string BucketKeyId { get; set; } = string.Empty;

        public string BucketSecret { get; set; } = string.Empty;

        public string CredentialsFile { get; set; } = string.Empty;

        public string BaseUrl { get; set; } = string.Empty;

        public List<string> ValidFileTypes { get; set; } = [".doc", ".docx", ".odt", ".fodt", ".pdf", ".epub", ".mobi", ".ppt", ".pptx", ".zip", ".mp3", ".wav", ".csv", ".xlsx"];

        public bool GenerateLowRes { get; set; } = false;

        public bool GenerateMidRes { get; set; } = false;

        public int HiResWidthLimit { get; set; } = 1024;
    }
}