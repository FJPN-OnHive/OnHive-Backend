using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OnHive.Core.Library.Entities.Events
{
    public class MauticIntegration : EntityBase
    {
        public string IntegrationAPIKey { get; set; } = string.Empty;

        public string MauticUrl { get; set; } = string.Empty;

        public string MauticClientId { get; set; } = string.Empty;

        public string MauticClientSecret { get; set; } = string.Empty;

        public string MauticAccessToken { get; set; } = string.Empty;
    }
}