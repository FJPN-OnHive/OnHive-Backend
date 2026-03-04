using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Payments;

namespace OnHive.Payments.Domain.Abstractions.Services
{
    public interface IPaymentsService
    {
        Task<List<ProviderInfoDto>> GetProviders();

        Task<PaymentReceiptDto?> Checkout(PaymentCheckoutDto paymentCheckout, LoggedUserDto? user);

        Task<PaymentReceiptDto?> Cancel(string paymentId, LoggedUserDto? user);

        Task<PaymentReceiptDto?> GetReceipt(string paymentId, LoggedUserDto? user);

        Task<PaymentReceiptDto?> GetReceiptByOrder(string orderId, LoggedUserDto? user);

        Task<List<PaymentReceiptDto>> GetReceipts(LoggedUserDto? user);

        Task<List<PaymentReceiptDto>> GetReceipts(string tenantId);

        Task<List<PaymentReceiptDto>> GetReceiptsByProvider(string ProviderId, LoggedUserDto? user);
    }
}