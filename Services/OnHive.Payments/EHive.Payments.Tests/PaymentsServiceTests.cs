using AutoMapper;
using EHive.Core.Library.Contracts.Common;
using EHive.Core.Library.Contracts.Login;
using EHive.Core.Library.Contracts.Orders;
using EHive.Core.Library.Contracts.Payments;
using EHive.Core.Library.Contracts.Tenants;
using EHive.Core.Library.Contracts.Users;
using EHive.Core.Library.Entities.Payments;
using EHive.Core.Library.Entities.Users;
using EHive.Core.Library.Enums.Orders;
using EHive.Core.Library.Enums.Payments;
using EHive.Events.Domain.Abstractions.Services;
using EHive.Orders.Domain.Abstractions.Services;
using EHive.Payments.Domain.Abstractions.Repositories;
using EHive.Payments.Domain.Exceptions;
using EHive.Payments.Domain.Mappers;
using EHive.Payments.Domain.Models;
using EHive.Payments.Services;
using EHive.Users.Domain.Abstractions.Services;
using FluentAssertions;
using Moq;
using RichardSzalay.MockHttp;
using System.Net;
using System.Text.Json;

namespace EHive.Payments.Tests
{
    public class PaymentServiceTests
    {
        private readonly MockRepository mockRepository;
        private readonly Mock<IPaymentsRepository> mockPaymentRepository;
        private readonly Mock<IEventRegister> mockEventRegister;
        private readonly Mock<IBankSlipNumberControlRepository> mockBankSlipNumberControlRepository;
        private readonly Mock<IBankSlipSettingsRepository> mockBankSlipSettingsRepository;
        private readonly Mock<IOrdersService> mockOrdersService;
        private readonly Mock<IUsersService> mockUsersService;

        private readonly PaymentsApiSettings paymentApiSettings;
        private readonly IMapper mapper;
        private readonly MockHttpMessageHandler mockHttpHandler;
        private readonly HttpClient httpClient;

        public PaymentServiceTests()
        {
            mockRepository = new MockRepository(MockBehavior.Default);

            mockPaymentRepository = mockRepository.Create<IPaymentsRepository>();
            mockEventRegister = mockRepository.Create<IEventRegister>();
            mockBankSlipNumberControlRepository = mockRepository.Create<IBankSlipNumberControlRepository>();
            mockBankSlipSettingsRepository = mockRepository.Create<IBankSlipSettingsRepository>();
            mockOrdersService = mockRepository.Create<IOrdersService>();
            mockUsersService = mockRepository.Create<IUsersService>();
            mapper = new MapperConfiguration(cfg => cfg.AddProfile<MappersConfig>()).CreateMapper();
            mockHttpHandler = new MockHttpMessageHandler();
            httpClient = new HttpClient(mockHttpHandler);
            paymentApiSettings = new PaymentsApiSettings
            {
                Processors = new List<PaymentProcessorSettings>
                {
                    new PaymentProcessorSettings()
                    {
                        Host = "http://localhost:5055",
                        Active = true,
                        Key = "MOCKPAY"
                    }
                }
            };
        }

        [Fact]
        public async Task Cancel_Success()
        {
            // Arrange
            var service = this.CreateService();
            string paymentId = Guid.NewGuid().ToString();
            UserDto? user = GetTestUser();
            string token = "TEST_TOKEN";
            var payment = new Payment
            {
                Id = paymentId,
                ExternalId = "111111",
                OrderId = "123456",
                Status = PaymentStatus.Confirmed,
                ProviderPaymentMessage = "Confirmed",
                ProviderReceiptKey = "111111",
                ProviderKey = "MOCKPAY"
            };

            var expected = new PaymentReceiptDto
            {
                PaymentId = paymentId,
                ExternalId = payment.ExternalId,
                OrderId = "123456",
                Status = PaymentStatus.Cancelled,
                CancellationDate = DateTime.UtcNow,
                ProviderPaymentMessage = "Canceled",
                PaymentToken = payment.ProviderReceiptKey,
                ProviderKey = "MOCKPAY"
            };

            mockHttpHandler.When($"{paymentApiSettings?.Processors?[0].Host}/{paymentApiSettings?.Processors?[0].Version}/Payment/Cancel")
                .Respond("application/json", JsonSerializer.Serialize(expected));

            mockPaymentRepository.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(payment);

            mockOrdersService.Setup(r => r.SetPaymentStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OrderStatus>(), It.IsAny<LoggedUserDto>()));

            mockOrdersService.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<LoggedUserDto>())).ReturnsAsync(new OrderDto { Id = Guid.NewGuid().ToString() });

            mockUsersService.Setup(s => s.GetByIdAsync(It.IsAny<string>(), It.IsAny<LoggedUserDto>())).ReturnsAsync(user);

            // Act
            var result = await service.Cancel(
                paymentId,
                new LoggedUserDto(user, token));

            // Assert
            result?.Should().NotBeNull();
            result?.PaymentToken.Should().Be(expected.PaymentToken);
            result?.ExternalId.Should().Be(expected.ExternalId);
            result?.Status.Should().Be(expected.Status);
            mockPaymentRepository.Verify(r => r.SaveAsync(It.IsAny<Payment>(), string.Empty), Times.Once);
        }

        [Fact]
        public async Task Cancel_Denied()
        {
            // Arrange
            var service = this.CreateService();
            string paymentId = Guid.NewGuid().ToString();
            UserDto? user = GetTestUser();
            string token = "TEST_TOKEN";

            mockPaymentRepository.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(() => null);

            // Act
            var result = await service.Cancel(
                paymentId,
                new LoggedUserDto(user, token));

            // Assert
            result?.Should().BeNull();
            mockPaymentRepository.Verify(r => r.SaveAsync(It.IsAny<Payment>(), string.Empty), Times.Never);
        }

        [Fact]
        public async Task Checkout_Success()
        {
            // Arrange
            var service = this.CreateService();
            string paymentId = Guid.NewGuid().ToString();
            string token = "TEST_TOKEN";

            var paymentCheckout = new PaymentCheckoutDto
            {
                PaymentId = paymentId,
                ExternalId = "111111",
                ProviderKey = "MOCKPAY",
                UserId = "1",
                TenantId = "111111",
                PaymentType = PaymentType.PIX,
                PaymentClient = new PaymentClientDto
                {
                    Name = "test",
                    IdentificationType = "CPF",
                    Identification = "11111111111"
                }
            };

            var expected = new PaymentReceiptDto
            {
                PaymentId = paymentId,
                ExternalId = "111111",
                OrderId = "123456",
                Status = PaymentStatus.Confirmed,
                CancellationDate = DateTime.UtcNow,
                ProviderPaymentMessage = "Confirmed",
                PaymentToken = "111111",
                ProviderKey = "MOCKPAY"
            };

            var paymentClient = new UserDto
            {
                Id = "1",
                Name = "test",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "", IsMain = true } },
                IsActive = true,
                Addresses = new List<AddressDto> { new AddressDto { City = "test", Country = new CountryDto { Code = "BR" }, State = new StateDto { Code = "PE" }, AddressLines = "test", ZipCode = "test", IsMainAddress = true } },
                Documents = new List<UserDocumentDto> { new UserDocumentDto { DocumentType = "CPF", DocumentNumber = "11111111111" } }
            };

            mockPaymentRepository.Setup(r => r.SaveAsync(It.IsAny<Payment>(), It.IsAny<string>())).Returns<Payment?, string>((p, u) => Task.FromResult(p));

            mockHttpHandler.When($"{paymentApiSettings?.Processors?[0].Host}/{paymentApiSettings?.Processors?[0].Version}/Payment/Checkout")
                .Respond("application/json", JsonSerializer.Serialize(expected));

            mockOrdersService.Setup(r => r.SetPaymentStatus(It.IsAny<string>(), It.IsAny<string>(), It.IsAny<OrderStatus>(), It.IsAny<LoggedUserDto>()));

            mockOrdersService.Setup(r => r.GetByIdAsync(It.IsAny<string>(), It.IsAny<LoggedUserDto>())).ReturnsAsync(new OrderDto { Id = Guid.NewGuid().ToString() });

            mockUsersService.Setup(s => s.GetByIdAsync(It.IsAny<string>(), It.IsAny<LoggedUserDto>())).ReturnsAsync(paymentClient);

            // Act
            var result = await service.Checkout(paymentCheckout, new LoggedUserDto(new UserDto(), "TOKEN"));

            // Assert
            result?.Should().NotBeNull();
            result?.PaymentToken.Should().Be(expected.PaymentToken);
            result?.ExternalId.Should().Be(expected.ExternalId);
            result?.Status.Should().Be(PaymentStatus.Confirmed);

            mockPaymentRepository.Verify(r => r.SaveAsync(It.Is<Payment>(p => p.Id == paymentId), string.Empty), Times.Exactly(2));
        }

        [Fact]
        public async Task GetProviders()
        {
            // Arrange
            var service = CreateService();

            var expected = new List<ProviderInfoDto>
            {
                new ProviderInfoDto
                {
                    Key = "MOCKPAY",
                    Name = "MockPay",
                    Description = "mockpay service"
                }
            };

            mockHttpHandler.When($"{paymentApiSettings?.Processors?[0].Host}/{paymentApiSettings?.Processors?[0].Version}/Payment/ProviderInfo")
                .Respond("application/json", JsonSerializer.Serialize(expected[0]));

            // Act
            var result = await service.GetProviders();

            // Assert
            result?.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?[0].Key.Should().Be("MOCKPAY");
        }

        [Fact]
        public async Task Checkout_InvalidProvider()
        {
            // Arrange
            var service = this.CreateService();
            string paymentId = Guid.NewGuid().ToString();
            string token = "TEST_TOKEN";

            var paymentCheckout = new PaymentCheckoutDto
            {
                PaymentId = paymentId,
                ExternalId = "111111",
                ProviderKey = "FAKEPAY",
                UserId = "1",
                TenantId = "111111",
                PaymentType = PaymentType.PIX,
                PaymentClient = new PaymentClientDto
                {
                    Name = "test",
                    IdentificationType = "CPF",
                    Identification = "11111111111"
                }
            };

            var expected = new PaymentReceiptDto
            {
                PaymentId = paymentId,
                ExternalId = "111111",
                Status = PaymentStatus.Confirmed,
                CancellationDate = DateTime.UtcNow,
                ProviderPaymentMessage = "Confirmed",
                PaymentToken = "111111",
                ProviderKey = "MOCKPAY"
            };

            mockPaymentRepository.Setup(r => r.SaveAsync(It.IsAny<Payment>(), It.IsAny<string>())).Returns<Payment?, string>((p, u) => Task.FromResult(p));

            mockHttpHandler.When($"{paymentApiSettings?.Processors?[0].Host}/{paymentApiSettings?.Processors?[0].Version}/Payment/Checkout")
                .Respond("application/json", JsonSerializer.Serialize(expected));

            // Act
            try
            {
                await service.Checkout(paymentCheckout, new LoggedUserDto(new UserDto(), "TOKEN"));

                // Assert
                Assert.Fail("Must throw InvalidPaymentProviderException");
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InvalidPaymentProviderException>("Must be of type InvalidPaymentProviderException");
            }
        }

        [Fact]
        public async Task Checkout_MissingUserInfo()
        {
            // Arrange
            var service = this.CreateService();
            string paymentId = Guid.NewGuid().ToString();
            string token = "TEST_TOKEN";

            var paymentCheckout = new PaymentCheckoutDto
            {
                PaymentId = paymentId,
                ExternalId = "111111",
                ProviderKey = "FAKEPAY",
                UserId = "1",
                TenantId = "111111",
                PaymentType = PaymentType.PIX,
                PaymentClient = new PaymentClientDto
                {
                    Name = "test"
                }
            };

            var expected = new PaymentReceiptDto
            {
                PaymentId = paymentId,
                ExternalId = "111111",
                Status = PaymentStatus.Confirmed,
                CancellationDate = DateTime.UtcNow,
                ProviderPaymentMessage = "Confirmed",
                PaymentToken = "111111",
                ProviderKey = "MOCKPAY"
            };

            mockPaymentRepository.Setup(r => r.SaveAsync(It.IsAny<Payment>(), It.IsAny<string>())).Returns<Payment?, string>((p, u) => Task.FromResult(p));

            mockHttpHandler.When($"{paymentApiSettings?.Processors?[0].Host}/{paymentApiSettings?.Processors?[0].Version}/Payment/Checkout")
                .Respond("application/json", JsonSerializer.Serialize(expected));

            // Act
            try
            {
                await service.Checkout(paymentCheckout, new LoggedUserDto(new UserDto(), "TOKEN"));

                // Assert
                Assert.Fail("Must throw InvalidPaymentException");
            }
            catch (Exception ex)
            {
                ex.Should().BeOfType<InvalidPaymentException>("Must be of type InvalidPaymentException");
            }
        }

        [Fact]
        public async Task GetReceiptsTest()
        {
            // Arrange
            var service = this.CreateService();
            string paymentId = Guid.NewGuid().ToString();
            string orderId = Guid.NewGuid().ToString();

            var payment = new Payment
            {
                Id = paymentId,
                Status = PaymentStatus.Confirmed,
                LastReceipt = new PaymentReceipt
                {
                    PaymentId = paymentId,
                    Status = PaymentStatus.Confirmed,
                    OrderId = orderId,
                    TenantId = "1111"
                }
            };

            mockPaymentRepository.Setup(r => r.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<string>())).ReturnsAsync(new List<Payment> { payment });

            // Act
            var result = await service.GetReceipts(new LoggedUserDto(new UserDto(), "TOKEN"));

            // Assert
            result.Should().NotBeNull();
            result.Should().HaveCount(1);
            result?[0].Status.Should().Be(PaymentStatus.Confirmed);
            mockPaymentRepository.Verify(r => r.GetByUserIdAsync(It.IsAny<string>(), It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public async Task GetReceipt_Exists()
        {
            // Arrange
            var service = this.CreateService();
            string paymentId = Guid.NewGuid().ToString();

            var payment = new Payment
            {
                Id = paymentId,
                Status = PaymentStatus.Confirmed,
                LastReceipt = new PaymentReceipt
                {
                    PaymentId = paymentId,
                    Status = PaymentStatus.Confirmed
                }
            };

            mockPaymentRepository.Setup(r => r.GetByIdAsync(paymentId)).ReturnsAsync(payment);

            // Act
            var result = await service.GetReceipt(paymentId, new LoggedUserDto(new UserDto(), "TOKEN"));

            // Assert
            result.Should().NotBeNull();
            result?.Status.Should().Be(PaymentStatus.Confirmed);
            mockPaymentRepository.Verify(r => r.GetByIdAsync(paymentId), Times.Once);
        }

        [Fact]
        public async Task GetReceipt_Missing()
        {
            // Arrange
            var service = this.CreateService();
            string paymentId = Guid.NewGuid().ToString();

            // Act
            var result = await service.GetReceipt(paymentId, new LoggedUserDto(new UserDto(), "TOKEN"));

            // Assert
            result.Should().BeNull();
            mockPaymentRepository.Verify(r => r.GetByIdAsync(paymentId), Times.Once);
        }

        [Fact]
        public async Task GetReceiptByOrder_Exists()
        {
            // Arrange
            var service = this.CreateService();
            string paymentId = Guid.NewGuid().ToString();
            string orderId = Guid.NewGuid().ToString();

            var payment = new Payment
            {
                Id = paymentId,
                Status = PaymentStatus.Confirmed,
                OrderId = orderId,
                TenantId = "1111",
                LastReceipt = new PaymentReceipt
                {
                    PaymentId = paymentId,
                    Status = PaymentStatus.Confirmed,
                    OrderId = orderId,
                    TenantId = "1111"
                }
            };

            mockPaymentRepository.Setup(r => r.GetByOrderIdAsync(payment.OrderId, payment.TenantId)).ReturnsAsync(payment);

            // Act
            var result = await service.GetReceiptByOrder(payment.OrderId, new LoggedUserDto(new UserDto { TenantId = payment.TenantId }, "TOKEN"));

            // Assert
            result.Should().NotBeNull();
            result?.Status.Should().Be(PaymentStatus.Confirmed);
            result?.PaymentId.Should().Be(paymentId);
            result?.OrderId.Should().Be(payment.OrderId);
            result?.TenantId.Should().Be(payment.TenantId);
            mockPaymentRepository.Verify(r => r.GetByOrderIdAsync(payment.OrderId, payment.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetReceipts_ByTenant()
        {
            // Arrange
            var service = this.CreateService();
            string paymentId = Guid.NewGuid().ToString();
            string orderId = Guid.NewGuid().ToString();

            var payment = new Payment
            {
                Id = paymentId,
                Status = PaymentStatus.Confirmed,
                OrderId = orderId,
                TenantId = "1111",
                LastReceipt = new PaymentReceipt
                {
                    PaymentId = paymentId,
                    Status = PaymentStatus.Confirmed,
                    OrderId = orderId,
                    TenantId = "1111"
                }
            };

            mockPaymentRepository.Setup(r => r.GetAllAsync(payment.TenantId)).ReturnsAsync(new List<Payment> { payment });

            // Act
            var result = await service.GetReceipts(payment.TenantId);

            // Assert
            result.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?[0].Status.Should().Be(PaymentStatus.Confirmed);
            result?[0].PaymentId.Should().Be(paymentId);
            result?[0].OrderId.Should().Be(payment.OrderId);
            result?[0].TenantId.Should().Be(payment.TenantId);
            mockPaymentRepository.Verify(r => r.GetAllAsync(payment.TenantId), Times.Once);
        }

        [Fact]
        public async Task GetReceipts_ByProvider()
        {
            // Arrange
            var service = this.CreateService();
            string paymentId = Guid.NewGuid().ToString();
            string orderId = Guid.NewGuid().ToString();

            var payment = new Payment
            {
                Id = paymentId,
                Status = PaymentStatus.Confirmed,
                OrderId = orderId,
                TenantId = "1111",
                ProviderKey = "MOCKPAY",
                LastReceipt = new PaymentReceipt
                {
                    PaymentId = paymentId,
                    Status = PaymentStatus.Confirmed,
                    OrderId = orderId,
                    TenantId = "1111"
                }
            };

            mockPaymentRepository.Setup(r => r.GetByProviderAsync(payment.ProviderKey, payment.TenantId)).ReturnsAsync(new List<Payment> { payment });

            // Act
            var result = await service.GetReceiptsByProvider(payment.ProviderKey, new LoggedUserDto(new UserDto { TenantId = payment.TenantId }, "TOKEN"));

            // Assert
            result.Should().NotBeNull();
            result?.Should().HaveCount(1);
            result?[0].Status.Should().Be(PaymentStatus.Confirmed);
            result?[0].PaymentId.Should().Be(paymentId);
            result?[0].OrderId.Should().Be(payment.OrderId);
            result?[0].TenantId.Should().Be(payment.TenantId);
            mockPaymentRepository.Verify(r => r.GetByProviderAsync(payment.ProviderKey, payment.TenantId), Times.Once);
        }

        private PaymentsService CreateService()
        {
            return new PaymentsService(
                mockPaymentRepository.Object,
                mockBankSlipNumberControlRepository.Object,
                mockBankSlipSettingsRepository.Object,
                paymentApiSettings,
                mapper,
                httpClient,
                mockEventRegister.Object,
                mockOrdersService.Object,
                mockUsersService.Object);
        }

        private UserDto GetTestUser()
        {
            var tenantId = Guid.NewGuid().ToString();
            return new UserDto
            {
                Id = Guid.NewGuid().ToString(),
                Name = "Test",
                Login = "Test",
                Emails = new List<UserEmailDto> { new UserEmailDto { Email = "Test@Test.com", IsMain = true, IsValidated = true } },
                IsActive = true,
                Roles = ["admin"],
                Permissions = ["admin", "payment_admin"],
                TenantId = tenantId,
                Tenant = new TenantDto
                {
                    Id = tenantId,
                    Domain = "TestCo",
                    Email = "Test@TestCo.com",
                    Name = "TestCo",
                    Features = new List<string> { "homolog" }
                },
            };
        }
    }
}