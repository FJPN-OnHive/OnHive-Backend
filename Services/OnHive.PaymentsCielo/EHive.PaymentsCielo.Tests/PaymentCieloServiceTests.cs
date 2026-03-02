using EHive.Core.Library.Contracts.Payments;
using EHive.Core.Library.Entities.Payments;
using EHive.Core.Library.Enums.Payments;
using EHive.PaymentsCielo.Domain.Contracts;
using EHive.PaymentsCielo.Domain.Models;
using EHive.PaymentsCielo.Services;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;

namespace EHive.PaymentsCielo.Tests
{
    public class PaymentCieloServiceTests
    {
        private readonly MockRepository mockRepository;

        private readonly PaymentCieloSettings cieloPaymentSettings;

        private readonly MockHttpMessageHandler mockHttpMessageHandler;

        private readonly HttpClient httpClient;

        public PaymentCieloServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);
            mockHttpMessageHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpMessageHandler);
            cieloPaymentSettings = new PaymentCieloSettings
            {
                MerchantId = "111111",
                MerchantKey = "111111",
                QueryUrl = "https://apiquerysandbox.cieloecommerce.cielo.com.br/1/sales/",
                TransactionalUrl = "https://apiquerysandbox.cieloecommerce.cielo.com.br/1/sales/"
            };
        }

        [Fact]
        public async Task CancelPaymentAsync_Success()
        {
            // Arrange
            var service = CreateService();
            string paymentId = Guid.NewGuid().ToString();
            string tenantId = "111111";
            var transaction = new MockPaymentTransaction
            {
                Id = paymentId,
                PaymentId = paymentId,
                TenantId = tenantId,
                Receipt = new PaymentReceiptDto
                {
                    PaymentId = paymentId,
                    ExternalId = Guid.NewGuid().ToString(),
                    Status = PaymentStatus.Confirmed
                }
            };

            var cancelResponse = new CancelResponse
            {
                AuthorizationCode = "123456",
                ReturnCode = "0",
                ReasonMessage = "Successful",
                Tid = Guid.NewGuid().ToString(),
                ReturnMessage = "Successful",
                ProviderReturnCode = "0",
                ProviderReturnMessage = "Successful",
                Status = 10,
                Links = new List<Link>
                {
                    new Link
                    {
                        Href = $"{cieloPaymentSettings.TransactionalUrl}/{transaction.Receipt.ExternalId}/void",
                    }
                }
            };

            mockHttpMessageHandler.When($"{cieloPaymentSettings.TransactionalUrl}/{transaction.Receipt.ExternalId}/void")
                .Respond("application/json", JsonSerializer.Serialize(cancelResponse));

            // Act
            var result = await service.CancelPaymentAsync(transaction.Receipt);

            // Assert
            result.Should().NotBeNull();
            result?.Status.Should().Be(PaymentStatus.Cancelled);
            result?.PaymentId.Should().Be(paymentId);
        }

        [Fact]
        public async Task CancelPaymentAsync_NotFound()
        {
            // Arrange
            var service = CreateService();
            string paymentId = Guid.NewGuid().ToString();

            mockHttpMessageHandler.When($"{cieloPaymentSettings.TransactionalUrl}/{paymentId}/void")
                .Respond(HttpStatusCode.NotFound, new StringContent(string.Empty));

            // Act
            try
            {
                _ = await service.CancelPaymentAsync(new PaymentReceiptDto { ExternalId = paymentId });
                Assert.Fail("Should raise Exception");
            }
            catch (Exception ex)
            {
                // Assert
                ex.Should().BeOfType<HttpRequestException>();
            }
        }

        [Fact]
        public async Task CheckoutAsync_Confirmed()
        {
            // Arrange
            var service = CreateService();
            string tenantId = "111111";
            var paymentCheckout = new PaymentCheckoutDto
            {
                TenantId = tenantId,
                PaymentType = PaymentType.CreditCard,
                PaymentTypeId = "1",
                Value = 100.00f,
                CardInfo = new PaymentCardDto
                {
                    CardNumber = "4235647728025682",
                    SecurityCode = "123",
                    ExpirationDate = "11/25",
                    CardType = "Credit",
                    CardHolder = new PaymentClientDto
                    {
                        Name = "Card Holder",
                        IdentificationType = "CPF",
                        Identification = "11111111111"
                    }
                }
            };

            var checkoutResponse = new CieloCreditCardResponse
            {
                Customer = new Customer
                {
                    Name = "Card Holder",
                    Identity = "11111111111",
                    IdentityType = "CPF"
                },
                MerchantOrderId = Guid.NewGuid().ToString(),
                Payment = new CreditCardResponse
                {
                    Amount = 10000,
                    Installments = 1,
                    Type = "CreditCard",
                    Capture = true,
                    Authenticate = false,
                    PaymentId = Guid.NewGuid().ToString(),
                    Status = 1,
                    CreditCard = new CreditCard
                    {
                        CardNumber = "4235647728025682",
                        Holder = "Card Holder",
                        ExpirationDate = "11/25",
                        SecurityCode = "123",
                        Brand = "Visa"
                    }
                },
            };

            mockHttpMessageHandler.When($"{cieloPaymentSettings.TransactionalUrl}")
                .Respond("application/json", JsonSerializer.Serialize(checkoutResponse));

            // Act
            var result = await service.CheckoutAsync(paymentCheckout);

            // Assert
            result.Should().NotBeNull();
            result?.Status.Should().Be(PaymentStatus.Confirmed);
        }

        [Fact]
        public async Task CheckoutAsync_Refused()
        {
            // Arrange
            var service = CreateService();
            string tenantId = "111111";
            var paymentCheckout = new PaymentCheckoutDto
            {
                TenantId = tenantId,
                PaymentType = PaymentType.CreditCard,
                PaymentTypeId = "1",
                Value = 100.00f,
                CardInfo = new PaymentCardDto
                {
                    CardNumber = "4235647728025682",
                    SecurityCode = "123",
                    ExpirationDate = "11/25",
                    CardType = "Credit",
                    CardHolder = new PaymentClientDto
                    {
                        Name = "Card Holder",
                        IdentificationType = "CPF",
                        Identification = "11111111111"
                    }
                }
            };

            var checkoutResponse = new CieloCreditCardResponse
            {
                Customer = new Customer
                {
                    Name = "Card Holder",
                    Identity = "11111111111",
                    IdentityType = "CPF"
                },
                MerchantOrderId = Guid.NewGuid().ToString(),
                Payment = new CreditCardResponse
                {
                    Amount = 10000,
                    Installments = 1,
                    Type = "CreditCard",
                    Capture = true,
                    Authenticate = false,
                    PaymentId = Guid.NewGuid().ToString(),
                    Status = 3,
                    CreditCard = new CreditCard
                    {
                        CardNumber = "4235647728025682",
                        Holder = "Card Holder",
                        ExpirationDate = "11/25",
                        SecurityCode = "123",
                        Brand = "Visa"
                    }
                },
            };

            mockHttpMessageHandler.When($"{cieloPaymentSettings.TransactionalUrl}")
                .Respond("application/json", JsonSerializer.Serialize(checkoutResponse));

            // Act
            var result = await service.CheckoutAsync(paymentCheckout);

            // Assert
            result.Should().NotBeNull();
            result?.Status.Should().Be(PaymentStatus.Refused);
        }

        [Fact]
        public async Task GetPaymentAsync()
        {
            // Arrange
            var service = CreateService();
            string paymentId = Guid.NewGuid().ToString();
            string tenantId = "111111";
            var transaction = new MockPaymentTransaction
            {
                Id = paymentId,
                PaymentId = paymentId,
                TenantId = tenantId,
                Receipt = new PaymentReceiptDto
                {
                    PaymentId = paymentId,
                    ExternalId = Guid.NewGuid().ToString(),
                    Status = PaymentStatus.Confirmed
                }
            };

            var paymentResponse = new CieloCreditCardResponse
            {
                Customer = new Customer
                {
                    Name = "Card Holder",
                    Identity = "11111111111",
                    IdentityType = "CPF"
                },
                MerchantOrderId = Guid.NewGuid().ToString(),
                Payment = new CreditCardResponse
                {
                    Amount = 10000,
                    Installments = 1,
                    Type = "CreditCard",
                    Capture = true,
                    Authenticate = false,
                    PaymentId = Guid.NewGuid().ToString(),
                    Status = 1,
                    IsSplitted = false,
                    CreditCard = new CreditCard
                    {
                        CardNumber = "4235647728025682",
                        Holder = "Card Holder",
                        ExpirationDate = "11/25",
                        SecurityCode = "123",
                        Brand = "Visa"
                    }
                },
            };

            mockHttpMessageHandler.When($"{cieloPaymentSettings.TransactionalUrl}/{transaction.Receipt.ExternalId}")
              .Respond("application/json", JsonSerializer.Serialize(paymentResponse));

            // Act
            var result = await service.GetPaymentAsync(transaction.Receipt);

            // Assert
            result.Should().NotBeNull();
            result?.Status.Should().Be(PaymentStatus.Confirmed);
        }

        [Fact]
        public async Task GetPaymentTypesAsync()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.GetPaymentTypesAsync();

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(3);
        }

        [Fact]
        public async Task GetProviderInfoAsync()
        {
            // Arrange
            var service = CreateService();

            // Act
            var result = await service.GetProviderInfoAsync();

            // Assert
            result.Should().NotBeNull();
        }

        private PaymentCieloService CreateService()
        {
            return new PaymentCieloService(
                cieloPaymentSettings,
                httpClient);
        }
    }
}