using AutoMapper;
using OnHive.Core.Library.Contracts.Common;
using OnHive.Core.Library.Contracts.Login;
using OnHive.Core.Library.Contracts.Orders;
using OnHive.Core.Library.Contracts.Payments;
using OnHive.Core.Library.Contracts.Users;
using OnHive.Core.Library.Domain.Exceptions;
using OnHive.Core.Library.Entities.Payments;
using OnHive.Core.Library.Enums.Orders;
using OnHive.Core.Library.Enums.Payments;
using OnHive.Core.Library.Exceptions;
using OnHive.Core.Library.Entities.Orders;
using OnHive.Events.Domain.Abstractions.Services;
using OnHive.Orders.Domain.Abstractions.Repositories;
using OnHive.Payments.Domain.Abstractions.Repositories;
using OnHive.Payments.Domain.Abstractions.Services;
using OnHive.Payments.Domain.Exceptions;
using OnHive.Payments.Domain.Models;
using OnHive.Users.Domain.Abstractions.Services;
using Serilog;
using System.Data;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace OnHive.Payments.Services
{
    public class PaymentsService : IPaymentsService
    {
        private readonly IPaymentsRepository paymentsRepository;
        private readonly IBankSlipNumberControlRepository bankSlipNumberControlRepository;
        private readonly IBankSlipSettingsRepository bankSlipSettingsRepository;
        private readonly IOrdersRepository ordersRepository;
        private readonly IUsersService usersService;
        private readonly PaymentsApiSettings paymentsApiSettings;
        private readonly IMapper mapper;
        private readonly ILogger logger;
        private readonly HttpClient httpClient;
        private readonly IEventRegister eventRegister;

        public PaymentsService(IPaymentsRepository paymentsRepository,
                               IBankSlipNumberControlRepository bankSlipNumberControlRepository,
                               IBankSlipSettingsRepository bankSlipSettingsRepository,
                               PaymentsApiSettings paymentsApiSettings,
                               IMapper mapper,
                               HttpClient httpClient,
                               IEventRegister eventRegister,
                               IOrdersRepository ordersRepository,
                               IUsersService usersService)
        {
            this.paymentsRepository = paymentsRepository;
            this.bankSlipNumberControlRepository = bankSlipNumberControlRepository;
            this.bankSlipSettingsRepository = bankSlipSettingsRepository;
            this.paymentsApiSettings = paymentsApiSettings;
            this.ordersRepository = ordersRepository;
            this.usersService = usersService;
            this.mapper = mapper;
            this.httpClient = httpClient;
            this.eventRegister = eventRegister;
            logger = Log.Logger;
        }

        public async Task<PaymentReceiptDto?> Cancel(string paymentId, LoggedUserDto? loggedUser)
        {
            var payment = await paymentsRepository.GetByIdAsync(paymentId);
            if (payment == null) return null;
            var provider = ValidateProvider(payment.ProviderKey);

            logger.Information("Received payment cancellation for user/tenant: {user}/{tenant} = id {id}", loggedUser.User?.Login, loggedUser.User?.TenantId, paymentId);

            var content = new StringContent(JsonSerializer.Serialize(payment.LastReceipt), UnicodeEncoding.UTF8, "application/json");
            var response = await httpClient.PutAsync($"{provider.Host}/{provider.Version}/Payment/Cancel", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidPaymentException($"Error processing payment cancellation: {response.ReasonPhrase}");
            }

            var result = JsonSerializer.Deserialize<PaymentReceiptDto>(await response.Content.ReadAsStringAsync());
            payment.LastReceipt = mapper.Map<PaymentReceipt>(result);

            await UpdatePayment(payment);

            await UpdateOrder(payment, loggedUser);

            logger.Information("Payment canceled with status {status} for user/tenant: {user}/{tenant} = id {id}", payment?.Status.ToString(), loggedUser.User?.Login, loggedUser.User?.TenantId, paymentId);

            var order = await GetOrder(payment.OrderId, loggedUser);
            var client = await GetClient(payment.UserId, loggedUser);
            Register(payment, client, order, EventKeys.PaymentCanceled, "Payment cancelled");

            return result;
        }

        public async Task<PaymentReceiptDto?> Checkout(PaymentCheckoutDto paymentCheckout, LoggedUserDto? loggedUser)
        {
            ValidateCheckout(paymentCheckout);
            var provider = ValidateProvider(paymentCheckout.ProviderKey);
            paymentCheckout = await ValidateOrder(paymentCheckout, loggedUser);
            paymentCheckout = await ValidateClient(paymentCheckout, loggedUser);
            var payment = await StorePayment(paymentCheckout);
            if (payment == null) return null;
            paymentCheckout.PaymentId = payment.Id;

            if (payment.PaymentType == PaymentType.BankSlip)
            {
                paymentCheckout = await GetBankSlipInfo(paymentCheckout, provider, payment);
            }
            logger.Information("Received payment for user/tenant: {user}/{tenant}, and provider {provider} = id {id}", paymentCheckout.UserId, paymentCheckout.TenantId, provider.Key, payment?.Id);
            var content = new StringContent(JsonSerializer.Serialize(paymentCheckout), UnicodeEncoding.UTF8, "application/json");
            httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue(loggedUser.Token);
            var response = await httpClient.PostAsync($"{provider.Host}/{provider.Version}/Payment/Checkout", content);
            if (!response.IsSuccessStatusCode)
            {
                throw new InvalidPaymentException($"Error processing payment: {response.ReasonPhrase}");
            }

            var result = JsonSerializer.Deserialize<PaymentReceiptDto>(await response.Content.ReadAsStringAsync());
            payment.LastReceipt = mapper.Map<PaymentReceipt>(result);

            payment = await UpdatePayment(payment);

            await UpdateOrder(payment, loggedUser);

            logger.Information("Payment concluded with status {status} for user/tenant: {user}/{tenant}, and provider {provider} = id {id}", payment?.Status.ToString(), paymentCheckout.UserId, paymentCheckout.TenantId, provider.Key, payment?.Id);

            var order = await GetOrder(payment.OrderId, loggedUser);
            var client = await GetClient(payment.UserId, loggedUser);
            Register(payment, client, order, EventKeys.Checkout, "Checkout");

            return result;
        }

        private async Task<PaymentCheckoutDto> GetBankSlipInfo(PaymentCheckoutDto paymentCheckout, PaymentProcessorSettings provider, Payment? payment)
        {
            var bankSlipSettings = await bankSlipSettingsRepository.GetByProviderAsync(provider.BankSlipProviderKey ?? throw new NotFoundException("BankSlipProviderKey not found"))
                    ?? throw new NotFoundException($"BankSlipSettings not found for provider: {provider.BankSlipProviderKey}");
            paymentCheckout.BankSlipInfo = new BankSlipInfoDto
            {
                Provider = bankSlipSettings.Provider,
                Assignor = bankSlipSettings.Assignor,
                Demonstrative = bankSlipSettings.Demonstrative,
                Instructions = bankSlipSettings.Instructions,
                ExpirationDate = DateTime.UtcNow.AddDays(bankSlipSettings.ExpirationsDays),
                Number = (await bankSlipNumberControlRepository.GetNextAsync(provider.Key, payment.OrderId, payment.Id)).ToString()
            };
            return paymentCheckout;
        }

        private async Task UpdateOrder(Payment? payment, LoggedUserDto loggedUser)
        {
            if (payment?.LastReceipt == null) return;
            var order = await ordersRepository.GetByIdAsync(payment.OrderId) ?? throw new NotFoundException($"Order not found: {payment.OrderId}");
            var status = OrderStatus.PaymentProcessing;
            switch (payment.LastReceipt.Status)
            {
                case Core.Library.Enums.Payments.PaymentStatus.Confirmed:
                    status = OrderStatus.Closed;
                    break;

                case Core.Library.Enums.Payments.PaymentStatus.Cancelled:
                    status = OrderStatus.Cancelled;
                    break;

                case Core.Library.Enums.Payments.PaymentStatus.Refounded:
                    status = OrderStatus.Refounded;
                    break;

                case Core.Library.Enums.Payments.PaymentStatus.Refused:
                    status = OrderStatus.PaymentRefused;
                    break;
            }
            order.Status = status;
            order.PaymentId = payment.Id;
            switch (status)
            {
                case OrderStatus.Closed:
                    order.ClosingDate = DateTime.UtcNow;
                    break;
                case OrderStatus.Cancelled:
                    order.CancellationDate = DateTime.UtcNow;
                    break;
                case OrderStatus.Refounded:
                    order.RefoundDate = DateTime.UtcNow;
                    break;
            }
            await ordersRepository.SaveAsync(order);
        }

        private async Task<PaymentCheckoutDto> ValidateOrder(PaymentCheckoutDto paymentCheckout, LoggedUserDto loggedUser)
        {
            var order = await GetOrder(paymentCheckout.OrderId, loggedUser);
            if (order?.Status != OrderStatus.Pending)
            {
                throw new InvalidPayloadException(new List<string> { $"Invalid Order Status: {paymentCheckout.OrderId}, Order status must be Pending" });
            }
            paymentCheckout.Value = order.TotalValue;
            paymentCheckout.OrderNumber = order.Code;

            return paymentCheckout;
        }

        private async Task<PaymentCheckoutDto> ValidateClient(PaymentCheckoutDto paymentCheckout, LoggedUserDto loggedUser)
        {
            var user = await GetClient(paymentCheckout.UserId, loggedUser);
            if (user == null)
            {
                throw new NotFoundException($"Client not found: {paymentCheckout.UserId}");
            }
            if (!user.Addresses.Any(a => a.IsMainAddress))
            {
                throw new NotFoundException($"Client main address not found: {paymentCheckout.UserId}");
            }
            if (user.BirthDate == DateTime.MinValue)
            {
                throw new NotFoundException($"Client birtdate not set: {paymentCheckout.UserId}");
            }
            if (user.Documents == null || !user.Documents.Any() || string.IsNullOrEmpty(user.Documents.FirstOrDefault().DocumentNumber)
                || string.IsNullOrEmpty(user.Documents.FirstOrDefault().DocumentType))
            {
                throw new NotFoundException($"Client document not set: {paymentCheckout.UserId}");
            }
            paymentCheckout.PaymentClient = new PaymentClientDto
            {
                Name = user.Name ?? string.Empty,
                Identification = user.Documents?.FirstOrDefault()?.DocumentNumber ?? string.Empty,
                IdentificationType = user.Documents?.FirstOrDefault()?.DocumentType ?? string.Empty,
                BirthDate = user.BirthDate.ToString() ?? DateTime.MinValue.ToString(),
                Email = user.MainEmail ?? string.Empty,
                PhoneNumber = user.PhoneNumber ?? string.Empty,
                Address = user.Addresses.First(a => a.IsMainAddress)
            };

            return paymentCheckout;
        }

        public async Task<List<ProviderInfoDto>> GetProviders()
        {
            var result = new List<ProviderInfoDto>();
            if (paymentsApiSettings.Processors == null) return result;

            foreach (var provider in paymentsApiSettings.Processors)
            {
                var providerInfo = await GetProviderInfo(provider);
                if (providerInfo == null) continue;
                result.Add(providerInfo);
            }

            return result;
        }

        public async Task<PaymentReceiptDto?> GetReceipt(string paymentId, LoggedUserDto? loggedUser)
        {
            var payment = await paymentsRepository.GetByIdAsync(paymentId);
            if (payment?.LastReceipt == null) return null;
            var result = mapper.Map<PaymentReceiptDto>(payment.LastReceipt);
            return result;
        }

        public async Task<PaymentReceiptDto?> GetReceiptByOrder(string orderId, LoggedUserDto? loggedUser)
        {
            var payment = await paymentsRepository.GetByOrderIdAsync(orderId, loggedUser.User?.TenantId ?? "");
            if (payment?.LastReceipt == null) return null;
            var result = mapper.Map<PaymentReceiptDto>(payment.LastReceipt);
            return result;
        }

        public async Task<List<PaymentReceiptDto>> GetReceipts(LoggedUserDto? loggedUser)
        {
            var payments = await paymentsRepository.GetByUserIdAsync(loggedUser.User?.Id ?? "", loggedUser.User?.TenantId ?? "");
            var result = mapper.Map<List<PaymentReceiptDto>>(payments.Select(p => p.LastReceipt));
            return result;
        }

        public async Task<List<PaymentReceiptDto>> GetReceipts(string tenantId)
        {
            var payments = await paymentsRepository.GetAllAsync(tenantId);
            var result = mapper.Map<List<PaymentReceiptDto>>(payments.Select(p => p.LastReceipt));
            return result;
        }

        public async Task<List<PaymentReceiptDto>> GetReceiptsByProvider(string ProviderId, LoggedUserDto? loggedUser)
        {
            var payments = await paymentsRepository.GetByProviderAsync(ProviderId, loggedUser.User?.TenantId ?? "");
            var result = mapper.Map<List<PaymentReceiptDto>>(payments.Select(p => p.LastReceipt));
            return result;
        }

        private async Task<OrderDto?> GetOrder(string orderId, LoggedUserDto loggedUser)
        {
            var order = await ordersRepository.GetByIdAsync(orderId) ?? throw new NotFoundException($"Order not found: {orderId}");
            return mapper.Map<OrderDto>(order);
        }

        private async Task<UserDto?> GetClient(string userId, LoggedUserDto loggedUser)
        {
            return await usersService.GetByIdAsync(userId, loggedUser) ?? throw new NotFoundException($"Client not found: {userId}");
        }

        private void ValidateCheckout(PaymentCheckoutDto paymentCheckout)
        {
            if (string.IsNullOrEmpty(paymentCheckout.TenantId))
            {
                throw new InvalidPaymentException("Missing TenantId");
            }

            if (string.IsNullOrEmpty(paymentCheckout.UserId))
            {
                throw new InvalidPaymentException("Missing UserId");
            }

            if (paymentCheckout.PaymentClient == null
                || string.IsNullOrEmpty(paymentCheckout.PaymentClient.Name)
                || string.IsNullOrEmpty(paymentCheckout.PaymentClient.Identification)
                || string.IsNullOrEmpty(paymentCheckout.PaymentClient.IdentificationType))
            {
                throw new InvalidPaymentException("Missing Client informations");
            }
        }

        private async Task<ProviderInfoDto?> GetProviderInfo(PaymentProcessorSettings provider)
        {
            try
            {
                var response = await httpClient.GetAsync($"{provider.Host}/{provider.Version}/Payment/ProviderInfo");
                response.EnsureSuccessStatusCode();
                var item = JsonSerializer.Deserialize<ProviderInfoDto>(await response.Content.ReadAsStringAsync(), new JsonSerializerOptions { PropertyNameCaseInsensitive = false });
                return item;
            }
            catch (Exception ex)
            {
                logger.Error("Error getting provider info {provider}", provider.Key, ex);
            }
            return null;
        }

        private async Task<Payment?> StorePayment(PaymentCheckoutDto paymentCheckout)
        {
            var payment = mapper.Map<Payment>(paymentCheckout);
            payment = await paymentsRepository.SaveAsync(payment);
            if (payment == null) throw new DataException("Database unknown error");
            return payment;
        }

        private PaymentProcessorSettings ValidateProvider(string? ProviderKey)
        {
            var provider = paymentsApiSettings?.Processors?.Find(p => p.Key?.Trim().ToUpper() == ProviderKey?.Trim().ToUpper());
            if (provider == null)
            {
                throw new InvalidPaymentProviderException(ProviderKey);
            }

            return provider;
        }

        private async Task<Payment?> UpdatePayment(Payment? payment)
        {
            if (payment == null) return payment;
            switch (payment.LastReceipt?.Status)
            {
                case Core.Library.Enums.Payments.PaymentStatus.Confirmed:
                    payment.ConfirmationDate = DateTime.UtcNow;
                    payment.CheckoutDate = DateTime.UtcNow;
                    payment.PaymentValue = payment.LastReceipt.PaymentValue;
                    payment.ProviderPaymentMessage = payment.LastReceipt.ProviderPaymentMessage;
                    payment.ExternalId = payment.LastReceipt.ExternalId;
                    payment.PaymentToken = payment.LastReceipt.PaymentToken;
                    break;

                case Core.Library.Enums.Payments.PaymentStatus.Cancelled:
                    payment.CancellationDate = DateTime.UtcNow;
                    payment.PaymentValue = payment.LastReceipt.PaymentValue;
                    payment.ProviderPaymentMessage = payment.LastReceipt.ProviderPaymentMessage;
                    payment.ExternalId = payment.LastReceipt.ExternalId;
                    payment.PaymentToken = payment.LastReceipt.PaymentToken;
                    break;

                case Core.Library.Enums.Payments.PaymentStatus.Refounded:
                    payment.RefoundDate = DateTime.UtcNow;
                    payment.PaymentValue = payment.LastReceipt.PaymentValue;
                    payment.ProviderPaymentMessage = payment.LastReceipt.ProviderPaymentMessage;
                    payment.ExternalId = payment.LastReceipt.ExternalId;
                    payment.PaymentToken = payment.LastReceipt.PaymentToken;
                    break;

                default:
                    break;
            }

            payment.Status = payment.LastReceipt?.Status ?? Core.Library.Enums.Payments.PaymentStatus.Pending;
            payment.PaymentValue = payment.LastReceipt?.PaymentValue ?? 0;
            payment.ProviderPaymentMessage = payment.LastReceipt?.ProviderPaymentMessage ?? string.Empty;

            return await paymentsRepository.SaveAsync(payment);
        }

        private void Register(Payment payment, UserDto user, OrderDto order, string key, string message)
        {
            _ = eventRegister.RegisterEvent(payment.TenantId, payment.UserId, key, message, new Dictionary<string, string>
            {
                { "PaymentId", payment.Id },
                { "PaymentExternalId", payment.ExternalId },
                { "PaymentCode", payment.PaymentToken },
                { "PaymentStatus", payment.Status.ToString() },
                { "OrderId", payment.OrderId },
                { "PaymentType", payment.PaymentType.ToString() },
                { "ClientId", payment.UserId },
                { "ClientName", user.Name },
                { "ClientEmail", user.MainEmail },
                { "Value", payment.PaymentValue.ToString() },
                { "ItemName", string.Join(",", order.Itens.Select(i => i.ProductName))},
                { "ItemId", string.Join(",", order.Itens.Select(i => i.ProductId)) },
                { "ItemCode", string.Join(",", order.Itens.Select(i => i.ExternalId)) }
            });
        }
    }
}