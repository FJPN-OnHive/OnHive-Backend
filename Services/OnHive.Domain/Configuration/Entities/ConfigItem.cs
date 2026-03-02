using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace EHive.Core.Library.Entities.Configuration
{
    public class ConfigItem : EntityBase
    {
        public string Key { get; set; } = string.Empty;

        public string Value { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;
    }
}