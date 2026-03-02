using MongoDB.Bson.Serialization.Attributes;
using MongoDB.Bson.Serialization.IdGenerators;

namespace EHive.Core.Library.Entities
{
    public class EntityBase
    {
        [BsonId(IdGenerator = typeof(StringObjectIdGenerator))]
        public string Id { get; set; } = string.Empty;

        public string VId { get; set; } = string.Empty;

        public string TenantId { get; set; } = string.Empty;

        public bool IsActive { get; set; }

        public DateTime CreatedAt { get; set; }

        public string CreatedBy { get; set; } = string.Empty;

        public DateTime UpdatedAt { get; set; }

        public string UpdatedBy { get; set; } = string.Empty;

        public string Version { get; set; } = "1";

        public int VersionNumber { get; set; } = 1;

        public bool ActiveVersion { get; set; } = true;
    }
}