using EHive.Core.Library.Enums.Videos;

namespace EHive.Core.Library.Entities.Videos
{
    public class Video : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public VideoStatus Status { get; set; } = VideoStatus.Processing;

        public string VideoSource { get; set; } = string.Empty;

        public string FileName { get; set; } = string.Empty;

        public string SourceId { get; set; } = string.Empty;

        public string VideoId { get; set; } = string.Empty;

        public string SourceFileUrl { get; set; } = string.Empty;

        public string VideoUrl { get; set; } = string.Empty;

        public string EmbeddedVideo { get; set; } = string.Empty;

        public string ThumbnailUrl { get; set; } = string.Empty;

        public List<string> Categories { get; set; } = [];

        public List<string> Tags { get; set; } = [];
    }
}