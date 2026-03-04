using OnHive.Core.Library.Entities.Users;

namespace OnHive.Core.Library.Entities.Payments
{
    public class PaymentCard : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public CardHolder CardHolder { get; set; } = new();

        public string CardNumber { get; set; } = string.Empty;

        public string ExpirationDate { get; set; } = string.Empty;

        public string SecurityCode { get; set; } = string.Empty;

        public string Issuer { get; set; } = string.Empty;
    }

    public class CardHolder
    {
        public string Name { get; set; } = string.Empty;

        public string IdentificationType { get; set; } = string.Empty;

        public string Identification { get; set; } = string.Empty;

        public string PhoneNumber { get; set; } = string.Empty;

        public string Email { get; set; } = string.Empty;

        public Address Address { get; set; } = new();
    }
}