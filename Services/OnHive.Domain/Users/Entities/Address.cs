namespace EHive.Core.Library.Entities.Users
{
    public class Address
    {
        public string Name { get; set; } = string.Empty;

        public string AddressLines { get; set; } = string.Empty;

        public string Number { get; set; } = string.Empty;

        public string Complement { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string ZipCode { get; set; } = string.Empty;

        public bool IsMainAddress { get; set; }
    }
}