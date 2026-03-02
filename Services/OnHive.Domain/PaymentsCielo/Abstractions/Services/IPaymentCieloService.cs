using EHive.Core.Library.Contracts.Payments;
using EHive.Core.Library.Contracts.Users;

namespace EHive.PaymentsCielo.Domain.Abstractions.Services
{
    public interface IPaymentCieloService
    {
        Task<ProviderInfoDto> GetProviderInfoAsync();

        Task<PaymentReceiptDto> CheckoutAsync(PaymentCheckoutDto paymentCheckout);

        Task<List<PaymentTypeDto>> GetPaymentTypesAsync();

        Task<PaymentReceiptDto?> CancelPaymentAsync(PaymentReceiptDto paymentReceipt);

        Task<PaymentReceiptDto?> GetPaymentAsync(PaymentReceiptDto paymentReceipt);
    }
}