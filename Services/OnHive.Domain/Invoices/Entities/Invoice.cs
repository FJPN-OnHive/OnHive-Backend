using EHive.Core.Library.Entities.Tenants;
using EHive.Core.Library.Entities.Users;
using EHive.Core.Library.Enums.Invoices;
using MongoDB.Bson.Serialization.Attributes;
using System.Text.Json.Serialization;

namespace EHive.Core.Library.Entities.Invoices
{
    [BsonIgnoreExtraElements]
    public class Invoice : EntityBase
    {
        public string UserId { get; set; } = string.Empty;

        public string OrderId { get; set; } = string.Empty;

        public string PaymentId { get; set; } = string.Empty;

        public string AuthorizationCode { get; set; } = string.Empty;

        public string ExternalId { get; set; } = string.Empty;

        public InvoiceStatus Status { get; set; } = InvoiceStatus.Pending;

        public int Number { get; set; } = 0;

        public string Series { get; set; } = string.Empty;

        public string RPS { get; set; } = string.Empty;

        public DateTime EmissionDate { get; set; }

        public DateTime CancellationDate { get; set; }

        public string InvoiceUrl { get; set; } = string.Empty;

        public string XmlUrl { get; set; } = string.Empty;

        public double TotalValue { get; set; } = 0.0;

        public double Discount { get; set; } = 0.0;

        public double Interest { get; set; } = 0.0;

        public InvoiceEmitter Emitter { get; set; } = new();

        public InvoiceClient Client { get; set; } = new();

        public List<InvoiceItens> Itens { get; set; } = new();

        public string Provider { get; set; } = string.Empty;

        public string Message { get; set; } = string.Empty;

        public string Token { get; set; } = string.Empty;
    }

    public class InvoiceItens
    {
        public string ProductId { get; set; } = string.Empty;

        public string Description { get; set; } = string.Empty;

        public double Quantity { get; set; } = 0.0;

        public double UnitValue { get; set; } = 0.0;

        public double TotalValue { get; set; } = 0.0;
    }

    public class InvoiceEmitter
    {
        public string Name { get; set; } = string.Empty;

        public string CNPJ { get; set; } = string.Empty;

        public string Address { get; set; } = string.Empty;

        public string District { get; set; } = string.Empty;

        public string City { get; set; } = string.Empty;

        public string State { get; set; } = string.Empty;

        public string PostalCode { get; set; } = string.Empty;

        public string Country { get; set; } = string.Empty;

        public string StateInscription { get; set; } = string.Empty;

        public string CityInscription { get; set; } = string.Empty;
    }

    public class InvoiceClient
    {
        public string Name { get; set; } = string.Empty;

        public string Document { get; set; } = string.Empty;

        public string DocumentType { get; set; } = string.Empty;

        public Address Address { get; set; } = new();

        public string Email { get; set; } = string.Empty;

        public string Phone { get; set; } = string.Empty;
    }
}