namespace OnHive.Core.Library.Entities.Payments
{
    public class TestCard : EntityBase
    {
        public string CardNumber { get; set; } = string.Empty;

        public string CardHolderName { get; set; } = string.Empty;

        public string CardHolderEmail { get; set; } = string.Empty;

        public string CardHolderIdentificationNumber { get; set; } = string.Empty;

        public string CardHolderIdentificationType { get; set; } = string.Empty;

        public string ExpirationDate { get; set; } = string.Empty;

        public string SecurityCode { get; set; } = string.Empty;

        public bool Accept { get; set; }
    }
}