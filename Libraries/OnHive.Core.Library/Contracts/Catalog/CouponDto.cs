using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Catalog
{
    public class CouponDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        [Required]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("key")]
        [Required]
        public string Key { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("discount")]
        [Required]
        public double Discount { get; set; } = 0;

        [JsonPropertyName("isPercentage")]
        [Required]
        public bool IsPercentage { get; set; } = false;

        [JsonPropertyName("quantity")]
        public int Quantity { get; set; } = 0;

        [JsonPropertyName("usesPerUser")]
        public int UsesPerUser { get; set; } = 1;

        [JsonPropertyName("products")]
        public List<string> Products { get; set; } = new();

        [JsonPropertyName("categories")]
        public List<string> Categories { get; set; } = new();

        [JsonPropertyName("startDate")]
        [Required]
        public DateTime StartDate { get; set; } = DateTime.Now;

        [JsonPropertyName("endDate")]
        [Required]
        public DateTime EndDate { get; set; } = DateTime.Now.AddMonths(1);

        [JsonPropertyName("totalUses")]
        public int TotalUses { get; set; } = 0;
    }
}