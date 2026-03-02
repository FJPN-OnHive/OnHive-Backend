using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EHive.Core.Library.Contracts.Events
{
    public class EventResumeDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("key")]
        [MaxLength(256)]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("origin")]
        [MaxLength(256)]
        public string Origin { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("date")]
        public DateTime Date { get; set; }

        [JsonPropertyName("tags")]
        public string? Tags { get; set; }
    }
}