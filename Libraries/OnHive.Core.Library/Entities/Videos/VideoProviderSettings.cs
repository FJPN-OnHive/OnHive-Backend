using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EHive.Core.Library.Entities.Videos
{
    public class VideoProviderSettings : EntityBase
    {
        public string Provider { get; set; } = string.Empty;

        public string Environment { get; set; } = string.Empty;

        public string Url { get; set; } = string.Empty;

        public string Key { get; set; } = string.Empty;

        public string Secret { get; set; } = string.Empty;
    }
}