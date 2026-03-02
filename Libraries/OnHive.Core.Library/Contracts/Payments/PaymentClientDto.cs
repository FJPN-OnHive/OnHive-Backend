using EHive.Core.Library.Contracts.Users;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Payments
{
    public class PaymentClientDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("identificationType")]
        public string IdentificationType { get; set; } = string.Empty;

        [JsonPropertyName("identification")]
        public string Identification { get; set; } = string.Empty;

        [JsonPropertyName("phoneNumber")]
        public string PhoneNumber { get; set; } = string.Empty;

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public AddressDto Address { get; set; } = new();

        [JsonPropertyName("birthDate")]
        public string BirthDate { get; set; } = string.Empty;
    }
}