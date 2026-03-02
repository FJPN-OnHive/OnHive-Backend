using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace EHive.PaymentsCielo.Domain.Contracts
{
    public abstract class CieloRequestBase
    {
        [JsonPropertyName("MerchantOrderId")]
        public string MerchantOrderId { get; set; }

        [JsonPropertyName("Customer")]
        public Customer Customer { get; set; }
    }

    public class CieloCreditCardRequest : CieloRequestBase
    {
        [JsonPropertyName("Payment")]
        public PaymentCreditCard Payment { get; set; }
    }

    public class CieloCreditCardResponse : CieloRequestBase
    {
        [JsonPropertyName("Payment")]
        public CreditCardResponse Payment { get; set; }
    }

    public class CieloDebitCardRequest : CieloRequestBase
    {
        [JsonPropertyName("Payment")]
        public PaymentDebitCard Payment { get; set; }
    }

    public class CieloDebitCardResponse : CieloRequestBase
    {
        [JsonPropertyName("Payment")]
        public DebitCardResponse Payment { get; set; }
    }

    public class CieloBankSlipRequest : CieloRequestBase
    {
        [JsonPropertyName("Payment")]
        public PaymentBankSlip Payment { get; set; }
    }

    public class CieloBankSlipResponse : CieloRequestBase
    {
        [JsonPropertyName("Payment")]
        public BankSlipResponse Payment { get; set; }
    }

    public class CieloPixRequest : CieloRequestBase
    {
        [JsonPropertyName("Payment")]
        public PaymentPix Payment { get; set; }
    }

    public class CieloPixResponse : CieloRequestBase
    {
        [JsonPropertyName("Payment")]
        public PixResponse Payment { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("Street")]
        public string Street { get; set; }

        [JsonPropertyName("Number")]
        public string Number { get; set; }

        [JsonPropertyName("Complement")]
        public string Complement { get; set; }

        [JsonPropertyName("ZipCode")]
        public string ZipCode { get; set; }

        [JsonPropertyName("City")]
        public string City { get; set; }

        [JsonPropertyName("State")]
        public string State { get; set; }

        [JsonPropertyName("Country")]
        public string Country { get; set; }

        [JsonPropertyName("District")]
        public string District { get; set; }
    }

    public class CreditCard
    {
        [JsonPropertyName("CardNumber")]
        public string CardNumber { get; set; }

        [JsonPropertyName("Holder")]
        public string Holder { get; set; }

        [JsonPropertyName("ExpirationDate")]
        public string ExpirationDate { get; set; }

        [JsonPropertyName("SecurityCode")]
        public string SecurityCode { get; set; }

        [JsonPropertyName("SaveCard")]
        public bool SaveCard { get; set; }

        [JsonPropertyName("Brand")]
        public string Brand { get; set; }
    }

    public class DebitCard
    {
        [JsonPropertyName("CardNumber")]
        public string CardNumber { get; set; }

        [JsonPropertyName("Holder")]
        public string Holder { get; set; }

        [JsonPropertyName("ExpirationDate")]
        public string ExpirationDate { get; set; }

        [JsonPropertyName("SecurityCode")]
        public string SecurityCode { get; set; }

        [JsonPropertyName("Brand")]
        public string Brand { get; set; }
    }

    public class Customer
    {
        [JsonPropertyName("Name")]
        public string Name { get; set; }

        [JsonPropertyName("Identity")]
        public string Identity { get; set; }

        [JsonPropertyName("IdentityType")]
        public string IdentityType { get; set; }

        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [JsonPropertyName("Birthdate")]
        public string Birthdate { get; set; }

        [JsonPropertyName("Address")]
        public Address? Address { get; set; }

        [JsonPropertyName("DeliveryAddress")]
        public DeliveryAddress? DeliveryAddress { get; set; }
    }

    public class DeliveryAddress
    {
        [JsonPropertyName("Street")]
        public string Street { get; set; }

        [JsonPropertyName("Number")]
        public string Number { get; set; }

        [JsonPropertyName("Complement")]
        public string Complement { get; set; }

        [JsonPropertyName("ZipCode")]
        public string ZipCode { get; set; }

        [JsonPropertyName("City")]
        public string City { get; set; }

        [JsonPropertyName("State")]
        public string State { get; set; }

        [JsonPropertyName("Country")]
        public string Country { get; set; }
    }

    public class PaymentCreditCard
    {
        [JsonPropertyName("Type")]
        public string Type { get; set; } = "CreditCard";

        [JsonPropertyName("Amount")]
        public int Amount { get; set; }

        [JsonPropertyName("Currency")]
        public string Currency { get; set; }

        [JsonPropertyName("Country")]
        public string Country { get; set; }

        [JsonPropertyName("Provider")]
        public string Provider { get; set; }

        [JsonPropertyName("ServiceTaxAmount")]
        public int ServiceTaxAmount { get; set; }

        [JsonPropertyName("Installments")]
        public int Installments { get; set; }

        [JsonPropertyName("Capture")]
        public bool Capture { get; set; }

        [JsonPropertyName("Authenticate")]
        public bool Authenticate { get; set; }

        [JsonPropertyName("Recurrent")]
        public bool Recurrent { get; set; }

        [JsonPropertyName("SoftDescriptor")]
        public string SoftDescriptor { get; set; }

        [JsonPropertyName("CreditCard")]
        public CreditCard CreditCard { get; set; }
    }

    public class CreditCardResponse : PaymentCreditCard
    {
        [JsonPropertyName("Tid")]
        public string? Tid { get; set; }

        [JsonPropertyName("ProofOfSale")]
        public string? ProofOfSale { get; set; }

        [JsonPropertyName("AuthorizationCode")]
        public string? AuthorizationCode { get; set; }

        [JsonPropertyName("IsQrCode")]
        public bool? IsQrCode { get; set; }

        [JsonPropertyName("ReceivedDate")]
        public string? ReceivedDate { get; set; }

        [JsonPropertyName("Status")]
        public int? Status { get; set; }

        [JsonPropertyName("IsSplitted")]
        public bool? IsSplitted { get; set; }

        [JsonPropertyName("ReturnMessage")]
        public string? ReturnMessage { get; set; }

        [JsonPropertyName("ReturnCode")]
        public string? ReturnCode { get; set; }

        [JsonPropertyName("PaymentId")]
        public string? PaymentId { get; set; }

        [JsonPropertyName("Links")]
        public List<Link>? Links { get; set; }
    }

    public class PaymentDebitCard
    {
        [JsonPropertyName("Type")]
        public string Type { get; set; } = "DebitCard";

        [JsonPropertyName("Amount")]
        public int Amount { get; set; }

        [JsonPropertyName("Provider")]
        public string Provider { get; set; }

        [JsonPropertyName("ReturnUrl")]
        public string ReturnUrl { get; set; }

        [JsonPropertyName("DebitCard")]
        public DebitCard DebitCard { get; set; }

        [JsonPropertyName("InitiatedTransactionIndicator")]
        public InitiatedTransactionIndicator InitiatedTransactionIndicator { get; set; }
    }

    public class InitiatedTransactionIndicator
    {
        [JsonPropertyName("Category")]
        public string Category { get; set; } = "C1";

        [JsonPropertyName("Subcategory")]
        public string Subcategory { get; set; } = "Standingorder";
    }

    public class DebitCardResponse : PaymentDebitCard
    {
        [JsonPropertyName("Tid")]
        public string Tid { get; set; }

        [JsonPropertyName("ProofOfSale")]
        public string ProofOfSale { get; set; }

        [JsonPropertyName("AuthorizationCode")]
        public string AuthorizationCode { get; set; }

        [JsonPropertyName("ReceivedDate")]
        public string ReceivedDate { get; set; }

        [JsonPropertyName("Status")]
        public int Status { get; set; }

        [JsonPropertyName("ReturnMessage")]
        public string ReturnMessage { get; set; }

        [JsonPropertyName("ReturnCode")]
        public string ReturnCode { get; set; }

        [JsonPropertyName("PaymentId")]
        public string PaymentId { get; set; }

        [JsonPropertyName("Links")]
        public List<Link> Links { get; set; }
    }

    public class PaymentBankSlip
    {
        [JsonPropertyName("Type")]
        public string Type { get; set; } = "Boleto";

        [JsonPropertyName("Amount")]
        public int Amount { get; set; }

        [JsonPropertyName("Provider")]
        public string Provider { get; set; }

        [JsonPropertyName("Address")]
        public string Address { get; set; }

        [JsonPropertyName("BoletoNumber")]
        public string BoletoNumber { get; set; }

        [JsonPropertyName("Assignor")]
        public string Assignor { get; set; }

        [JsonPropertyName("Demonstrative")]
        public string Demonstrative { get; set; }

        [JsonPropertyName("ExpirationDate")]
        public string ExpirationDate { get; set; }

        [JsonPropertyName("Identification")]
        public string Identification { get; set; }

        [JsonPropertyName("Instructions")]
        public string Instructions { get; set; }
    }

    public class PaymentPix
    {
        [JsonPropertyName("Type")]
        public string Type { get; set; } = "Pix";

        [JsonPropertyName("Amount")]
        public int Amount { get; set; }
    }

    public class BankSlipResponse : PaymentBankSlip
    {
        [JsonPropertyName("Url")]
        public string Url { get; set; }

        [JsonPropertyName("BarCodeNumber")]
        public string BarCodeNumber { get; set; }

        [JsonPropertyName("DigitableLine")]
        public string DigitableLine { get; set; }

        [JsonPropertyName("Bank")]
        public int Bank { get; set; }

        [JsonPropertyName("ReceivedDate")]
        public string ReceivedDate { get; set; }

        [JsonPropertyName("Status")]
        public int Status { get; set; }

        [JsonPropertyName("IsSplitted")]
        public bool IsSplitted { get; set; }

        [JsonPropertyName("PaymentId")]
        public string PaymentId { get; set; }

        [JsonPropertyName("Currency")]
        public string Currency { get; set; }

        [JsonPropertyName("Country")]
        public string Country { get; set; }

        [JsonPropertyName("Links")]
        public List<Link> Links { get; set; }
    }

    public class PixResponse : PaymentPix
    {
        [JsonPropertyName("PaymentId")]
        public string PaymentId { get; set; }

        [JsonPropertyName("AcquirerTransactionId")]
        public string AcquirerTransactionId { get; set; }

        [JsonPropertyName("ProofOfSale")]
        public string ProofOfSale { get; set; }

        [JsonPropertyName("QrcodeBase64Image")]
        public string QrcodeBase64Image { get; set; }

        [JsonPropertyName("QrCodeString")]
        public string QrCodeString { get; set; }

        [JsonPropertyName("ReceivedDate")]
        public DateTime ReceivedDate { get; set; }

        [JsonPropertyName("Status")]
        public int Status { get; set; }

        [JsonPropertyName("ReturnCode")]
        public string ReturnCode { get; set; }

        [JsonPropertyName("ReturnMessage")]
        public string ReturnMessage { get; set; }

        [JsonPropertyName("Links")]
        public List<Link> Links { get; set; }
    }

    public class CancelResponse
    {
        [JsonPropertyName("Status")]
        public int Status { get; set; }

        [JsonPropertyName("ReasonCode")]
        public int ReasonCode { get; set; }

        [JsonPropertyName("ReasonMessage")]
        public string ReasonMessage { get; set; }

        [JsonPropertyName("ProviderReturnCode")]
        public string ProviderReturnCode { get; set; }

        [JsonPropertyName("ProviderReturnMessage")]
        public string ProviderReturnMessage { get; set; }

        [JsonPropertyName("ReturnCode")]
        public string ReturnCode { get; set; }

        [JsonPropertyName("ReturnMessage")]
        public string ReturnMessage { get; set; }

        [JsonPropertyName("Tid")]
        public string Tid { get; set; }

        [JsonPropertyName("ProofOfSale")]
        public string ProofOfSale { get; set; }

        [JsonPropertyName("AuthorizationCode")]
        public string AuthorizationCode { get; set; }

        [JsonPropertyName("Links")]
        public List<Link> Links { get; set; }
    }

    public class Link
    {
        [JsonPropertyName("Method")]
        public string Method { get; set; }

        [JsonPropertyName("Rel")]
        public string Rel { get; set; }

        [JsonPropertyName("Href")]
        public string Href { get; set; }
    }
}