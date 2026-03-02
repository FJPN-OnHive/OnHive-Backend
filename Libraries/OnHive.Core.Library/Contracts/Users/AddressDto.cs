using EHive.Core.Library.Attributes;
using EHive.Core.Library.Validations.Users;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Users
{
    public class AddressDto
    {
        [JsonPropertyName("name")]
        [MaxLength(256)]
        [Required]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("addressLines")]
        [MaxLength(256)]
        [Required]
        public string AddressLines { get; set; } = string.Empty;

        [JsonPropertyName("number")]
        [MaxLength(256)]
        [Required]
        public string Number { get; set; } = string.Empty;

        [JsonPropertyName("complement")]
        [MaxLength(256)]
        public string Complement { get; set; } = string.Empty;

        [JsonPropertyName("district")]
        [MaxLength(256)]
        [Required]
        public string District { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        [MaxLength(256)]
        [Required]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        [ValidateObject]
        [Required]
        public StateDto State { get; set; } = new();

        [JsonPropertyName("country")]
        [ValidateObject]
        [Required]
        public CountryDto Country { get; set; } = new();

        [JsonPropertyName("zipCode")]
        [MaxLength(256)]
        [Required]
        public string ZipCode { get; set; } = string.Empty;

        [JsonPropertyName("isMainAddress")]
        [Required]
        public bool IsMainAddress { get; set; }

        public override string ToString()
        {
            return $"{AddressLines}, {Number}, {Complement}, {District}, {City}, {State}";
        }
    }

    public class StateDto
    {
        [JsonPropertyName("name")]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("uf")]
        [ValidValues(typeof(UFValidValues))]
        [Required]
        public string Code { get; set; } = string.Empty;
    }

    public class CountryDto
    {
        [JsonPropertyName("name")]
        [MaxLength(256)]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("code")]
        [ValidValues(typeof(CountryValidValues))]
        [Required]
        public string Code { get; set; } = string.Empty;
    }
}