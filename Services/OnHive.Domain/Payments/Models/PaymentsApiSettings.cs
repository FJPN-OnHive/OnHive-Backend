using System.ComponentModel.DataAnnotations;

namespace OnHive.Payments.Domain.Models
{
    public class PaymentsApiSettings
    {
        public string? PaymentsAdminPermission { get; set; } = "payments_admin";

        public List<PaymentProcessorSettings>? Processors { get; set; } = [];
    }

    public class PaymentProcessorSettings
    {
        public string? Key { get; set; }

        public string? Host { get; set; }

        public string? BankSlipProviderKey { get; set; }

        public bool Active { get; set; }

        public string Version { get; set; } = "v1";
    }
}