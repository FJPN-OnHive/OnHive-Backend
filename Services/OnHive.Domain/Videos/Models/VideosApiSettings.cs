namespace OnHive.Videos.Domain.Models
{
    public class VideosApiSettings
    {
        public string? VideosAdminPermission { get; set; } = "videos_admin";

        public string VideoProcessingQueueName { get; set; } = string.Empty;

        public string MuxApi { get; set; } = "https://api.mux.com";

        public string MuxUser { get; set; } = string.Empty;

        public string MuxPassword { get; set; } = string.Empty;

        public string MuxEnv { get; set; } = string.Empty;

        public string MuxSigningKeyId { get; set; } = string.Empty;

        public string MuxSigningSecretKey { get; set; } = string.Empty;

        public string MuxCors { get; set; } = string.Empty;

        public string CredentialsFile { get; set; } = string.Empty;

        public int VideoProcessingMaxMessages { get; set; } = 10;

        public int ChunkSizeMb { get; set; } = 256;

        public string? DataFolder { get; set; } = "data/";
    }
}