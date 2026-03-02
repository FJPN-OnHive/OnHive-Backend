using EHive.Core.Library.Entities.Users;
using EHive.Core.Library.Enums.Invoices;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Contracts.Invoices
{
    public class InvoiceDto
    {
        [JsonPropertyName("id")]
        [MaxLength(256)]
        public string Id { get; set; } = string.Empty;

        [JsonPropertyName("userId")]
        [MaxLength(256)]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("tenantId")]
        [MaxLength(256)]
        public string TenantId { get; set; } = string.Empty;

        [JsonPropertyName("orderId")]
        public string OrderId { get; set; } = string.Empty;

        [JsonPropertyName("paymentId")]
        public string PaymentId { get; set; } = string.Empty;

        [JsonPropertyName("authorizationCode")]
        public string AuthorizationCode { get; set; } = string.Empty;

        [JsonPropertyName("externalId")]
        public string ExternalId { get; set; } = string.Empty;

        [JsonPropertyName("status")]
        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        [JsonPropertyName("number")]
        public int Number { get; set; } = 0;

        [JsonPropertyName("series")]
        public string Series { get; set; } = string.Empty;

        [JsonPropertyName("rps")]
        public string RPS { get; set; } = string.Empty;

        [JsonPropertyName("emissionDate")]
        public DateTime EmissionDate { get; set; }

        [JsonPropertyName("cancellationDate")]
        public DateTime CancellationDate { get; set; }

        [JsonPropertyName("totalValue")]
        public double TotalValue { get; set; } = 0.0;

        [JsonPropertyName("discount")]
        public double Discount { get; set; } = 0.0;

        [JsonPropertyName("interest")]
        public double Interest { get; set; } = 0.0;

        [JsonPropertyName("invoiceUrl")]
        public string InvoiceUrl { get; set; } = string.Empty;

        [JsonPropertyName("xmlUrl")]
        public string XmlUrl { get; set; } = string.Empty;

        [JsonPropertyName("emitter")]
        public InvoiceEmitterDto Emitter { get; set; } = new();

        [JsonPropertyName("client")]
        public InvoiceClientDto Client { get; set; } = new();

        [JsonPropertyName("itens")]
        public List<InvoiceItensDto> Itens { get; set; } = new();

        [JsonPropertyName("provider")]
        public string Provider { get; set; } = string.Empty;

        [JsonPropertyName("message")]
        public string Message { get; set; } = string.Empty;

        [JsonPropertyName("token")]
        public string Token { get; set; } = string.Empty;
    }

    public class InvoiceItensDto
    {
        [JsonPropertyName("productId")]
        public string ProductId { get; set; } = string.Empty;

        [JsonPropertyName("description")]
        public string Description { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        public double Quantity { get; set; } = 0.0;

        [JsonPropertyName("unitValue")]
        public double UnitValue { get; set; } = 0.0;

        [JsonPropertyName("totalValue")]
        public double TotalValue { get; set; } = 0.0;
    }

    public class InvoiceEmitterDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("cnpj")]
        public string CNPJ { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public string Address { get; set; } = string.Empty;

        [JsonPropertyName("city")]
        public string City { get; set; } = string.Empty;

        [JsonPropertyName("district")]
        public string District { get; set; } = string.Empty;

        [JsonPropertyName("state")]
        public string State { get; set; } = string.Empty;

        [JsonPropertyName("postalCode")]
        public string PostalCode { get; set; } = string.Empty;

        [JsonPropertyName("country")]
        public string Country { get; set; } = string.Empty;

        [JsonPropertyName("stateInscription")]
        public string StateInscription { get; set; } = string.Empty;

        [JsonPropertyName("cityInscription")]
        public string CityInscription { get; set; } = string.Empty;
    }

    public class InvoiceClientDto
    {
        [JsonPropertyName("name")]
        public string Name { get; set; } = string.Empty;

        [JsonPropertyName("document")]
        public string Document { get; set; } = string.Empty;

        [JsonPropertyName("documentType")]
        public string DocumentType { get; set; } = string.Empty;

        [JsonPropertyName("address")]
        public Address Address { get; set; } = new();

        [JsonPropertyName("email")]
        public string Email { get; set; } = string.Empty;

        [JsonPropertyName("phone")]
        public string Phone { get; set; } = string.Empty;
    }
}