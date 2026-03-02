namespace EHive.Core.Library.Entities.Tenants
{
    public class Tenant : EntityBase
    {
        public string Name { get; set; } = string.Empty;

        public string Domain { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public string CNPJ { get; set; } = string.Empty;

        public string StateInscription { get; set; } = string.Empty;

        public string CityInscription { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string PostalCode { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string ApiKey { get; set; } = string.Empty;

        public string? Slug { get; set; }

        public List<Feature> Features { get; set; } = new();
    }
}