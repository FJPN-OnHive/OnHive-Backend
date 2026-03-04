using OnHive.Core.Library.Contracts.Payments;
using OnHive.Core.Library.Contracts.Users;

namespace OnHive.PaymentsCielo.Domain.Abstractions.Services
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