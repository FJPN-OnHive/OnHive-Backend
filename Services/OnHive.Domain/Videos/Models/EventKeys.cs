using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnHive.Videos.Domain.Models
{
    public static class EventKeys
    {
        public const string VideoUploaded = "VideoUploaded";
        public const string VideoProcessing = "VideoProcessing";
        public const string VideoProcessed = "VideoProcessed";
        public const string VideoError = "VideoError";
    }
}